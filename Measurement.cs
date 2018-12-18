using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Measurements
{
    class Measurement
    {
       public string mType;
        private int fileNumber;
        public int FileNumber
        {
            get { return this.fileNumber; }
            set { this.fileNumber = value; }
        }
       public string mOperatorName;
       public int mDuration;
       public DateTime mDateStart;
       public DateTime mTimeStart;
       public DateTime mDateFinish;
       public DateTime mTimeFinish;

       Measurement() { }
       public void Start() { }
       public void Continue() { }
       public void Reset() { }
       public void Stop() { }
       public void OpenMVCG() { }
       public void SetSampleInfo(Sample s) { }
       public void InitializeFields() { }
       public bool CheckDependencies() { return false; } //check Detectors, Samples, FileNumbers, SampleChangers, System, DBConnections,
    }
}
