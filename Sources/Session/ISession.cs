using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   Start(); //for each detector task run (or await) start measure queue
        void   NextSample();
        void   PrevSample();
        void   MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref Detector det);
        void   Pause();
        void   Stop(); //Pause and Clear
        void   SaveSpectraFiles(); //if connection closed save locally to json
        void   SaveSession(string nameOfSession, bool isBasic = false, string note = "");
        void   Continue();
        void   Clear();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);
        void   SpreadSamplesToDetectors();
        void   SetIrradiationsList(DateTime date);

        event EventHandler SessionComplete;
        event EventHandler MeasurementDone;
        AcquisitionModes CountMode { get; set; }
        int Counts { get; set; }
        string Type { get; set; }
        string Name { get; }
        decimal Height { get; set; }
        string Note { get; set; }
        List<IrradiationInfo> IrradiationList { get; } //linq from dbcontext
        List<MeasurementInfo> MeasurementList { get; } //linq from dbcontext
        Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
        IrradiationInfo CurrentSample { get; }
        MeasurementInfo CurrentMeasurement { get; } 
 
    }
}
