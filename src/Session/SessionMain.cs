/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;

namespace Measurements.Core
{
  // this file contains general fields and methods of the Session class.
  // Session class divided by few files:
  // ├── ISession.cs                - interface of Session class
  // ├── SessionDetectorsControl.cs - contains methods that related with managing of detector
  // ├── SessionInfo.cs             - contains fields of Session for EF core interaction
  // └── SessionMain.cs --> opened

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
    private NLog.Logger _nLogger;

    private string _name;
    /// <summary>
    /// Property for setting of the name of session.
    /// </summary>
    public string Name
    {
      get { return _name; }
      set
      {
        if (!string.IsNullOrEmpty(Name))
          _nLogger.Info($"Session '{Name}' will rename to '{value}'");
        _name = value;
        _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", Name);
      }
    }

    /// <summary>
    /// Contains additional information about current session
    /// </summary>
    public string Note { get; set; }

    /// <summary>
    /// This event will occur after all detectors complete measurements of all samples
    /// </summary>
    public event Action SessionComplete;
    public event Action<MeasurementInfo> MeasurementOfSampleDone;

    /// <summary>
    /// This event will occur after one of managed detector by the session complete measurements of all samples
    /// </summary>
    public event Action<string> MeasurementDone;

    /// <summary>
    /// Sets duration of measurement and the mode of acqusition for each detector controlled by the session. <seealso cref="CanberraDeviceAccessLib.AcquisitionModes"/>
    /// </summary>
    /// <param name="duration">Characterize number of counts for certain mode of acquisition</param>
    /// <param name="acqm">Characterize mode of spectra acquisition. By default is aCountToRealTime</param>
    //private void SetAcquireDurationAndMode(int duration, CanberraDeviceAccessLib.AcquisitionModes acqm)
    //{
    //    Counts = duration;
    //    foreach (var d in ManagedDetectors)
    //        d.SetAcqureCountsAndMode(Counts, CountMode);
    //}

    /// <summary>
    /// Allows user to get chosen acqusition mode specified via <seealso cref="SetAcquireDurationAndMode(int, CanberraDeviceAccessLib.AcquisitionModes)"/>
    /// </summary>
    private CanberraDeviceAccessLib.AcquisitionModes _countMode;
    public CanberraDeviceAccessLib.AcquisitionModes CountMode
    {
      get { return _countMode; }
      set
      {
        var AvailableAcquisitionModes = new CanberraDeviceAccessLib.AcquisitionModes[]
                                            {
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime,
                                                        CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime
                                            };

        if (!AvailableAcquisitionModes.Contains(value))
        {
          _nLogger.Info($"Acquisition mode could be chosen only from this modes: {string.Join(", ", AvailableAcquisitionModes)}. aCountToRealTime will be set");
          return;
        }
        _nLogger.Info($"Acquisition mode of measurements is set to {value}");
        _countMode = value;
        foreach (var d in ManagedDetectors)
          d.AcquisitionMode = value;
      }
    }

    /// <summary>
    /// List of detectors that controlled by the session
    /// </summary>
    public List<IDetector> ManagedDetectors { get; private set; }
    private bool _isDisposed = false;
    /// <summary>
    /// This is the simple counter which increment when one of detectors complete the measurement process. When all detectors are done measurement process, this number should be the same with number of managed detector by the session. When matching occur SessionComplete event will invoke. <seealso cref="SessionComplete"/>
    /// </summary>
    private int _countOfDetectorsWichDone = 0;

    public string Type { get; set; }

    public override string ToString() => $"{Name}-{Type}-{string.Join(",", ManagedDetectors.Select(d => d.Name).ToArray())}-{CountMode}-{SessionControllerSingleton.ConnectionStringBuilder.UserID}-{Note}";

    /// <summary>
    /// Default constructor of the session class. This initialize field and specify some default values. For more details see the code.
    /// </summary>
    public Session()
    {
      Name = "Untitled session";

      _nLogger.Info("Initialisation of session has begun");

      ManagedDetectors = new List<IDetector>();
      CountMode = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
      MeasurementDone += MeasurementDoneHandler;
      DetectorsListsChanged += SessionControllerSingleton.AvailableDetectorsChangesHaveOccurred;
      SessionControllerSingleton.ConectionRestoreEvent += UploadLocalDataToDB;
    }

    /// <summary>
    /// Overloaded constructor for the loading of session from data base
    /// </summary>
    /// <param name="session"></param>
    public Session(SessionInfo session) : this()
    {
      _nLogger.Info($"Session with parameters {session} will be created");
      Name = session.Name;
      Type = session.Type;
      CountMode = (CanberraDeviceAccessLib.AcquisitionModes)Enum.Parse(typeof(CanberraDeviceAccessLib.AcquisitionModes), session.CountMode);
      Note = session.Note;

      if (!string.IsNullOrEmpty(session.DetectorsNames))
      {
        foreach (var dName in session.DetectorsNames.Split(','))
          AttachDetector(dName);
      }

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
    public void SaveSession(string nameOfSession, bool isPublic = false)
    {
      _nLogger.Info($"Session with parameters {this} will save into DB {(isPublic ? "as public" : "as private")} session with name '{nameOfSession}'");

      try
      {
        if (string.IsNullOrEmpty(nameOfSession))
          throw new ArgumentNullException("Name of session must be specified");
        Name = nameOfSession;
        var sc = new InfoContext();

        if (sc.Sessions.Where(s => s.Name == Name).Any())
          UpdateExistingSession(isPublic);
        else
          SaveNewSession(isPublic);

      }
      catch (ArgumentNullException are)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, are, Handlers.ExceptionLevel.Error);
      }
      catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, dbe, Handlers.ExceptionLevel.Warn);
      }
      catch (Exception e)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
      }
    }


    private void SaveNewSession(bool isPublic)
    {
      try
      {
        var sessionContext = new InfoContext();

        string assistant = null;
        if (!isPublic) assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;

        sessionContext.Sessions.Add(new SessionInfo
        {
          CountMode = this.CountMode.ToString(),
          Name = this.Name,
          Type = this.Type,
          Assistant = assistant,
          Note = this.Note,
          DetectorsNames = string.Join(",", ManagedDetectors.Select(n => n.Name).ToArray())
        });

        sessionContext.SaveChanges();
      }
      catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, dbe, Handlers.ExceptionLevel.Warn);
      }
      catch (Exception e)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
      };
    }

    private void UpdateExistingSession(bool isPublic)
    {
      try
      {
        string assistant = null;
        if (!isPublic) assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;

        var sc = new InfoContext();
        var si = sc.Sessions.Where(s => s.Name == Name).First();

        si.CountMode = this.CountMode.ToString();
        si.Name = this.Name;
        si.Type = this.Type;
        si.Assistant = assistant;
        si.Note = this.Note;
        si.DetectorsNames = string.Join(",", ManagedDetectors.Select(n => n.Name).ToArray());

        sc.Sessions.Update(si);
        sc.SaveChanges();
      }
      catch (Microsoft.EntityFrameworkCore.DbUpdateException dbe)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, dbe, Handlers.ExceptionLevel.Warn);
      }
      catch (Exception e)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
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
      _nLogger.Info($"Disposing session has begun");

      if (!_isDisposed)
      {
        for (var i = ManagedDetectors.Count - 1; i >= 0; --i)
          DetachDetector(ManagedDetectors[i].Name);

        MeasurementDone -= MeasurementDoneHandler;
        DetectorsListsChanged -= SessionControllerSingleton.AvailableDetectorsChangesHaveOccurred;
        SessionControllerSingleton.ConectionRestoreEvent -= UploadLocalDataToDB;
        SessionControllerSingleton.ManagedSessions.Remove(this);
      }
      _isDisposed = true;
    }

    // TODO: Change to SqlConnection.State
    /// <summary>
    /// Checks if connection to db is available.
    /// In case of success save current measurement on detector to db, in the other case use
    /// serialization for save CurrentMeasurement from detector to the local storage. By default path is 'D:\LocalData'
    /// </summary>
    /// <param name="det"></param>
    public void SaveMeasurement(ref IDetector det)
    {
      if (SessionControllerSingleton.TestDBConnection())
        SaveRemotely(det);
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
      if (det.CurrentMeasurement == null || det == null) return;

      StreamWriter sw = null;
      JsonWriter writer = null;
      try
      {
        _nLogger.Info($"Something wrong with connection to the data base. Information about measurement of current sample {det.CurrentMeasurement} from detector '{det.Name}' will be save locally");

        JsonSerializer serializer = new JsonSerializer();
        serializer.NullValueHandling = NullValueHandling.Include;

        if (!Directory.Exists(@"D:\\LocalData"))
          Directory.CreateDirectory(@"D:\\LocalData");

        sw = new StreamWriter($"D:\\LocalData\\{DateTime.Now.ToString("dd-MM-yyyy_hh-mm")}_{det.CurrentMeasurement}.json");
        writer = new JsonTextWriter(sw);
        serializer.Serialize(writer, det.CurrentMeasurement);
      }
      catch (Exception e)
      {
        Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
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
    private void SaveRemotely(IDetector det)
    {
      if (det.CurrentMeasurement == null || det == null) return;
      using (var ic = new InfoContext())
      {
        try
        {
          if (ic.Measurements.Where(m => m.Id == det.CurrentMeasurement.Id && string.IsNullOrEmpty(m.FileSpectra)).Any())
          {
            _nLogger.Info($"Information about measurement of current sample {det.CurrentMeasurement} from detector '{det.Name}' will be save to the data base");
            ic.Measurements.Update(det.CurrentMeasurement);
          }
          else
          {
            ic.Measurements.Add(new MeasurementInfo
            {
              IrradiationId = det.CurrentMeasurement.IrradiationId,
              CountryCode = det.CurrentMeasurement.CountryCode,
              ClientNumber = det.CurrentMeasurement.ClientNumber,
              Year = det.CurrentMeasurement.Year,
              SetNumber = det.CurrentMeasurement.SetNumber,
              SetIndex = det.CurrentMeasurement.SetIndex,
              SampleNumber = det.CurrentMeasurement.SampleNumber,
              Type = det.CurrentMeasurement.Type,
              Height = det.CurrentMeasurement.Height,
              DateTimeStart = det.CurrentMeasurement.DateTimeStart,
              Duration = det.CurrentMeasurement.Duration,
              DateTimeFinish = det.CurrentMeasurement.DateTimeFinish,
              FileSpectra = det.CurrentMeasurement.FileSpectra,
              Detector = det.CurrentMeasurement.Detector,
              Assistant = det.CurrentMeasurement.Assistant,
              Note = det.CurrentMeasurement.Note
            }
            );
          }
          ic.SaveChanges();
        }
        catch (Exception dbe)
        {
          Handlers.ExceptionHandler.ExceptionNotify(this, dbe.InnerException, Handlers.ExceptionLevel.Error);
          SaveLocally(ref det);
        }
      }
    }

    /// <summary>
    /// This internal method will be call when ConnectionRestoreEvent will occur <see cref="SessionControllerSingleton.ConectionRestoreEvent"/>
    /// It upload all files into memory via usage of desirilizer and then upload it to database.
    /// </summary>
    /// <returns>List of object with MeasurementInfo type that will be load to the data base. <seealso cref="MeasurementInfo"/></returns>
    private List<MeasurementInfo> LoadMeasurementsFiles()
    {

      StreamReader fileStream = null;
      var MeasurementsInfoForUpload = new List<MeasurementInfo>();

      try
      {
        _nLogger.Info($"Deserilization has begun");
        var dir = new DirectoryInfo(@"D:\LocalData");

        if (!dir.Exists)
          return MeasurementsInfoForUpload;

        var files = dir.GetFiles("*.json").ToList();
        string fileName = "";

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
        Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
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
      var fileList = LoadMeasurementsFiles();
      if (fileList.Count == 0) return;

      using (var ic = new InfoContext())
      {
        try
        {
          _nLogger.Info($"Local data has found. It will deserialize, load into db and then delete from local storage");

          ic.Measurements.UpdateRange(fileList);
          ic.SaveChanges();
          var dir = new DirectoryInfo(@"D:\LocalData");
          var files = dir.GetFiles("*.json").ToList();
          foreach (var file in files)
            file.Delete();
        }
        catch (Exception e)
        {
          Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
        }
      }
    }

  } // Session
}     // Measurements.Core