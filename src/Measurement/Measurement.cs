using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

    namespace MeasurementsCore
    {
        class Measurement : Detector, IMeasurement, IDisposable
        {

            public Measurement(string detectorName, Sample s, string type, bool withSampleChanger, bool withDataBase) : base(detectorName)
            {

            }

            public int FileNumber { get; set; }
            public string mOperatorName { get; set; }
            public int mDuration { get; set; }
            public DateTime mDateStart { get; set; }
            public DateTime mTimeStart { get; set; }
            public DateTime mDateFinish { get; set; }
            public DateTime mTimeFinish { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            async void IMeasurement.Start(int time)
            {
                await Task.Run(() => base.AStart(time));
            }
            void IMeasurement.Continue()
            { }
            void IMeasurement.Restart()
            { }
            void IMeasurement.Stop()
            { }
            void IMeasurement.Pause()
            { }
            void IMeasurement.Clear()
            { }
            void IMeasurement.Save()
            { }
            void IMeasurement.SetInfo(Sample s, string type, string experimentator, string description)
            { }

            void IDisposable.Dispose()
            { }

        }
    }
