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

    partial class Session : ISession, IDisposable
    {
        public string Name { get; private set; }
        private string _type;
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                // TODO: perhaps here is better to use view with data has already agregated
                IrradiationDateList = _infoContext.Irradiations.Where(i => i.Type == Type).Select(i => i.DateTimeStart).Distinct().ToList();
            }
        }
        public List<DateTime> IrradiationDateList { get; private set; }
        private DateTime _currentIrradiationDate;
        public DateTime CurrentIrradiationDate { get { return _currentIrradiationDate; }
            set
            {
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
                foreach (var m in MeasurementList)
                    m.Height = value;
            }
        }
        public event EventHandler SessionComplete;
        public event EventHandler MeasurementDone;
        private CanberraDeviceAccessLib.AcquisitionModes _countMode;
        public CanberraDeviceAccessLib.AcquisitionModes CountMode
        {
            get { return _countMode; }
            set
            {
                _countMode = value;
                foreach (var d in _managedDetectors)
                    d.Options(value, Counts);
            }
        }
        private int _counts;
        public int Counts
        {
            get { return _counts; }
            set
            {
                _counts = value;
                foreach (var d in _managedDetectors)
                    d.Options(CountMode, value);
            }
        }
        private InfoContext _infoContext;
        
        public IrradiationInfo CurrentSample { get; private set; }
        public MeasurementInfo CurrentMeasurement { get; private set; } 
        public List<IrradiationInfo> IrradiationList { get; private set; }
        public List<MeasurementInfo> MeasurementList { get; private set; }
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        private List<Detector> _managedDetectors;
        public List<Detector> ManagedDetectors { get; }
        private bool _isDisposed = false;
        private Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes> _countModeDict;

             
        public Session()
        {
            Name                   = "Untitled session";
            _height                = 2.5m;
            _infoContext           = new InfoContext();
            IrradiationDateList    = new List<DateTime>();
            IrradiationList        = new List<IrradiationInfo>();
            MeasurementList        = new List<MeasurementInfo>();
            CurrentMeasurement     = new MeasurementInfo();
            CurrentSample          = new IrradiationInfo();
            _managedDetectors      = new List<Detector>();
            SpreadedSamples        = new Dictionary<string, List<IrradiationInfo>>();
            CountMode              = CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime;
            _countModeDict         = new Dictionary<string, CanberraDeviceAccessLib.AcquisitionModes>
                                        {
                                            { "aCountToLiveTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime },
                                            { "aCountToRealTime", CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime },
                                            { "aCountNormal", CanberraDeviceAccessLib.AcquisitionModes.aCountNormal }
                                        };
            
        }

        public Session(SessionInfo session) : this()
        {
            Name = session.Name;
            Type = session.Type;
            Counts = session.Duration;
            Height = session.Height;
            CountMode = _countModeDict[session.CountMode];
            Note = session.Note;

            foreach (var dName in session.DetectorsNames.Split(','))
            {
                AttachDetector(dName);                
            }

        }

        public void SaveSession(string nameOfSession, bool isBasic=false, string note = "")
        {
            //TODO: add try catch for warning in case of some data is missed.
            Name = nameOfSession;
            var sessionContext = new InfoContext();

            string assistant = null;
            if (!isBasic) assistant = SessionControllerSingleton.ConnectionStringBuilder.UserID;

            sessionContext.Add(new SessionInfo
                                              {
                                                   CountMode      = this.CountMode.ToString(),
                                                   Duration       = this.Counts,
                                                   Height         = this.Height,
                                                   Name           = this.Name,
                                                   Type           = this.Type,
                                                   Assistant      = assistant,
                                                   Note           = note,
                                                   DetectorsNames = string.Join(",", _managedDetectors.Select(n => n.Name).ToArray())
                                               }
            );
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

            if (!_isDisposed)
            {
                foreach (var d in _managedDetectors)
                        SessionControllerSingleton.AvailableDetectors.Add(d);
                _managedDetectors.Clear();
                SessionControllerSingleton.ManagedSessions.Remove(this);
            }
            _isDisposed = true;
        }

    }
}
