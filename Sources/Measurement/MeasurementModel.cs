using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Measurements.Core
{
    public partial class Measurement //Model
    {
        [Key]
        public int      Id             { get; set; }
        [Required]
        public int      IrradiationId  { get; set; }
        [Required]
        public string   CountryCode    { get; set; }
        [Required]
        public string   ClientNumber   { get; set; }
        [Required]
        public string   Year           { get; set; }
        [Required]
        public string   SetNumber      { get; set; }
        [Required]
        public string   SetIndex       { get; set; }
        [Required]
        public string   SampleNumber   { get; set; }
        [Required]
        public string   Type           { get; set; }
        public int?     Height         { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateTimeStart  { get; set; }
        [Required]
        public int?     Duration       { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime DateTimeFinish { get; set; }
        [Required]
        public string   FileSpectra    { get; set; }
        [Required]
        public string   Detector       { get; set; }
        [Required]
        public int?     Assistant      { get; set; }
        public string   Note           { get; set; }
        [NotMapped]
        public string SetKey => $"{CountryCode}-{ClientNumber}-{Year}-{SetNumber}-{SetIndex}";
        public override string ToString() => $"{SetKey}-{SampleNumber}";

    }
}
