using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    //TODO: rethinking data delivery via events from detector. Session should distinguish error, done, so on not only based on status.
    //TODO: add tests
    //TODO: add docs

    public partial class Session : ISession, IDisposable
    {
        private NLog.Logger        _nLogger;
        private string _name;
        public string Name
        {
            get { return _name; }
            private set
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

        public void SetAcquireModeAndDuration(CanberraDeviceAccessLib.AcquisitionModes acqm, int duration)
        {
            foreach (var d in ManagedDetectors)
                d.Options(acqm, duration);
        }

        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; }
        public int Counts { get; }

        private InfoContext _infoContext;
        public IrradiationInfo CurrentSample { get; private set; }
        public MeasurementInfo CurrentMeasurement { get; private set; } 
        public List<IrradiationInfo> IrradiationList { get; private set; }
        public List<MeasurementInfo> MeasurementList { get; private set; }
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        //private List<IDetector> _managedDetectors;
        public List<IDetector> ManagedDetectors { get; }
        private bool _isDisposed = false;
        private int _countOfDetectorsWichDone = 0;
        private Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes> _countModeDict;

        public Session()
        {
            Name = "Untitled session";

            _nLogger.Info("Initialisation of session has began");

            _height = 2.5m;
            _infoContext = new InfoContext();
            IrradiationDateList = new List<DateTime?>();
            IrradiationList = new List<IrradiationInfo>();
            MeasurementList = new List<MeasurementInfo>();
            CurrentMeasurement = new MeasurementInfo();
            CurrentSample = new IrradiationInfo();
            ManagedDetectors = new List<IDetector>();
            SpreadedSamples = new Dictionary<string, List<IrradiationInfo>>();
            CountMode = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
            _countModeDict = new Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes>
                                        {
                                            { "aCountToLiveTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime },
                                            { "aCountToRealTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime },
                                            { "aCountNormal", CanberraDeviceAccessLib.AcquisitionModes.aCountNormal }
                                        };
            MeasurementDone += MeasurementDoneHandler;
        }

        public Session(SessionInfo session) : this()
        {
            _nLogger.Info($"Session with parameters {session} will be loaded");
            Name = session.Name;
            Type = session.Type;
            Counts = session.Duration;
            Height = session.Height;
            CountMode = _countModeDict[session.CountMode];
            Note = session.Note;
            
            foreach (var dName in session.DetectorsNames.Split(','))
                AttachDetector(dName);                

        }

        public void SaveSession(string nameOfSession, bool isBasic=false, string note = "")
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
                    Assistant = assistant,
                    Note = note,
                    DetectorsNames = string.Join(",", ManagedDetectors.Select(n => n.Name).ToArray())
                }
                );
            }
            catch (ArgumentNullException arne)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = arne.Message, Level = NLog.LogLevel.Error });
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbu)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{dbu.InnerException}. The most probably you specified already existing name of session. Name of session should be unique.", Level = NLog.LogLevel.Warn });
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = e.Message, Level = NLog.LogLevel.Error });
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

    }
}
