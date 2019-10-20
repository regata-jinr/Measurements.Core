using System;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   StartMeasurements();
        void   PauseMeasurements();
        void   StopMeasurements(); //Pause and Clear
        void   SaveMeasurement(ref IDetector d);
        void   SaveSpectraOnDetectorToFile(ref IDetector d);
        void   SaveSpectraOnDetectorToFileAndDataBase(ref IDetector d);
        void   SaveSession(string nameOfSession, bool isPublic = false);
        void   ContinueMeasurements();
        void   ClearMeasurements();
        void   Dispose();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);

        event Action                     SessionComplete;
        event Action<MeasurementInfo>    MeasurementOfSampleDone;
        event Action<string>             MeasurementDone;

        AcquisitionModes      CountMode              { get; set; }
        string                Type                   { get; set; }
        string                Name                   { get; set; }
        string                Note                   { get; set; }
        List<IDetector>       ManagedDetectors       { get;      }
    }
}
