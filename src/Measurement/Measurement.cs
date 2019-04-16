using System;
using System.Collections.Generic;
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
    public class Measurement : Detector, IMeasurement, IDisposable
    {

        public Measurement(string detectorName, Sample s, string type, float height, int duration, string operatorName, bool withSampleChanger = true, bool withDataBase = true) : base(detectorName)
        {
            CountToRealTime = duration;
            FillSampleInfo(ref s);
            Type = type;
            OperatorName = operatorName;
            Height = height;
            // StartAsync();
        }

        public int FileNumber { get; set; }
        public DateTime mDateTimeStart { get; set; }
        public DateTime mDateTimeFinish { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async void StartAsync()
        {
            mDateTimeStart = DateTime.Now;
            await Task.Run(() => AStart());
        }
        public void Continue()
        {
            AContinue();
        }
        public void Stop()
        {
            AStop();
        }
        public void Clear()
        {
            AClear();
        }

        public void ShowMvcg()
        {

        }
        //void IMeasurement.SaveToDB()
        //{ }
        //void IMeasurement.Backup()
        //{ }
        //void IMeasurement.Restore()
        //{ }

        void IDisposable.Dispose()
        {
            Dispose();
        }


    }
}