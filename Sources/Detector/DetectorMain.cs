﻿// Only methods of Detector class
// Basic descriptions see in DetectorFsPs.cs file

using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

namespace Measurements.Core
{
    public partial class Detector : IDetector, IDisposable
    {
        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        /// 
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

                if (!Directory.Exists(_baseDir))
                    Directory.CreateDirectory(_baseDir);

                _device            = new DeviceAccessClass();
                _currentSample     = new IrradiationInfo();
                CurrentMeasurement = new MeasurementInfo();
                
                Name = name;
                _device.DeviceMessages += ProcessDeviceMessages;
                _timeOutLimitSeconds = timeOutLimitSeconds * 1000;
                IsPaused = false;
                Connect();
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }
        }

        /// <summary>
        ///
        ///
        ///  |Advise Mask        |Description                                        |int value(lParam)|
        ///  |:-----------------:|:-------------------------------------------------:|:---------------:|
        ///  |DisplaySetting     | Display settings have changed                     |  1              |
        ///  |ExternalStart      | Acquisition has been started externall            |	1048608        |
        ///  |CalibrationChange  | A calibration parameter has changed               |	4              |
        ///  |AcquireStart       | Acquisition has been started                      |  134217728      |
        ///  |AcquireDone        | Acquisition has been stopped                      | -2147483648     |
        ///  |DataChange         | Data has been changes (occurs after AcquireClear) |	67108864       |
        ///  |HardwareError      | Hardware error                                    |	2097152        |
        ///  |HardwareChange     | Hardware setting has changed                      |	268435456      |
        ///  |HardwareAttention  | Hardware is requesting attention                  |	16777216       |
        ///  |DeviceUpdate       | Device settings have been updated                 |	8388608        |
        ///  |SampleChangerSet   | Sample changer set                                |	1073741824     |
        ///  |SampleChangeAdvance| Sample changer advanced                           |	4194304        |

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
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Has got message amHardwareError. Error Message is [{ErrorMessage}]", Level = NLog.LogLevel.Error });
                    isForCalling = true;
                }
                if ((int)AdviseMessageMasks.amAcquisitionParamChange == lParam)
                {
                    response = "Device ready to use!";

                }

                if (isForCalling)
                    AcquiringStatusChanged?.Invoke(this, new DetectorEventsArgs { Message = response, AcquireMessageParam = lParam, Name = this.Name, Status = this.Status });
            }
            catch (Exception ex)
            {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Has got error: [{ex.Message}]", Level = NLog.LogLevel.Error });
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
                catch (Exception ex)
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
                    if (ex.Message.Contains("278e2a")) Status = DetectorStatus.busy;

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
            catch (TimeoutException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Connection timeout.", Level = NLog.LogLevel.Warn });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
                if (ex.Message.Contains("278e2a")) Status = DetectorStatus.busy;

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
        /// Save current session on device.
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
            catch (ArgumentNullException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Input name of file spectra is empty. It should generated automatically. Something wrong in the sequence of actions.", Level = NLog.LogLevel.Error });
            }
            catch (InvalidOperationException)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector has connection problem", Level = NLog.LogLevel.Error });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }

        /// <summary>
        /// Disconnects from detector. Change status to off. Reset ErrorMessage. Not clearing the detector.
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
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
            }
        }

        /// <summary>
        /// Power reset for the detector. 
        /// </summary>
        public void Reset()
        {
            //TODO: test it
            try
            {
                _nLogger.Info($"Attempt to reset the detector");
                _device.SendCommand(DeviceCommands.aReset);

                if (Status == DetectorStatus.ready)
                    _nLogger.Info($"Resetting was successful");
                else
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Reset command was passed, but status is {Status}", Level = NLog.LogLevel.Warn });
            }
            catch (Exception ex) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
                {
                    Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"Detector is not ready for acquiring. Status is {Status}", Level = NLog.LogLevel.Warn });
                    return;
                }

                _device.AcquireStart(); // already async
                _nLogger.Info($"Acquiring in process...");
                Status = DetectorStatus.busy;
                CurrentMeasurement.DateTimeStart = DateTime.Now;
                CurrentMeasurement.Detector = Name;
            }
            catch (Exception ex) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
            catch (Exception ex) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
            catch (Exception ex) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
            catch (Exception ex) 
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = $"{ex.Message}", Level = NLog.LogLevel.Error });
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
                if (CurrentMeasurement == null || string.IsNullOrEmpty(CurrentMeasurement.Assistant) || string.IsNullOrEmpty(CurrentMeasurement.Type) || string.IsNullOrEmpty(CurrentSample.SampleKey))
                    throw new ArgumentNullException("Some of principal parameters is null. Probably you don't specify the sample");

                _nLogger.Info($"Filling information about sample: {CurrentMeasurement.ToString()}");

                _device.Param[ParamCodes.CAM_T_STITLE]      = $"{CurrentSample.SampleKey}";// title
                _device.Param[ParamCodes.CAM_T_SCOLLNAME]   = CurrentMeasurement.Assistant; // operator's name
                DivideString(CurrentSample.Note);           //filling description field in file
                _device.Param[ParamCodes.CAM_T_SIDENT]      = $"{CurrentMeasurement.SetKey}"; // sample code
                _device.Param[ParamCodes.CAM_F_SQUANT]      = (double)CurrentSample.Weight; // weight
                _device.Param[ParamCodes.CAM_F_SQUANTERR]   = 0; // err = 0
                _device.Param[ParamCodes.CAM_T_SUNITS]      = "gram"; // units = gram
                _device.Param[ParamCodes.CAM_T_BUILDUPTYPE] = "IRRAD";
                _device.Param[ParamCodes.CAM_X_SDEPOSIT]    = CurrentSample.DateTimeStart; // irr start date time
                _device.Param[ParamCodes.CAM_X_STIME]       = CurrentSample.DateTimeFinish; // irr finish date time
                _device.Param[ParamCodes.CAM_F_SSYSERR]     = 0; // Random sample error (%)
                _device.Param[ParamCodes.CAM_F_SSYSTERR]    = 0; // Non-random sample error (%)
                _device.Param[ParamCodes.CAM_T_STYPE]       = CurrentMeasurement.Type;
                _device.Param[ParamCodes.CAM_T_SGEOMTRY]    = CurrentMeasurement.Height.ToString();
            }
            catch (ArgumentNullException aex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = aex.Message, Level = NLog.LogLevel.Error });
            }
             catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(this, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }
       }

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

     public class DetectorEventsArgs : EventArgs
    {
        public string         Name;
        public DetectorStatus Status;
        public int            AcquireMessageParam;
        public string         Message;
    }


}

