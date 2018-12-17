using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measurements
{
    struct Sample
    {
        public string countryCode; //"RU"
        public int clientId; // 1
        public int year; // 18
        public int sampleSetId; // 55
        public char sampleSetIndex; // j
        public int sampleNumber; // 1
        public string clientSampleId; // a-01
        public float weight;
        public DateTime IrrStartDate;

        Sample(string countryCode, int clientId, int year, int sampleSetId, char sampleSetIndex, int sampleNumber, string clientSampleId, float weight, DateTime IrrStartDate) {
            this.countryCode = countryCode;
            this.clientId = clientId;
            this.year = year;
            this.sampleSetId = sampleSetId;
            this.sampleSetIndex = sampleSetIndex;
            this.sampleNumber = sampleNumber;
            this.clientSampleId = clientSampleId;
            this.weight = weight;
            this.IrrStartDate = IrrStartDate;
        }

        public override string ToString() {return $"{countryCode}-{clientId}-{year}-{sampleSetId}-{sampleSetIndex}-{sampleNumber}-[{clientSampleId}]";}

    }

}
