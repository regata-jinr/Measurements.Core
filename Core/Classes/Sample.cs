using System;
using System.Data;

namespace Measurements.Core.Classes
{
    struct  Sample
    {
        public string CountryCode { get; set; } // "RU"
        public int ClientId { get; set; } // 1
        public int Year { get; set; } // 18
        public int SampleSetId { get; set; } // 55
        public char SampleSetIndex { get; set; } // j
        public int SampleNumber { get; set; } // 1
        public string ClientSampleId { get; set; } // a-01
        public float Weight { get; set; }
        public string IrradiationOperator { get; set; }
        public DateTime IrradiationStartDateTime { get; set; }
        public DateTime IrradiationFinishDateTime { get; set; }
        public string Description { get; set; }
        public string SetKey { get { return $"{CountryCode}-{ClientId}-{Year}-{SampleSetId}-{SampleSetIndex}"; } }

        public Sample(string countryCode, int clientId, int year, int sampleSetId, char sampleSetIndex, int sampleNumber, string clientSampleId, float weight, string irrOperator, DateTime irrStartDateTime, DateTime irrFinishDateTime, string description = "")
        {
            CountryCode = countryCode;
            ClientId = clientId;
            Year = year;
            SampleSetId = sampleSetId;
            SampleSetIndex = sampleSetIndex;
            SampleNumber = sampleNumber;
            ClientSampleId = clientSampleId;
            Weight = weight;
            IrradiationOperator = irrOperator;
            IrradiationStartDateTime = irrStartDateTime;
            IrradiationFinishDateTime = irrFinishDateTime;
            Description = description;

        }

        //TODO: perhaps a good idea to add constructor for concrete data container (DataGridViewRow or SqlDataReader)

        public override string ToString() {return $"{SetKey}-{SampleNumber}";}
    }
}
