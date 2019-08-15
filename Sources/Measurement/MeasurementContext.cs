using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Measurements.Core
{
     public partial class MeasurementContext : DbContext, IMeasurement, IDisposable
    {
        public int Id { get; } // readonly
        public int Duration { get; set; }
        public string Type { get; set; }
        public string DetectorName { get; set; } // readonly
        public string FileName { get; set; }
        public string Assistant { get; set; } // readonly
        public int SampleId { get; } // readonly
        public int ContainerNumber { get; set; }
        public double Height { get; set; }
        public DateTime FinishTime { get; set; }
        public DateTime StartTime { get; set; }


        public void Dispose()
        {
        }


        MeasurementContext()
        {

        }
    }
}
