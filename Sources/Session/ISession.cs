using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   StartMeasurements();
        bool   NextSample(ref IDetector d);
        void   PrevSample(ref IDetector d);
        void   MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref IDetector det);
        void   MakeSamplesCurrentOnAllDetectorsByNumber(int n);
        void   PauseMeasurements();
        void   StopMeasurements(); //Pause and Clear
        void   SaveMeasurement(ref IDetector d);
        void   SaveSpectra(ref IDetector d);
        void   SaveSession(string nameOfSession, bool isBasic = false);
        void   ContinueMeasurements();
        void   ClearMeasurements();
        void   Dispose();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);
        void   SpreadSamplesToDetectors();
        void   SetAcquireDurationAndMode(int duration, AcquisitionModes acqm = AcquisitionModes.aCountToRealTime);

        event EventHandler    SessionComplete;
        event EventHandler    MeasurementDone;

        AcquisitionModes      CountMode              { get; }
        SpreadOptions         SpreadOption           { get; set; }
        int                   Counts                 { get; }
        string                Type                   { get; set; }
        string                Name                   { get; set; }
        decimal               Height                 { get; set; }
        string                Note                   { get; set; }
        DateTime              CurrentIrradiationDate { get; set; }
        List<DateTime?>       IrradiationDateList    { get; }
        List<IrradiationInfo> IrradiationList        { get; }
        List<MeasurementInfo> MeasurementList        { get; }
        List<IDetector>       ManagedDetectors       { get; }

        Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
 
    }
}
