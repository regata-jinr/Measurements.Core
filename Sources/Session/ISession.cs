using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;


namespace Measurements.Core
{
    public interface ISession
    {
        string Type { get; set; }
        void Start(); //for each detector task run (or await) start measure queue
        void Stop();
        void Save(); //if connection closed save locally to json check if json exists
        void Continue();
        void Clear();
        void AttachDetector(string dName);
        void DetachDetector(string dName);

        void SpreadSamplesToDetectors(string option);
        CanberraDeviceAccessLib.AcquisitionModes CountMode { get; set; }
        int Counts { get; set; }
        void FillIrradiationList(DateTime date);
        List<IrradiationInfo> CurrentSamples { get; } //linq from dbcontext
        Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }


    }
}
