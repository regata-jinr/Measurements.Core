using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Measurements.Core
{
    public class IrradiationInfo
    {
        [Key]
        public int Id { get; set; }
        public string CountryCode { get; set; } // "RU"
        public string ClientNumber { get; set; } // 1
        public string Year { get; set; } // 18
        public string SetNumber { get; set; } // 55
        public string SetIndex { get; set; } // j
        public string SampleNumber { get; set; } // 1
        public string Type { get; set; } // SLI
        public double Weight { get; set; }
        public DateTime DateTimeStart { get; set; }
        public int Duration { get; set; }
        public DateTime DateTimeFinish { get; set; }
        public int? Container { get; set; }
        public int? Position { get; set; }
        public int? Channel { get; set; }
        public int? LoadNumber { get; set; }
        public int? Rehandler { get; set; }
        public int? Assistant { get; set; }
        public string Note { get; set; }

        [NotMapped]
        public string SetKey => $"{CountryCode}-{ClientNumber}-{Year}-{SetNumber}-{SetIndex}";
        [NotMapped]
        public string SampleKey => $"{SetIndex}-{SampleNumber}";
        public override string ToString() => $"{SetKey}-{SampleNumber}";

    }
}
