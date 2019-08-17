using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;


namespace Measurements.Core
{
    public interface ISession
    {
        string Type { get; set; }
        string Assistant { get; }
        void Start(); //for each detector task run (or await) start measure queue
        void Stop();
        void Save(); //if connection closed save locally to json check if json exists
        void Continue();
        void Clear();
        void AttachDetector(string dName);
        void DetachDetector(string dName);

        void SpreadSamplesToDetectors(string option);
        // in case of two session and different detectors we have opportunity to start measurements process with the same sample
        // It should have a checks for such case and it means we should save changes to db two times: in start time and after finish
        bool IsItMeasuring(); 
        CanberraDeviceAccessLib.AcquisitionModes CountMode { get; set; }
        int Counts { get; set; }
        void FillIrradiationList(DateTime date);
        List<IrradiationInfo> CurrentSamples { get; } //linq from dbcontext
        Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }


    }
}
