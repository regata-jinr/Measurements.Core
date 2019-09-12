using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace Measurements.Core
{
    //TODO: add tests
    //TODO: add docs

    public enum SpreadOptions { container, uniform, inOrder }

    public partial class Session : ISession, IDisposable
    {
        private NLog.Logger        _nLogger;
        private string _name;

        public SpreadOptions SpreadOption { get; set; }

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

        public string Type
        {
            get { return _type; }
            set
            {
                _nLogger.Info($"Type of measurement is {value}. List of irradiations dates will be prepare");
                _type = value;
                // TODO: perhaps here is better to use view with data has already agregated
                IrradiationDateList.AddRange(_infoContext.Irradiations.Where(i => i.Type == value).Select(i => i.DateTimeStart).Distinct().ToList());
            }
        }
        public List<DateTime?> IrradiationDateList { get; private set; }
        private DateTime _currentIrradiationDate;
        public DateTime CurrentIrradiationDate { get { return _currentIrradiationDate; }
            set
            {
                _nLogger.Info($"{value.ToString("dd.MM.yyyy")} has chosen. List of samples will be prepare");
                _currentIrradiationDate = value;
                SetIrradiationsList(_currentIrradiationDate); 
            }
        }

        public string Note { get; set; }
        private decimal _height;
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
        public event EventHandler SessionComplete;
        public event EventHandler MeasurementDone;

        public void SetAcquireDurationAndMode(int duration, CanberraDeviceAccessLib.AcquisitionModes acqm = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime)
        {
            foreach (var d in ManagedDetectors)
                d.SetAcqureCountsAndMode(duration, acqm);
            CountMode = acqm;
            Counts = duration;
        }

        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; private set; }
        private int _counts;
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

        private InfoContext _infoContext;
        public IrradiationInfo CurrentSample { get; private set; }
        public MeasurementInfo CurrentMeasurement { get; private set; } 
        public List<IrradiationInfo> IrradiationList { get; private set; }
        public List<MeasurementInfo> MeasurementList { get; private set; }
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        public List<IDetector> ManagedDetectors { get; }
        private bool _isDisposed = false;
        private int _countOfDetectorsWichDone = 0;

        public override string ToString() => $"{Name}-{Type}-{string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray())}-{CountMode}-{Counts}-{SpreadOption}-{Height}-{SessionControllerSingleton.ConnectionStringBuilder.UserID}-{Note}";

        public Session()
        {
            Name = "Untitled session";

            _nLogger.Info("Initialisation of session has began");

            _height             = 2.5m;
            _infoContext        = new InfoContext();
            IrradiationDateList = new List<DateTime?>();
            IrradiationList     = new List<IrradiationInfo>();
            MeasurementList     = new List<MeasurementInfo>();
            CurrentMeasurement  = new MeasurementInfo();
            CurrentSample       = new IrradiationInfo();
            ManagedDetectors    = new List<IDetector>();
            SpreadedSamples     = new Dictionary<string, List<IrradiationInfo>>();
            CountMode           = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
            MeasurementDone     += MeasurementDoneHandler;
            SpreadOption        = SpreadOptions.container;
            SessionControllerSingleton.ConectionRestoreEvent += UploadLocalDataToDB;
        }

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

        public void SaveMeasurement(ref IDetector det)
        {
            if (SessionControllerSingleton.TestDBConnection())
                SaveRemotely(ref det);
            else
                SaveLocally(ref det);
        }

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
