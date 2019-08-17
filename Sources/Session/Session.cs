using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measurements.Core
{
    class Session : ISession
    {

        public string Type { get; set; }
        private readonly string _assistant;
        public string Assistant { get { return _assistant; } }

        public Session(string assistant)
        {
            _assistant = assistant;
        }

        //for each detector task run (or await) start measure queue
        public void Start()
        {

        }
        public void Stop()
        { }
        //if connection closed save locally to json check if json exists
        public void Save()
        {

        }
        private void SaveLocally()
        { }
        private void SaveRemotely()
        { }
        public void Continue()
        { }
        public void Clear()
        { }
        public void AttachDetector(string dName)
        { }
        public void DetachDetector(string dName)
        { }

        public void SpreadSamplesToDetectors(string option)
        { }
        // in case of two session and different detectors we have opportunity to start measurements process with the same sample
        // It should have a checks for such case and it means we should save changes to db two times: in start time and after finish
        public bool IsItMeasuring()
        {
            return false;
        }
        public CanberraDeviceAccessLib.AcquisitionModes CountMode { get; set; }
        public int Counts { get; set; }
        public void FillIrradiationList(DateTime date)
        {
        }
        public List<IrradiationInfo> CurrentSamples { get; } //linq from dbcontext
        public Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }

    }
}
