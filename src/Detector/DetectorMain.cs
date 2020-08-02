// Only methods of Detector class
// Basic descriptions see in DetectorFsPs.cs file

using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public partial class Detector : IDetector, IDisposable
    {
        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite, int timeOutLimitSeconds = 10)
        {
            try
            {
                
                //_nLogger.SetProperty("Assistant", SessionControllerSingleton.ConnectionStringBuilder.UserID);
                _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", name);
                _nLogger.Info($"Initialisation of the detector {name} with mode {option.ToString()} and timeout limit {timeOutLimitSeconds}");

                _conOption         = option;
                _isDisposed        = false;
                Status             = DetectorStatus.off;
                ErrorMessage       = "";
                AcquisitionMode    = AcquisitionModes.aCountToRealTime;
                _device            = new DeviceAccessClass();
                CurrentMeasurement = new MeasurementInfo();

                if (CheckNameOfDetector(name))
                    _name = name;

                _device.DeviceMessages += ProcessDeviceMessages;
                _timeOutLimitSeconds   = timeOutLimitSeconds * 1000;
                IsPaused               = false;

                Connect();
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this,e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        ///
        ///  |Advise Mask        |Description                                        |int value(lParam)|
        ///  |:-----------------:|:-------------------------------------------------:|:---------------:|
        ///  |DisplaySetting     | Display settings have changed                     |  1              |
        ///  |ExternalStart      | Acquisition has been started externall            |  1048608        |
        ///  |CalibrationChange  | A calibration parameter has changed               |  4              |
        ///  |AcquireStart       | Acquisition has been started                      |  134217728      |
        ///  |AcquireDone        | Acquisition has been stopped                      | -2147483648     |
        ///  |DataChange         | Data has been changes (occurs after AcquireClear) |  67108864       |
        ///  |HardwareError      | Hardware error                                    |  2097152        |
        ///  |HardwareChange     | Hardware setting has changed                      |  268435456      |
        ///  |HardwareAttention  | Hardware is requesting attention                  |  16777216       |
        ///  |DeviceUpdate       | Device settings have been updated                 |  8388608        |
        ///  |SampleChangerSet   | Sample changer set                                |  1073741824     |
        ///  |SampleChangeAdvance| Sample changer advanced                           |  4194304        |

        /// </summary>
        /// <param name="message">DeviceMessages type from CanberraDeviceAccessLib</param>
        /// <param name="wParam">The first parameter of information associated with the message.</param>
        /// <param name="lParam">The second parameter of information associated with the message</param>
        private void ProcessDeviceMessages(int message, int wParam, int lParam)
        {
            string response = "";
            bool isForCalling = false;
            try
            {
                if ((int)AdviseMessageMasks.amAcquireDone == lParam && !IsPaused)
                {
                    _nLogger.Info($"Has got message AcquireDone.");
                    response = "Acquire has done";
                    Status = DetectorStatus.ready;
                    CurrentMeasurement.DateTimeFinish = DateTime.Now;
                    isForCalling = true;
                }

                if ((int)AdviseMessageMasks.amAcquireStart == lParam)
                {
                    _nLogger.Info($"Has got message amAcquireStart.");
                    response = "Acquire has start";
                    Status = DetectorStatus.busy;
                    isForCalling = true;
                }

                if ((int)AdviseMessageMasks.amHardwareError == lParam)
                {
                    Status = DetectorStatus.error;
                    ErrorMessage = $"{_device.Message((MessageCodes)lParam)}";
                    response = ErrorMessage;
                    isForCalling = true;
                }
                if ((int)AdviseMessageMasks.amAcquisitionParamChange == lParam)
                {
                    response = "Device ready to use!";

                }

                if (isForCalling)
                    AcquiringStatusChanged?.Invoke(this, new DetectorEventsArgs { Message = response, AcquireMessageParam = lParam, Name = this.Name, Status = this.Status });
            }
            catch (Exception e)
            {
                    Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        private Task ConnectInternalTask(CancellationToken c)
        {
            _nLogger.Debug($"Starts internal connection task to the detector");
            var t = Task.Run(() => 
            {
                try
                {
                    _device.Connect(_name, _conOption);
                    Status = DetectorStatus.ready;
                    c.ThrowIfCancellationRequested();
<<<<<<< HEAD:src/Detector/DetectorMain.cs
=======

>>>>>>> master:Sources/Detector/DetectorMain.cs
                }
                catch (Exception e)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                    if (e.Message.Contains("278e2a")) Status = DetectorStatus.busy;

                }
            });

            return t;
        }

        public async void ConnectAsync()
        {
            _nLogger.Info($"Starts async connecting to the detector.");

            CancellationTokenSource cts = new CancellationTokenSource();
            var ct = cts.Token;
            cts.CancelAfter(_timeOutLimitSeconds);

            await ConnectInternalTask(ct);
            

        }

        /// <summary>Overload Connect method from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        public void Connect()
        {
            try
            {
                _nLogger.Info($"Starts connecting to the detector");
                ConnectInternal();
                //TODO: Figure out how to add to internal connection timeout exception.
                //var task = new Task(() => ConnectInternal());
                //if (!task.Wait(TimeSpan.FromMilliseconds(_timeOutLimitSeconds)))
                //    throw new TimeoutException("Connection timeout");
                if (_device.IsConnected)
                    _nLogger.Info($"Connection to the detector was successful");

            }
            catch (TimeoutException te)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, te, Handlers.ExceptionLevel.Warn );
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        private void ConnectInternal()
        {
            try
            {
                _nLogger.Debug($"Starts internal connection to the detector");

                Status = DetectorStatus.off;
                _device.Connect(_name, _conOption);
                Status = DetectorStatus.ready;

                _nLogger.Debug($"Internal connection was successful");
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                if (e.Message.Contains("278e2a")) Status = DetectorStatus.busy;

            }

        }

        /// <summary>
        /// Recconects will trying to ressurect connection via detector. In case detector has status error or ready, it will do nothing. In case detector is off it will just call connect. In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
        public void Reconnect()
        {
            _nLogger.Info($"Attempt to reconnect to the detector.");
            if (_device.IsConnected) { Connect(); return; }
            Disconnect();
            Connect();
        }

        /// <summary>
        /// Save current measurement session on the device.
        /// </summary>
        public void Save(string fullFileName = "")
        {
            _nLogger.Info($"Starts saving of current session to file");
            try
            {
                if (!_device.IsConnected || Status == DetectorStatus.off)
                    throw new InvalidOperationException();

                if (string.IsNullOrEmpty(fullFileName))
                {
                    var _currentDir = $"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{CurrentMeasurement.Type.ToLower()}";
                    Directory.CreateDirectory(_currentDir);
                    fullFileName = $"{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf";
                }

                _device.Save($"{fullFileName}", true);
                FullFileSpectraName = fullFileName;

                if (File.Exists(FullFileSpectraName))
                    _nLogger.Info($"File '{FullFileSpectraName}' was saved");
                else _nLogger.Error($"Some problems during saving. File {CurrentMeasurement.FileSpectra}.cnf doesn't exist.");
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ie, Handlers.ExceptionLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// Disconnect from detector. Change status to off. Reset ErrorMessage. Not clearing the detector.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _nLogger.Info($"Disconnecting from the detector.");
                if (_device.IsConnected)
                    _device.Disconnect();
                _nLogger.Info($"Disconnecting was successful.");
                Status = DetectorStatus.off;
                ErrorMessage = "";
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// Power reset for the detector. 
        /// </summary>
        public void Reset()
        {
            //FIXME: not tested
            try
            {
                _nLogger.Info($"Attempt to reset the detector");
                _device.SendCommand(DeviceCommands.aReset);

                if (Status == DetectorStatus.ready)
                    _nLogger.Info($"Resetting was successful");
                else
                    throw new Exception($"Something were wrong during reseting of the detector '{Name}'");
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        private void SetAcquireCounts(int counts)
        {
            _nLogger.Info($"Detector get Acquisition mode - '{AcquisitionMode}' and count number - '{counts}'");
            _device.SpectroscopyAcquireSetup(AcquisitionMode, counts);
        }


        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime, before this the device will be cleared.
        /// </summary>
        /// <param name="time"></param>
        public void Start()
        {
            try
            {
                if (!IsPaused)
                    _device.Clear();

                IsPaused = false;

                if (Status != DetectorStatus.ready)
                    throw new Exception($"Status of detector '{Name}' is not 'ready'");
                _device.SpectroscopyAcquireSetup(AcquisitionModes.aCountToRealTime, CurrentMeasurement.Duration.Value);
                _device.AcquireStart(); // already async
                _nLogger.Info($"Acquiring in process...");
                Status = DetectorStatus.busy;
                CurrentMeasurement.DateTimeStart = DateTime.Now;
                CurrentMeasurement.Detector = Name;
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Set acquiring on pause.
        /// </summary>
        public void Pause()
        {
            try
            {
                if (Status == DetectorStatus.ready)
                    return;

                _nLogger.Info($"Attempt to set pause for the acquiring");
                _device.AcquirePause();
                IsPaused = true;
                Status = DetectorStatus.ready;
                _nLogger.Info($"Paused was successful. Detector ready to continue acquire process");
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Stops acquiring. Means pause and clear and **generate acquire done event.**
        /// </summary>
        public void Stop()
        {
            try
            {
                if (Status == DetectorStatus.ready)
                    return;
                _nLogger.Info($"Attempt to stop the acquiring");
<<<<<<< HEAD:src/Detector/DetectorMain.cs
                _device.AcquireStop(StopOptions.aNormalStop);
                //_device.SendCommand(DeviceCommands.aStop); // use command sending because in this case it will generate AcquireDone message
=======
                _device.SendCommand(DeviceCommands.aStop); // use command sending because in this case it will generate AcquireDone message
>>>>>>> master:Sources/Detector/DetectorMain.cs
                IsPaused = false;
                Status = DetectorStatus.ready;
                _nLogger.Info($"Stop was successful. Acquire done event will be generate. Detector ready to acquire again");
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }

        }

        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        public void Clear()
        {
            try
            {
                _nLogger.Info($"Clearing the detector");
                _device.Clear();
            }
            catch (Exception e) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }


        private void CleanUp(bool isDisposing)
        {
            _nLogger.Info($"Cleaning of the detector {Name}");

            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    NLog.LogManager.Flush();
                }
                Disconnect();
            }
            _isDisposed = true;
        }

        ~Detector()
        {
            CleanUp(false);
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        public void Dispose()
        {
            CleanUp(true);
            GC.SuppressFinalize(this);
        }


        public string GetParameterValue(ParamCodes parCode)
        {
            return _device.Param[parCode].ToString();
        }

        public void SetParameterValue<T>(ParamCodes parCode, T val)
        {
            try
            {
                if (val == null)
                    throw new ArgumentNullException($"{parCode} can't be a null");

                //if (Nullable.GetUnderlyingType(typeof(T)) != null)
                _device.Param[parCode] = val;
                _device.Save("", true);
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"A problem with saving information to file. {parCode} can't has a value {val}"), Handlers.ExceptionLevel.Warn);
            }
        }

        public void SetAcquisitionMode(AcquisitionModes mode)
        {
            _device.SpectroscopyAcquireSetup(mode);
        }


        /// <summary>
        /// Save information about measurement to device
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="irradiation"></param>        
        public void FillSampleInformation(MeasurementInfo measurement, IrradiationInfo irradiation)
        {
            if (!CheckIrradiationInfo(irradiation) || !CheckMeasurementInfo(measurement))
                return;

            try
            {
                CurrentMeasurement = measurement;
                RelatedIrradiation = irradiation;
                _device.Param[ParamCodes.CAM_T_STITLE]      = measurement.SampleKey;                  // title
                _device.Param[ParamCodes.CAM_T_SCOLLNAME]   = measurement.Assistant;                  // operator's name
                _device.Param[ParamCodes.CAM_T_SIDENT]      = measurement.SetKey;                     // sample code
                _device.Param[ParamCodes.CAM_F_SQUANT]      = (double)irradiation.Weight.Value;       // weight
                _device.Param[ParamCodes.CAM_F_SQUANTERR]   = 0;                                      // err = 0
                _device.Param[ParamCodes.CAM_T_SUNITS]      = "gram";                                 // units = gram
                _device.Param[ParamCodes.CAM_T_BUILDUPTYPE] = "IRRAD";
                _device.Param[ParamCodes.CAM_X_SDEPOSIT]    = irradiation.DateTimeStart.Value;        // irr start date time
                _device.Param[ParamCodes.CAM_X_STIME]       = irradiation.DateTimeFinish.Value;       // irr finish date time
                _device.Param[ParamCodes.CAM_F_SSYSERR]     = 0;                                      // Random sample error (%)
                _device.Param[ParamCodes.CAM_F_SSYSTERR]    = 0;                                      // Non-random sample error (%)
                _device.Param[ParamCodes.CAM_T_STYPE]       = measurement.Type;
                _device.Param[ParamCodes.CAM_T_SGEOMTRY]    = measurement.Height.Value.ToString("f"); // height

                AddEfficiencyCalibrationFile(measurement.Height.Value);

                DivideString(CurrentMeasurement.Note); //filling description field in file

                SetAcquireCounts(measurement.Duration.Value);

                _device.Save("", true);
            }
            catch(Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

        private bool CheckIrradiationInfo(IrradiationInfo irradiation)
        {
            bool isCorrect = true;
            try
            {
                if (irradiation == null)
                    throw new ArgumentException("Irradiated sample has not chosen");

                var type = typeof(IrradiationInfo);
                var neededProps = new string[] {"DateTimeStart", "Duration", "Weight"};

                foreach (var pi in type.GetProperties())
                {
                    if (neededProps.Contains(pi.Name) && pi.GetValue(irradiation) == null)
                        throw new ArgumentException($"{pi.Name} should not be null");
                }

                if (!irradiation.DateTimeFinish.HasValue)
                        irradiation.DateTimeFinish = irradiation.DateTimeStart.Value.AddSeconds(irradiation.Duration.Value);

                if (irradiation.DateTimeFinish.Value.TimeOfDay.TotalSeconds == 0)
                        irradiation.DateTimeFinish = irradiation.DateTimeStart.Value.AddSeconds(irradiation.Duration.Value);
            }
            catch(ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Warn);
                isCorrect = false;
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                isCorrect = false;
            }
            return isCorrect;
        }

        private bool CheckMeasurementInfo(MeasurementInfo measurement)
        {
            bool isCorrect = true;
            try
            {
                if (measurement == null)
                    throw new ArgumentException("Sample for measurement should not be null");

                var type = typeof(MeasurementInfo);
                var neededProps = new string[] {"Type", "Duration", "Height"};

                foreach (var pi in type.GetProperties())
                {
                    if (neededProps.Contains(pi.Name) && pi.GetValue(measurement) == null)
                        throw new ArgumentException($"{pi.Name} should not be null");
                }

                if (string.IsNullOrEmpty(measurement.Detector))
                    measurement.Detector = Name;

                if (measurement.Detector != Name)
                    throw new ArgumentException("Name of detector in db doesn't correspond to current detector for the sample");

                if (measurement.Duration.Value == 0)
                    throw new ArgumentException("Duration of the measurement process should not be equal to zero");

            }
            catch(ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Warn);
                isCorrect = false;
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
                isCorrect = false;
            }
            return isCorrect;
        }
      
        /// <summary>
        /// In spectra file we have four row for notes, Each row allows to keep 64 charatcer.
        /// This method divide a big string to these rows
        /// </summary>
        /// <param name="iStr"></param>
        private void DivideString(string iStr)
        {
            if (string.IsNullOrEmpty(iStr)) return;
            int descriptionsCount = iStr.Length / 65;

            switch (descriptionsCount)
            {
                case 0:
                    _device.Param[ParamCodes.CAM_T_SDESC1] = iStr;
                    break;
                case 1:
                    _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0,65);
                    _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66);
                    break;
                case 2:
                    _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66,65);
                    _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132);
                    break;
                case 3:
                    _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132,65);
                    _device.Param[ParamCodes.CAM_T_SDESC4] = iStr.Substring(198);
                    break;
                default:
                    _device.Param[ParamCodes.CAM_T_SDESC1] = iStr.Substring(0, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC2] = iStr.Substring(66, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC3] = iStr.Substring(132, 65);
                    _device.Param[ParamCodes.CAM_T_SDESC4] = iStr.Substring(198, 65);
                    break;
            }
        }

        public void AddEfficiencyCalibrationFile(decimal height)
        {
            try
            {
                if (height == 20)   height = 20m;
                if (height == 10)   height = 10m;
                if (height == 5)    height = 5m;
                if (height == 2.5m) height = 2.5m;

                string effFileName = $"C:\\GENIE2K\\CALFILES\\Efficiency\\{Name}\\{Name}-eff-{height.ToString().Replace('.', ',')}.CAL";

                if (!File.Exists(effFileName))
                    throw new FileNotFoundException($"Efficiency file {effFileName} not found!");

                
                _nLogger.Info($"Efficiency file {effFileName} will add to the detector");
                var effFile = new CanberraDataAccessLib.DataAccess();
                effFile.Open(effFileName);
                effFile.CopyBlock(_device, CanberraDataAccessLib.ClassCodes.CAM_CLS_GEOM);
                effFile.Close();
                _device.Save("", true);
            }
            catch (FileNotFoundException fnfe)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, fnfe, Handlers.ExceptionLevel.Warn);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
        }

    }

    /// <summary>
    /// This class shared information about events occured on the detector between callers.
    /// </summary>
     public class DetectorEventsArgs : EventArgs
    {
        public string         Name;
        public DetectorStatus Status;
        public int            AcquireMessageParam;
        public string         Message;
    }


}

