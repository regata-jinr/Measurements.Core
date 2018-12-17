using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measurements
{
    class Measurement
    {
       public string mType;
       public int mFileNumber;
       public string mOperatorName;
       public int mDuration;
       public DateTime mDateStart;
       public DateTime mTimeStart;
       public DateTime mDateFinish;
       public DateTime mTimeFinish;


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
