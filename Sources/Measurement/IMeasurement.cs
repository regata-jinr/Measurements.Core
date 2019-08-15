using System;

namespace Measurements.Core
{
    public interface IMeasurement
    {
        int Id { get; } // readonly
        int SampleId { get; } // readonly
        int Duration { get; set; }
        string Type { get; set; } 
        string DetectorName { get; set; } // readonly
        string FileName { get; set; }
        string Assistant { get; set; } // readonly
        int ContainerNumber { get; set; }
        double Height { get; set; }
        DateTime FinishTime { get; set; }
        DateTime StartTime { get; set; }
        void Dispose();
    }
}
