// Only methods of Detector class
// Basic descriptions see in DetectorFsPs.cs file

using System;
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
                _nLogger = SessionControllerSingleton.logger.WithProperty("ParamName", name);
                _nLogger.Info($"Initialisation of the detector {name} with mode {option.ToString()} and timeout limit {timeOutLimitSeconds}");
                _conOption = option;
                _isDisposed = false;
                Status = DetectorStatus.off;
                ErrorMessage = "";

                _device            = new DeviceAccessClass();
                _currentSample     = new IrradiationInfo();
                CurrentMeasurement = new MeasurementInfo();

                if (CheckNameOfDetector(name))
                    _name = name;

                _device.DeviceMessages += ProcessDeviceMessages;
                _timeOutLimitSeconds = timeOutLimitSeconds * 1000;
                IsPaused = false;
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
        public void Save()
        {
            _nLogger.Info($"Starts saving of current session to file");
            try
            {
                if (!_device.IsConnected || Status == DetectorStatus.off)
                    throw new InvalidOperationException();

                if (string.IsNullOrEmpty(CurrentMeasurement.FileSpectra))
                    throw new ArgumentNullException();

                FillFileInfo();

                var _currentDir = $"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{CurrentMeasurement.Type.ToLower()}";
                Directory.CreateDirectory(_currentDir);

                _device.Save($"{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf", true);
                FullFileSpectraName = $"{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf";

                if (File.Exists($"{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf"))
                    _nLogger.Info($"File '{_currentDir}\\{CurrentMeasurement.FileSpectra}.cnf' saved");
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

        public void SetAcqureCountsAndMode(int counts, CanberraDeviceAccessLib.AcquisitionModes mode = AcquisitionModes.aCountToRealTime)
        {
            _nLogger.Info($"Detector get Acquisition mode - '{mode}' and count number - '{counts}'");
            _device.SpectroscopyAcquireSetup(mode, counts);
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
                Status = DetectorStatus.ready;
                _nLogger.Info($"Paused was successful. Detector ready to continue acquire process");
                IsPaused = true;
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
                _device.AcquireStop();
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

        /// <summary>
        /// Fill the sample information
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="type"></param>
        private void FillFileInfo()
        {
            try
            {
                //FIXME: 2019-09-11 18:09:25.8397--ERROR----Canberra.DeviceAccess.1 has generated exception from method set_Param. The message is 'Error: ece99d7d. Programming error invalid calling argument.' Stack trace is:'   at CanberraDeviceAccessLib.DeviceAccessClass.set_Param(ParamCodes Params, Int32 lRec, Int32 lEntry, Object pVal)

                if (CurrentMeasurement == null || string.IsNullOrEmpty(CurrentMeasurement.Assistant) || string.IsNullOrEmpty(CurrentMeasurement.Type) || string.IsNullOrEmpty(CurrentSample.SampleKey))
                    throw new ArgumentNullException("Some of principal parameters is null. Probably you didn't specify the list of samples");

                _nLogger.Info($"Filling information about sample: {CurrentMeasurement.ToString()}");

                _device.Param[ParamCodes.CAM_T_STITLE]      = $"{CurrentSample.SampleKey}";// title
                _device.Param[ParamCodes.CAM_T_SCOLLNAME]   = CurrentMeasurement.Assistant; // operator's name
                DivideString(CurrentSample.Note);           //filling description field in file
                _device.Param[ParamCodes.CAM_T_SIDENT]      = $"{CurrentMeasurement.SetKey}"; // sample code

                if (CurrentSample.Weight.HasValue)
                    _device.Param[ParamCodes.CAM_F_SQUANT]  = (double)CurrentSample.Weight.Value; // weight
                else
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"Weight is empty for {CurrentSample}. Zero will set."), Handlers.ExceptionLevel.Warn);
                    _device.Param[ParamCodes.CAM_F_SQUANT]  = 0;
                }

                _device.Param[ParamCodes.CAM_F_SQUANTERR]   = 0; // err = 0
                _device.Param[ParamCodes.CAM_T_SUNITS]      = "gram"; // units = gram
                _device.Param[ParamCodes.CAM_T_BUILDUPTYPE] = "IRRAD";

                if (CurrentSample.DateTimeStart.HasValue)
                    _device.Param[ParamCodes.CAM_X_SDEPOSIT] = CurrentSample.DateTimeStart.Value; // irr start date time
                else
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"DateTimeStart is empty for {CurrentSample}. DateTime.Now will set."), Handlers.ExceptionLevel.Warn);
                    _device.Param[ParamCodes.CAM_X_SDEPOSIT] = DateTime.Now;
                }

                if (CurrentSample.DateTimeFinish.HasValue)
                    _device.Param[ParamCodes.CAM_X_STIME] = CurrentSample.DateTimeFinish.Value; // irr finish date time
                else
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"DateTimeFinish is empty for {CurrentSample}. DateTime.Now + duration will set."), Handlers.ExceptionLevel.Warn);
                    if (CurrentMeasurement.Duration.HasValue)
                        _device.Param[ParamCodes.CAM_X_STIME] = DateTime.Now.AddSeconds(CurrentMeasurement.Duration.Value);
                    else
                    {
                        Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"Duration also is empty. A counts to real time will add"), Handlers.ExceptionLevel.Warn);
                        _device.Param[ParamCodes.CAM_X_STIME] = DateTime.Now.AddSeconds(CountToRealTime);
                    }
                }

                _device.Param[ParamCodes.CAM_F_SSYSERR]     = 0; // Random sample error (%)
                _device.Param[ParamCodes.CAM_F_SSYSTERR]    = 0; // Non-random sample error (%)
                _device.Param[ParamCodes.CAM_T_STYPE]       = CurrentMeasurement.Type;

                if (CurrentMeasurement.Height.HasValue)
                    _device.Param[ParamCodes.CAM_T_SGEOMTRY] = CurrentMeasurement.Height.Value.ToString("f"); // height
                else
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Exception($"Height is empty for {CurrentSample}. Zero will set."), Handlers.ExceptionLevel.Warn);
                    _device.Param[ParamCodes.CAM_T_SGEOMTRY] = 0;
                }
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, ae, Handlers.ExceptionLevel.Error);
            }
             catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, e, Handlers.ExceptionLevel.Error);
            }
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

