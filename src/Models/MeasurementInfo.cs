using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Regata.Measurements.Models
{
    [AutoMap(typeof(IrradiationInfo))]
    public class MeasurementInfo : INotifyPropertyChanged
    {
        [Key]
        [Ignore]
        public int Id { get; set; }
        [Required]
        [SourceMember(nameof(IrradiationInfo.Id))]
        public int IrradiationId { get; set; }
        [Required]
        public DateTime IrrJournalDate { get; set; }
        [Ignore]
        public int? LoadNumber { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        public string ClientNumber { get; set; }
        [Required]
        public string Year { get; set; }
        [Required]
        public string SetNumber { get; set; }
        [Required]
        public string SetIndex { get; set; }
        [Required]
        public string SampleNumber { get; set; }
        [Required]
        public string Type { get; set; }
        [Ignore]
        public decimal? Height { get; set; }
        [Ignore]
        public DateTime? DateTimeStart { get; set; }
        [Ignore]
        public int? Duration { get; set; }
        [Ignore]
        public DateTime? DateTimeFinish { get; set; }
        public string FileSpectra { get; set; }
        public string Detector { get; set; }
        public string Token { get; set; }
        [Ignore]
        public string Assistant { get; set; }

        [Ignore]
        public string Note 
        {
            get
            {
                return _note;
            }
            set
            {
                if (_note == value) return;
                _note = value;
                NotifyPropertyChanged();
            }
        }

        private string _note;

        [NotMapped]
        [Ignore]
        public string SetKey => $"{CountryCode}-{ClientNumber}-{Year}-{SetNumber}-{SetIndex}";

        [NotMapped]
        [Ignore]
        public string SampleKey => $"{SetIndex}-{SampleNumber}";
        public override string ToString() => $"{SetKey}-{SampleNumber}";

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotMapped]
        [Ignore]
        public static readonly IReadOnlyDictionary<MeasurementsType, string> SessionTypeMapStr = new Dictionary<MeasurementsType, string> { { MeasurementsType.sli, "SLI" }, { MeasurementsType.lli1,"LLI-1" }, { MeasurementsType.lli2, "LLI-2" }, { MeasurementsType.bckg, "BCKG" } };

        [NotMapped]
        [Ignore]
        public static readonly IReadOnlyDictionary<MeasurementsType, int> SessionTypeMapInt = new Dictionary<MeasurementsType, int> { { MeasurementsType.sli, 0 }, { MeasurementsType.lli1, 1 }, { MeasurementsType.lli2, 2 }, { MeasurementsType.bckg, 3 } };
    }


    public enum MeasurementsType { sli, lli1, lli2, bckg };
}
