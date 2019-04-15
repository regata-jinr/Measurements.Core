using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace MeasurementsCore
{
    /// <summary>
    /// Measurement class ia a wrapper for measurements process. 
    /// This class uses Detector, Sample, DataBase, FileStreamers so that control process of measurement.
    /// </summary>
    class Measurement : Detector, IMeasurement, IDisposable
    {


        public Measurement(string detectorName, Sample s, string type, int duration, string operatorName, bool withSampleChanger = true, bool withDataBase = true) : base(detectorName)
        {
            mOperatorName = operatorName;
            Start(duration);
        }

        public int FileNumber { get; set; }
        public string mOperatorName { get; set; }
        public int mDuration { get; set; }
        public DateTime mDateTimeStart { get; set; }
        public DateTime mDateTimeFinish { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async void Start(int time)
        {
            mDateTimeStart = DateTime.Now;
            await Task.Run(() => AStart(time));
        }
        public void Continue()
        { }
        public void Restart()
        { }
        public void Stop()
        { }
        public void Pause()
        { }
        public void Clear()
        { }
        //void IMeasurement.SaveToDB()
        //{ }
        //void IMeasurement.Backup()
        //{ }
        //void IMeasurement.Restore()
        //{ }
        public void SetInfo(Sample s, string type, string experimentator, string description)
        { }

        void IDisposable.Dispose()
        { }


    }
}