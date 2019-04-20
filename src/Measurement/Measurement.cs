using System;
using System.IO;
using Renci.SshNet;
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
        private ProcessManager _mProcessManager;
        private bool _isShowed;
        private string _spectraFile;
        public Measurement(string detectorName, Sample s, string type, float height, int duration, string operatorName, bool withSampleChanger = true, bool withDataBase = true) : base(detectorName)
        {
            CountToRealTime = duration;
            FillSampleInfo(ref s);
            Type = type;
            OperatorName = operatorName;
            Height = height;
            _mProcessManager = new ProcessManager();
            _isShowed = false;
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

        public void ShowDetectorInMvcg()
        {
            _mProcessManager.ShowDetectorInMvcg(Name);
        }

        public void CompleteMeasurement()
        {

        }
        public void SaveSpectraToFile()
        {
            if (!Directory.Exists(Path.GetDirectoryName(_spectraFile)))
                Directory.CreateDirectory(Path.GetDirectoryName(_spectraFile));
            SaveSpectraToFile(_spectraFile);
            //TODO: prepare settings file
            using (var sftp = new SftpClient("", "", "")) // new SftpClient(Properties.Settings.Default.host, Properties.Settings.Default.user, Properties.Settings.Default.psw))
            {
                sftp.Connect();
                try
                {
                    //TODO: server path not the same as local
                    using (var file = File.Open(_spectraFile, FileMode.Open))
                    {
                        sftp.UploadFile(file, $"Spectra/{_spectraFile}");
                    }

                }
                catch (Exception ex)
                {

                }
            }

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