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
        private int _spectraFile;
        private string _localDir;
        private string _remoteDir;
        private string _mType;


        public Measurement(string detectorName, Sample s, string type, float height, int duration, string operatorName, bool withSampleChanger = true, bool withDataBase = true) : base(detectorName)
        {
            CountToRealTime = duration;
            FillSampleInfo(ref s);
            Type = type;
            OperatorName = operatorName;
            Height = height;
            _mProcessManager = new ProcessManager();
            _isShowed = false;
            MType = type;
            SpectraFile = 0;
            LocalDir = $"D:\\Spectra\\{DateTimeStart.Year}\\{DateTimeStart.Month.ToString("D2")}\\{MType}\\";
            RemoteDir = $"/Users/bdrum/Spectra/{DateTimeStart.Year}/{DateTimeStart.Month.ToString("D2")}/{MType}/";
            StartAsync();
        }

        public string LocalDir
        {
            get { return _localDir; }
            set
            {
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                _localDir = value;
            }
        }

        public string RemoteDir
        {
            get { return _remoteDir; }
            set
            {
                try
                {
                    using (var sftp = new SftpClient(Properties.Resources.sftpHost, Properties.Resources.sftpUser, Properties.Resources.sftpPass))
                    {
                        sftp.Connect();

                        if (!sftp.Exists(value))
                            sftp.CreateDirectory(value);
                        _remoteDir = value;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        //TODO: generate file number from last file in DB
        public int SpectraFile
        {
            get { return _spectraFile; }
            private set
            {
                var r = new Random();
                _spectraFile = r.Next();
            }
        }
        public string MType {
            get { return _mType; }
            set
            {
                if (string.Equals(value, "SLI", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "LLI-1", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "LLI-2", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(value, "BACKGROUND", StringComparison.OrdinalIgnoreCase))
                    _mType = value.ToUpper();
                else
                    throw new InvalidOperationException($"Such type {value} can't be used. You should choose from that list: " +
                                                        $"['SLI', 'LLI-1', 'LLI-2', 'BACKGROUND']");
            }
        }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeFinish { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async void StartAsync()
        {
            DateTimeStart = DateTime.Now;
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
            DateTimeFinish = DateTime.Now;
        }
        public void SaveSpectraToFile()
        {

            SaveSpectraToFile($"{LocalDir}\\{SpectraFile}");

            using (var sftp = new SftpClient(Properties.Resources.sftpHost, Properties.Resources.sftpUser, Properties.Resources.sftpPass))
            {
                sftp.Connect();
                try
                {
                    using (var file = File.Open($"{RemoteDir}/{SpectraFile}.cnf", FileMode.Open))
                    {
                        sftp.UploadFile(file, $"{RemoteDir}/{SpectraFile}.cnf");
                    }

                }
                //TODO: add special extinsions for connections problems and so on
                catch (Exception ex)
                {

                }
            }

        }


        public void SaveToDB()
        {

        }
        public void Backup()
        {

        }
        //{ }
        //void IMeasurement.Restore()
        //{ }

        void IDisposable.Dispose()
        {
            Dispose();
        }


    }
}