using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

    namespace Measurements.Core.Classes
    {
        class Measurement : Detector, Interfaces.IMeasurement, IDisposable
        {

            public Measurement(string detectorName, Sample s, string type, bool withSampleChanger, bool withDataBase) : base(detectorName)
            {

            }

        public string Type { get; set; }
            public int FileNumber { get; set; }
            public string mOperatorName { get; set; }
            public int mDuration { get; set; }
            public DateTime mDateStart { get; set; }
            public DateTime mTimeStart { get; set; }
            public DateTime mDateFinish { get; set; }
            public DateTime mTimeFinish { get; set; }
            public float Height { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            async void Interfaces.IMeasurement.Start(int time)
            {
                await Task.Run(() => base.AStart(time));
            }
            void Interfaces.IMeasurement.Continue()
            { }
            void Interfaces.IMeasurement.Restart()
            { }
            void Interfaces.IMeasurement.Stop()
            { }
            void Interfaces.IMeasurement.Pause()
            { }
            void Interfaces.IMeasurement.Clear()
            { }
            void Interfaces.IMeasurement.Save()
            { }
            void Interfaces.IMeasurement.SetInfo(Sample s, string type, string experimentator, string description)
            { }

            void IDisposable.Dispose()
            { }

        }
    }
