using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MeasurementsCore
{

    //FIXME: few thoughts about design: In my case when application has started user choose detectors which he'd use.
    //      It means, further we should keep connections with choosen detectors and change only certain properties like
    //      sample info, height and so on. But in current design I fill all of this inforamtion via constructor, it 
    //      means usage of new object of measurement for each real measurement, but in this case I should free memory including 
    //      connections, displaying of detector status via mvcg. This bad logic. 

    /// <summary>
    /// Measurement class ia a wrapper for measurements process. 
    /// This class uses Detector, Sample, DataBase, FileStreamers so that control process of measurement.
    /// </summary>
    public class Measurement : IMeasurement, IDisposable
    {

        private ProcessManager _mProcessManager;
        private bool _isShowed;
        private int _spectraFile;
        private string _localDir;
        private string _remoteDir;
        private string _mType;
        private string _mOperatorName;
        private float _mHeight;
        private Detector _det;
        
       

        public Measurement(string detectorName, string operatorName)
        {
            _det = new Detector(detectorName);
            //_det.CountToRealTime = duration;
            //_det.FillInfo(ref s, type, operatorName, height);
            //MType = type;
            OperatorName = operatorName;
            //Height = height;
            _mProcessManager = new ProcessManager();
            _isShowed = false;
            SpectraFile = 0;
            //FIXME: adding directories directly is bad idea just for debugging
            LocalDir = $"D:\\Spectra\\{DateTimeStart.Year}\\{DateTimeStart.Month.ToString("D2")}\\{MType}\\";
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


        public string OperatorName
        {
            get { return _mOperatorName; }
            set
            {
                _mOperatorName = value;
            }
        }

        public float Height
        {
            get { return _mHeight; }
            set
            {
                _mHeight = value;
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
            await Task.Run(() => _det.AStart());
        }
        public void Continue()
        {
            _det.AContinue();
        }
        public void Stop()
        {
            _det.AStop();
        }
        public void Clear()
        {
            _det.AClear();
        }

        public void ShowDetectorInMvcg()
        {
            _mProcessManager.ShowDetectorInMvcg(_det.Name);
        }

        public void CompleteMeasurement()
        {
            DateTimeFinish = DateTime.Now;
        }
        public void SaveSpectraToFile()
        {

            _det.Save($"{LocalDir}\\{SpectraFile}");

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
            _det.Dispose();
        }


    }
}