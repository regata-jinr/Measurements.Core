using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measurements.Core.Classes
{
    //todo: add get and set string like 'RU-01-18-55-j' to corresponding parameters
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
        public DateTime IrrStartDate { get; }

        public Sample(string countryCode, int clientId, int year, int sampleSetId, char sampleSetIndex, int sampleNumber, string clientSampleId, float weight, DateTime irrStartDate)
        {
            CountryCode = countryCode;
            ClientId = clientId;
            Year = year;
            SampleSetId = sampleSetId;
            SampleSetIndex = sampleSetIndex;
            SampleNumber = sampleNumber;
            ClientSampleId = clientSampleId;
            Weight = weight;
            IrrStartDate = irrStartDate;
        }

       //TODO: in such case first off all we should have additional way for setting weight, date and clientId (setter)
       //TODO: in case of exceptions, what values should be assign?
       //public Sample(string setKey, int sampleId)
       // {
       //     try
       //     {
       //         string[] arr = setKey.Split('-');
       //         if (arr.Length != 5) throw new IndexOutOfRangeException();
       //         CountryCode = arr[0];
       //         ClientId = Convert.ToInt32(arr[1]);
       //         Year = Convert.ToInt32(arr[2]);
       //         SampleSetId = Convert.ToInt32(arr[3]);
       //         SampleSetIndex = arr[4][0];
       //         SampleNumber = sampleId;
       //         ClientSampleId = "";
       //         Weight = 0F;
       //         IrrStartDate = Convert.ToDateTime("01.01.2019");

       //     }
       //     catch (FormatException fe)
       //     {

       //     }
       //     catch (IndexOutOfRangeException ir)
       //     {

       //     }
       // }






        public override string ToString() {return $"{CountryCode}-{ClientId}-{Year}-{SampleSetId}-{SampleSetIndex}-{SampleNumber}-[{ClientSampleId}]";}

    }

}
