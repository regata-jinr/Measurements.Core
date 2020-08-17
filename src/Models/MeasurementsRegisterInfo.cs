using System;
using System.ComponentModel.DataAnnotations;

namespace Regata.Measurements.Models
{
    public class MeasurementsRegisterInfo
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int LoadNumber { get; set; }
        [Required]
        public string Type { get; set; }
        public DateTime? DateTimeStart { get; set; }
        public DateTime? DateTimeFinish { get; set; }
        public int SamplesCnt { get; set; }
        public string Detectors { get; set; }
        public string Assistant { get; set; }
        public string Note { get; set; }

    }
}

