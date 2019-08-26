using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace Measurements.Core
{
    //TODO: add docs
    //TODO: add tests

    partial class Session : ISession, IDisposable
    {
        public string Type { get; set; }
        public event EventHandler SessionComplete;
        public event EventHandler MeasurementDone;
        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; set; }
        public int Counts { get; set; }
        private IrradiationInfoContext _irradiationInfoContext;
        private MeasurementInfoContext _measurementInfoContext;
        
        public IrradiationInfo CurrentSample { get; private set; }
        public MeasurementInfo CurrentMeasurement { get; private set; } 
        public List<IrradiationInfo> IrradiationList { get; private set; }
        public List<MeasurementInfo> MeasurementList { get; private set; }
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        private List<Detector> _managedDetectors;
        private bool _isDisposed = false;

        public Session()
        {
            _irradiationInfoContext = new IrradiationInfoContext();
            _measurementInfoContext = new MeasurementInfoContext();
            IrradiationList = new List<IrradiationInfo>();
            MeasurementList = new List<MeasurementInfo>();
            CurrentMeasurement = new MeasurementInfo();
            CurrentSample = new IrradiationInfo();
            _managedDetectors = new List<Detector>();
            SpreadedSamples = new Dictionary<string, List<IrradiationInfo>>();
        }

        ~Session()
        {
            CleanUp(false);
        }

        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }

        private void CleanUp(bool isDisposing)
        {

            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    SessionControllerSingleton.ManagedSessions.Remove(this);
                    foreach (var d in _managedDetectors)
                        d.Dispose();
                }

                //FIXME: actually dispose already do the disconnect.
                foreach (var d in _managedDetectors)
                    d.Disconnect();

                _managedDetectors.Clear();
            }
            _isDisposed = true;
        }
    }
}
