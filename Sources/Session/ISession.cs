using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   StartMeasurements();
        void   NextSample(ref Detector d);
        void   PrevSample(ref Detector d);
        void   MakeSampleCurrentOnDetector(ref IrradiationInfo ii, ref Detector det);
        void   PauseMeasurements();
        void   StopMeasurements(); //Pause and Clear
        void   SaveSpectra(ref Detector d); //if connection closed save locally to json
        void   SaveSession(string nameOfSession, bool isBasic = false, string note = "");
        void   ContinueMeasurements();
        void   ClearMeasurements();
        void   Dispose();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);
        void   SpreadSamplesToDetectors();

        event EventHandler    SessionComplete;
        event EventHandler    MeasurementDone;

        AcquisitionModes      CountMode              { get; set; }
        int                   Counts                 { get; set; }
        string                Type                   { get; set; }
        string                Name                   { get; }
        decimal               Height                 { get; set; }
        string                Note                   { get; set; }
        DateTime              CurrentIrradiationDate { get; set; }
        List<DateTime>        IrradiationDateList    { get; }
        List<IrradiationInfo> IrradiationList        { get; }
        List<MeasurementInfo> MeasurementList        { get; }
        List<Detector>        ManagedDetectors       { get; }
        IrradiationInfo       CurrentSample          { get; }
        MeasurementInfo       CurrentMeasurement     { get; } 

        Dictionary<string, List<IrradiationInfo>> SpreadedSamples { get; }
 
    }
}
