using System;

namespace Measurements.Core.Classes
{
    struct  Sample
    {
        public string CountryCode { get; } // "RU"
        public int ClientId { get; } // 1
        public int Year { get; } // 18
        public int SampleSetId { get; } // 55
        public char SampleSetIndex { get; } // j
        public int SampleNumber { get; } // 1
        public string ClientSampleId { get; } // a-01
        public float Weight { get; }
        public DateTime IrrStartDateTime { get; }
        public DateTime IrrFinishDateTime { get; }
        public string SetKey { get { return $"{CountryCode}-{ClientId}-{Year}-{SampleSetId}-{SampleSetIndex}"; } }

        public Sample(string countryCode, int clientId, int year, int sampleSetId, char sampleSetIndex, int sampleNumber, string clientSampleId, float weight, DateTime irrStartDateTime, DateTime irrFinishDateTime)
        {
            CountryCode = countryCode;
            ClientId = clientId;
            Year = year;
            SampleSetId = sampleSetId;
            SampleSetIndex = sampleSetIndex;
            SampleNumber = sampleNumber;
            ClientSampleId = clientSampleId;
            Weight = weight;
            IrrStartDateTime = irrStartDateTime;
            IrrFinishDateTime = irrFinishDateTime;

        }
        public override string ToString() {return $"{SetKey}-{SampleNumber}";}
    }
}
