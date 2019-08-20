using System;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Measurements.Core
{
    [AutoMap(typeof(IrradiationInfo))]
    public partial class MeasurementInfo
    {
        [Key]
        [Ignore]
        public int      Id             { get; set; }
        [Required]
        [SourceMember(nameof(IrradiationInfo.Id))]
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
        [Ignore]
        public DateTime DateTimeStart  { get; set; }
        [Required]
        public int?     Duration       { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        [Ignore]
        public DateTime DateTimeFinish { get; set; }
        [Required]
        public string   FileSpectra    { get; set; }
        [Required]
        public string   Detector       { get; set; }
        [Required]
        public string   Assistant      { get; set; }
        public string   Note           { get; set; }
        [NotMapped]
        [Ignore]
        public string SetKey => $"{CountryCode}-{ClientNumber}-{Year}-{SetNumber}-{SetIndex}";
        public override string ToString() => $"{SetKey}-{SampleNumber}";
    }
}
