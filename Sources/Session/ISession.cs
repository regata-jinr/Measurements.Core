using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public interface ISession
    {
        void   StartMeasurements();
        void   PauseMeasurements();
        void   StopMeasurements();
        //void   SaveMeasurements();
        void   ContinueMeasurements();
        void   ClearMeasurements();
        void   Dispose();
        void   AttachDetector(string dName);
        void   DetachDetector(string dName);

        AcquisitionModes CountMode        { get; set; }
        string           Type             { get; set; }
        string           Name             { get; set; }
        string           Note             { get; set; }
        List<IDetector>  ManagedDetectors { get;      }
    }
}
