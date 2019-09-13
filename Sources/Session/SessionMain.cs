﻿/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2018-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Measurements.Core
{
    //TODO: add docs

    /// <summary>
    /// Before start the measurement process one of principal thing that user should define is 
    /// how exactly samples should be distributed(spreaded) to detectors. We suggest three types
    /// of spreading: 
    /// 1. By number of container (All sample from the same container will measure at the same detector)
    /// 2. Uniform spreading. We merely count all samples and divide it to number of detectors
    /// 3. Just in order: first sample to the first detectors, second to second, so on.
    /// </summary>
    public enum SpreadOptions { container, uniform, inOrder }

    /// <summary>
    /// Session class is used for control measurement process. Our measurement process involved few principal parameters:
    /// Type of measurement might be short lived(SLI), long lived that divided by two types just 1 or 2(LLI-1, LLI-2), measurement of background(FON)
    /// Date of irradiation that allow to receive list of sample which were irradiated in this date.
    /// Duration of measurement
    /// Count Mode - is internal parameter of MCA. It might be aCountToNormal, aCountToRealTime, aCountToLiveTime
    /// Height - is the distance between head of the detector and the sample
    /// </summary>
    public partial class Session : ISession, IDisposable
    {
        /// <summary>
        /// See description of logger in <see cref="SessionControllerSingleton"/>
        /// </summary>
        private NLog.Logger        _nLogger;

        /// <summary>
        /// Only this types of measurements are available for the work.
        /// </summary>
        public static readonly string[] MeasurementTypes = {"SLI", "LLI-1", "LLI-2", "FON" };

        /// <summary>
        /// Allows to specify SpreadOption. <see cref="SpreadOptions"/>
        /// </summary>
        public SpreadOptions SpreadOption { get; set; }

        private string _name;
        /// <summary>
        /// Property for setting of the name of session.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", $"{SessionControllerSingleton.ConnectionStringBuilder.UserID}--{Name}");
            }
        }
        private string _type;

        /// <summary>
        /// Type of measurement characterize some internal working logic for analysis. <seealso cref="MeasurementTypes"/>
        /// After type will specify. List of the irradiations date become available for the usage.
        /// </summary>
        public string Type
        {
            get { return _type; }
            set
            {
                try
                {
                    _nLogger.Info($"Type of measurement is {value}. List of irradiations dates will be prepare");

                    if (!MeasurementTypes.Contains(value))
                        throw new ArgumentException($"Type of measurement should contained in this list [{string.Join(",", MeasurementTypes)}]");

                    _type = value;
                    // TODO: perhaps here is better to use view with data has already agregated
                    IrradiationDateList.AddRange(_infoContext.Irradiations.Where(i => i.Type == value).Select(i => i.DateTimeStart).Distinct().ToList());
                }
                catch (ArgumentException ae)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, ae, NLog.LogLevel.Warn);
                }
                catch (Exception e)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
                }
            }
        }

        /// <summary>
        /// List of the unique irradiation date that is used for getting samples which were irradiate in this date
        /// </summary>
        public List<DateTime?> IrradiationDateList { get; private set; }
        private DateTime _currentIrradiationDate;

        /// <summary>
        /// Assignation this date will form list of samples which available for the spectra acquisition
        /// </summary>
        public DateTime CurrentIrradiationDate { get { return _currentIrradiationDate; }
            set
            {
                _nLogger.Info($"{value.ToString("dd.MM.yyyy")} has chosen. List of samples will be prepare");
                _currentIrradiationDate = value;
                SetIrradiationsList(_currentIrradiationDate); 
            }
        }

        /// <summary>
        /// Contains additional information about session
        /// </summary>
        public string Note { get; set; }
        private decimal _height;

        /// <summary>
        /// Characterize the distance between head of detector and measurement sample.
        /// We have a plans for automatically assign this property based on activity of the measurement sample
        /// </summary>
        public decimal Height
        {
            get { return _height; }
            set
            {
                _nLogger.Info($"Height {value} has specified");

                foreach (var m in MeasurementList)
                    m.Height = value;
            }
        }

        /// <summary>
        /// This event will occur after all detectors complete measurements of all samples
        /// </summary>
        public event EventHandler SessionComplete;

        /// <summary>
        /// This event will occur after one of managed detector by the session complete measurements of all samples
        /// </summary>
        public event EventHandler MeasurementDone;

        /// <summary>
        /// Allows user to specify duration of measurement and the mode of acqusition for each detector controlled by the session. <seealso cref="CanberraDeviceAccessLib.AcquisitionModes"/>
        /// </summary>
        /// <param name="duration">Characterize number of counts for certain mode of acquisition</param>
        /// <param name="acqm">Characterize mode of spectra acquisition. By default is aCountToRealTime</param>
        public void SetAcquireDurationAndMode(int duration, CanberraDeviceAccessLib.AcquisitionModes acqm = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime)
        {
            foreach (var d in ManagedDetectors)
                d.SetAcqureCountsAndMode(duration, acqm);
            CountMode = acqm;
            Counts = duration;
        }

        /// <summary>
        /// Allows user to get chosen acqusition mode specified via <seealso cref="SetAcquireDurationAndMode(int, CanberraDeviceAccessLib.AcquisitionModes)"/>
        /// </summary>
        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; private set; }
        private int _counts;
        /// <summary>
        /// Allows user to get the number of counts(duration) specified via <seealso cref="SetAcquireDurationAndMode(int, CanberraDeviceAccessLib.AcquisitionModes)"
        /// </summary>
        public int Counts
        {
            get { return _counts; }
            private set
            {
                _counts = value;
               foreach (var m in MeasurementList)
                    m.Duration = value;
            }
        }

        /// <summary>
        /// Internal field allow to control of filling models by the data via EF Core.
        /// </summary>
        private InfoContext _infoContext;
        /// <summary>
        /// List of irradiated samples with specified date and type. <see cref="IrradiationInfo"/>
        /// </summary>
        public List<IrradiationInfo> IrradiationList { get; private set; }
        /// <summary>
        /// This list contains information about measurement samples. <see cref="MeasurementInfo"/>
        /// </summary>
        public List<MeasurementInfo> MeasurementList { get; private set; }
        /// <summary>
        /// Allows user to get the list of samples spreaded to the detector with certain name
        /// </summary>
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        /// <summary>
        /// List of detectors that controlled by the session
        /// </summary>
        public List<IDetector> ManagedDetectors { get; }
        private bool _isDisposed = false;
        /// <summary>
        /// This is the simple counter which increment when one of detectors complete the measurement process. When all detectors are done measurement process, this number should be the same with number of managed detector by the session. When matching occur SessionComplete event will invoke. <seealso cref="SessionComplete"/>
        /// </summary>
        private int _countOfDetectorsWichDone = 0;

        public override string ToString() => $"{Name}-{Type}-{string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray())}-{CountMode}-{Counts}-{SpreadOption}-{Height}-{SessionControllerSingleton.ConnectionStringBuilder.UserID}-{Note}";

        /// <summary>
        /// Default constructor of the session class. This initialize field and specify some default values. For more details see the code.
        /// </summary>
        public Session()
        {
            Name = "Untitled session";

            _nLogger.Info("Initialisation of session has began");

            _height             = 2.5m;
            _infoContext        = new InfoContext();
            IrradiationDateList = new List<DateTime?>();
            IrradiationList     = new List<IrradiationInfo>();
            MeasurementList     = new List<MeasurementInfo>();
            ManagedDetectors    = new List<IDetector>();
            SpreadedSamples     = new Dictionary<string, List<IrradiationInfo>>();
            CountMode           = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
            MeasurementDone     += MeasurementDoneHandler;
            SpreadOption        = SpreadOptions.container;
            SessionControllerSingleton.ConectionRestoreEvent += UploadLocalDataToDB;
        }

        /// <summary>
        /// Overloaded constructor needed for the load of session from data base
        /// </summary>
        /// <param name="session"></param>
        public Session(SessionInfo session) : this()
        {
            _nLogger.Info($"Session with parameters {session} will be created");
            Name                    = session.Name;
            Type                    = session.Type;
            Counts                  = session.Duration;
            Height                  = session.Height;
            CountMode               = (CanberraDeviceAccessLib.AcquisitionModes)Enum.Parse(typeof(CanberraDeviceAccessLib.AcquisitionModes), session.CountMode);
            SpreadOption            = (SpreadOptions)Enum.Parse(typeof(SpreadOptions), session.SpreadOption);
            Note                    = session.Note;
            
            foreach (var dName in session.DetectorsNames.Split(','))
                AttachDetector(dName);                

        }

        /// <summary>
        /// Allows user to save session into the connected data base
        /// Schema of the session table:
        ///     [Name] [nvarchar](20) NOT NULL,
        ///     [DetectorsNames] [nvarchar] (30) NOT NULL,
        ///     [Type] [nvarchar] (5) NULL,
        ///     [CountMode] [nvarchar] (20) NULL,
        ///     [SpreadOption] [nvarchar] (10) NULL,
        ///     [Duration] [int] NULL,
        ///     [Height] [decimal](3,1) NULL,
        ///     [Assistant] [nvarchar] (15) NULL,
        ///     [Note] [nvarchar] (300) NULL,
        ///     PRIMARY KEY(Name)
        /// </summary>
        /// <param name="nameOfSession"></param>
        /// <param name="isBasic"></param>
        public void SaveSession(string nameOfSession, bool isBasic=false)
        {
            _nLogger.Info($"Session with parameters {this} will be save into DB {(isBasic ? "as basic" : "as customed" )} session with name '{nameOfSession}'");

            try
            {
                if (string.IsNullOrEmpty(nameOfSession))
                    throw new ArgumentNullException("Name of session must be specified");
                Name = nameOfSession;
                var sessionContext = new InfoContext();

                string assistant = null;
                if (!isBasic) assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;

                sessionContext.Add(new SessionInfo
                {
                    CountMode = this.CountMode.ToString(),
                    Duration = this.Counts,
                    Height = this.Height,
                    Name = this.Name,
                    Type = this.Type,
                    SpreadOption = this.SpreadOption.ToString(),
                    Assistant = assistant,
                    Note = this.Note,
                    DetectorsNames = string.Join(",", ManagedDetectors.Select(n => n.Name).ToArray())
                }
                );
                sessionContext.SaveChanges();
            }
            catch (ArgumentNullException are)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, are, NLog.LogLevel.Error);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, dbe, NLog.LogLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        }

        ~Session()
        {
            CleanUp();
        }

        public void Dispose()
        {
            CleanUp();
            GC.SuppressFinalize(this);
        }

        private void CleanUp()
        {
            _nLogger.Info($"Disposing session has began");

            if (!_isDisposed)
            {
                foreach (var d in ManagedDetectors)
                        SessionControllerSingleton.AvailableDetectors.Add(d);
                ManagedDetectors.Clear();
                SessionControllerSingleton.ManagedSessions.Remove(this);
            }
            _isDisposed = true;
        }

        /// <summary>
        /// Checks if connection to db is available. In case of success save current measurement on detector to db, in the other case use
        /// serialization for save CurrentMeasurement from detector to the local storage. By default path is 'D:\LocalData'
        /// </summary>
        /// <param name="det"></param>
        public void SaveMeasurement(ref IDetector det)
        {
            if (SessionControllerSingleton.TestDBConnection())
                SaveRemotely(ref det);
            else
                SaveLocally(ref det);
        }

        /// <summary>
        /// Save current measurement locally to the disk storage. By default folder is 'D:\LocalData'
        /// Name of file is "dd-MM-yyyy_hh-mm"_CountryCode-ClientNumber-Year-SetNumber-SetIndex-SampleNumber.json"
        /// </summary>
        /// <paramref name="det">Reference to the instance of detector class</>
        private void SaveLocally(ref IDetector det)
        {
            _nLogger.Info($"Something wrong with connection to the data base. Information about measurement of current sample {det.CurrentSample} from detector '{det.Name}' will be save locally");

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Include;

            if (!Directory.Exists(@"D:\\LocalData"))
                Directory.CreateDirectory(@"D:\\LocalData");

            StreamWriter sw   = null;
            JsonWriter writer = null;

            try
            {
                sw = new StreamWriter($"D:\\LocalData\\{DateTime.Now.ToString("dd-MM-yyyy_hh-mm")}_{det.CurrentSample}.json");
                writer = new JsonTextWriter(sw);
                serializer.Serialize(writer, det.CurrentMeasurement);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
            finally
            {
                sw?.Dispose();
                writer?.Close();
            }
        }


        /// <summary>
        /// Saves information about current measurement to the data base. <seealso cref="MeasurementInfo"/>
        /// </summary>
        /// <paramref name="det">Reference to the instance of detector class</>
        private void SaveRemotely(ref IDetector det)
        {
            try
            {
                _nLogger.Info($"Information about measurement of current sample {det.CurrentSample} from detector '{det.Name}' will be save to the data base");
                var ic = new InfoContext();
                ic.Measurements.Add(det.CurrentMeasurement);
                ic.SaveChanges();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, dbe, NLog.LogLevel.Error);
                SaveLocally(ref det);
            }
        }

        /// <summary>
        /// This internal method will be call when ConnectionRestoreEvent will occur <see cref="SessionControllerSingleton.ConectionRestoreEvent"/>
        /// It upload all files into memory via usage of desirilizer and then upload it to database.
        /// </summary>
        /// <returns>List of object with MeasurementInfo type that will be load to the data base. <seealso cref="MeasurementInfo"/></returns>
        private List<MeasurementInfo> LoadMeasurementsFiles()
        {
            _nLogger.Info($"Deserilization has begun");
            
            var dir                       = new DirectoryInfo(@"D:\LocalData");
            var files                     = dir.GetFiles("*.json").ToList();
            var MeasurementsInfoForUpload = new List<MeasurementInfo>();
            string                        fileName = "";
            StreamReader                  fileStream = null;

            try
            {
                foreach (var file in files)
                {
                    fileName = file.Name;
                    fileStream = File.OpenText(file.FullName);
                    JsonSerializer serializer = new JsonSerializer();
                    MeasurementsInfoForUpload.Add((MeasurementInfo)serializer.Deserialize(fileStream, typeof(MeasurementInfo)));
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
            finally
            {
                fileStream?.Dispose();
            }

            return MeasurementsInfoForUpload;
        }

        /// <summary>
        /// Upload local file to the database in order to keep data after connection lost. 
        /// In case of success loading local files wil be delete.
        /// </summary>
        private void UploadLocalDataToDB()
        {
            try
            {
                _nLogger.Info($"Local data has found. It will deserialize, load into db and then delete from local storage");
                var fileList = LoadMeasurementsFiles();
                if (fileList.Count == 0) return;
                var ic = new InfoContext();
                ic.Measurements.AddRange(LoadMeasurementsFiles());
                ic.SaveChanges();
                var dir   = new DirectoryInfo(@"D:\LocalData");
                var files = dir.GetFiles("*.json").ToList();
                foreach (var file in files)
                    file.Delete();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, NLog.LogLevel.Error);
            }
        }
    }
}
