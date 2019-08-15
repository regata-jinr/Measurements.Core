﻿using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using CanberraDeviceAccessLib;

//TODO: set up db target https://knightcodes.com/.net/2016/05/25/logging-to-a-database-wth-nlog.html

/// <summary>
/// This namespace contains implementations of core interfaces or internal classes.
/// </summary>
namespace Measurements.Core
{
    /// <summary>
    ///  Enumeration of possible detector's working statuses
    ///  ready - Detector is enabled and ready for acquiring
    ///  off   - Detector is disabled
    ///  busy  - Detector is acquiring spectrum
    ///  error - Detector has porblems
    /// </summary>
    public enum DetectorStatus { off, ready, busy, error }

    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>


    //TODO: improve logs readability
    //TODO: save logs to db
    public class Detector : IDetector, IDisposable
    {
        private DeviceAccessClass _device;
        private string _name;
        private int _timeOutLimitSeconds;
        private int _countToRealTime;
        private int _countToLiveTime;
        private int _countNormal;
        private DetectorStatus _detStatus;
        private ConnectOptions _conOption;
        public event EventHandler DetectorChangedStatusEvent;
        public event EventHandler<DetectorEventsArgs> DetectorMessageEvent;
        private bool _disposed;
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private NLog.Logger _nLogger;


        public string Name
        {
            get { return _name; }
            set
            {
                var detsList = (IEnumerable<object>)_device.ListSpectroscopyDevices;
                if (detsList.Contains(value))
                {
                    _name = value;
                    _nLogger.Info($"{value})--Detector with name '{value}' was found in the MID wizard list and will be used.");
                }
                else
                {

                    DetStatus = DetectorStatus.error;
                    ErrorMessage = $"{value})--Detector with name '{value}' wasn't find in the MID wizard list. Status will change to 'error'.";
                    GenerateWarnOrErr(NLog.LogLevel.Error, $"Detector({_name}). {ErrorMessage}");
                }
            }
        }
        public int CountToRealTime
        {
            get { return _countToRealTime; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountToRealTime.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime, value);
            }
        }

        public int CountToLiveTime
        {
            get { return _countToLiveTime; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountToLiveTime.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, value);
            }
        }

        public int CountNormal
        {
            get { return _countNormal; }
            set
            {
                _nLogger.Info($"{value})--Setting aCountNormal.");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountNormal, value);
            }
        }

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite, int timeOutLimitSeconds = 10)
        {
            _nLogger = logger.WithProperty("DetName", name);
            _nLogger.Info($"{name}, {option.ToString()}, {timeOutLimitSeconds})--Initializing of the detector.");
            _disposed = false;
            _conOption = option;
            DetStatus = DetectorStatus.off;
            ErrorMessage = "";
            _device = new DeviceAccessClass();
            Name = name;
            if (DetStatus == DetectorStatus.error)
            {
                _disposed = true;
                Dispose();
                return;
            }
            _device.DeviceMessages += ProcessDeviceMessages;
            _timeOutLimitSeconds = timeOutLimitSeconds * 1000;
            _countNormal = 0;
            _countToLiveTime = 0;
            _countToRealTime= 0;
            Connect();
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
            if ((int)AdviseMessageMasks.amAcquireDone == lParam)
            {
                _nLogger.Info($"{message}, {wParam}, {lParam})--Has got message AcquireDone.");
                DetStatus = DetectorStatus.ready;
            }

            if ((int)AdviseMessageMasks.amAcquireStart == lParam)
            {
                _nLogger.Info($"{message}, {wParam}, {lParam})--Has got message amAcquireStart.");
                DetStatus = DetectorStatus.busy;
            }

            if ((int)AdviseMessageMasks.amHardwareError == lParam)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = $"{_device.Message((MessageCodes)lParam)}";
                GenerateWarnOrErr(NLog.LogLevel.Error, $"{message}, {wParam}, {lParam})--Has got message amHardwareError. Error Message is [{ErrorMessage}]");
            }

        }

        private Task ConnectInternalTask(CancellationToken c)
        {
            _nLogger.Debug($")--Starts internal connection task to the detector");
            var t = Task.Run(() => 
            {
                try
                {
                    _device.Connect(_name, _conOption);
                    DetStatus = DetectorStatus.ready;
                    c.ThrowIfCancellationRequested();

                }
                catch (Exception ex)
                {
                    HandleError(ex);
                    if (ex.Message.Contains("278e2a")) DetStatus = DetectorStatus.busy;

                }
            });

            return t;
        }


        public async void ConnectAsync()
        {
            _nLogger.Info($")--Starts async connecting to the detector.");

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
                _nLogger.Info($")--Starts connecting to the detector.");
                ConnectInternal();
                //TODO: Figure out how to add to internal connection timeout exception.
                //var task = new Task(() => ConnectInternal());
                //if (!task.Wait(TimeSpan.FromMilliseconds(_timeOutLimitSeconds)))
                //    throw new TimeoutException("Connection timeout");
                if (_device.IsConnected)
                    _nLogger.Info($")--Connection to the detector was successful");

            }
            catch (TimeoutException tex)
            {
                HandleError(tex);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

        }

        private void ConnectInternal()
        {
            try
            {
                _nLogger.Debug($")--Starts internal connection to the detector");

                DetStatus = DetectorStatus.off;
                _device.Connect(_name, _conOption);
                DetStatus = DetectorStatus.ready;

                _nLogger.Debug($")--Internal connection was successful");
            }
            catch (Exception ex)
            {
                HandleError(ex);
                if (ex.Message.Contains("278e2a")) DetStatus = DetectorStatus.busy;

            }

        }

        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        public DetectorStatus DetStatus
        {
            get { return _detStatus; }

            private set
            {
                if (_detStatus != value)
                {
                    _nLogger.Info($"value)--The detector status changed from {_detStatus } to {value}");
                    _detStatus = value;
                    DetectorChangedStatusEvent?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Returns true if high voltage is on.
        /// </summary>
        public bool IsHV { get { return _device.HighVoltage.On; } }

        /// <summary>
        /// Returns true if detector connected successfully.
        /// </summary>
        public bool IsConnected { get { return _device.IsConnected; } }

        /// <summary>
        /// Returns error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Recconects will trying to ressurect connection via detector. In case detector has status error or ready, it will do nothing. In case detector is off it will just call connect. In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
        public void Reconnect()
        {
            _nLogger.Info($")--Attempt to reconnect to the detector.");
            if (_device.IsConnected) { Connect(); return; }
            Disconnect();
            Connect();
        }

        /// <summary>
        /// Save current session on device.
        /// </summary>
        public void Save(string fileName = "")
        {
            try
            {
                if (!_device.IsConnected || DetStatus == DetectorStatus.off)
                {
                    _nLogger.Warn($"{fileName})--Attempt to save current acquiring session, but detector doesn't have connection.");
                    return;
                }
                if (string.IsNullOrEmpty(fileName))
                {
                    _nLogger.Info($"{fileName})--Attempt to save current acquiring session to {NLog.LogManager.Configuration.Variables["basedir"]}\\Sessions.");
                    System.IO.Directory.CreateDirectory($"{NLog.LogManager.Configuration.Variables["basedir"]}\\Sessions");
                    _device.Save($"{NLog.LogManager.Configuration.Variables["basedir"]}\\Sessions\\{Name}-{DateTime.Now.ToString()}.cnf");
                }
                else
                {
                    if (DetStatus == DetectorStatus.error)
                        _nLogger.Warn($"{fileName})--Attempt to save current session, but some error occured [{ErrorMessage}].");

                    if (DetStatus == DetectorStatus.busy)
                        _nLogger.Warn($"{fileName})--Attempt to save current session, which still not finish.");
                    if (DetStatus == DetectorStatus.ready)
                        _nLogger.Info($"{fileName})--Attempt to save session in file");
                    if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName))) _device.Save(fileName, true);
                    else
                    {
                        GenerateWarnOrErr(NLog.LogLevel.Warn, $"{fileName})--Such directory doesn't exist. File will be save to C:\\GENIE2K\\CAMFILES\\");
                        _device.Save($"C:\\GENIE2K\\CAMFILES\\{System.IO.Path.GetFileName(fileName)}", true);
                    }
                }
                if (System.IO.File.Exists(fileName))
                    _nLogger.Info($"{fileName})--Saving was successful.");
                else _nLogger.Error($"{fileName})--After saving file doesn't exist!");
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Disconnects from detector. Change status to off. Reset ErrorMessage. Not clearing the detector.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _nLogger.Info($")--Disconnecting from the detector.");
                _device.Disconnect();
                DetStatus = DetectorStatus.off;
                ErrorMessage = "";
                if (!_device.IsConnected)
                    _nLogger.Info($")--Disconnecting was successful.");
                else _nLogger.Warn($")--The detector still have connection.");
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Power reset for the detector. 
        /// </summary>
        public void Reset()
        {
            try
            {
                //TODO: check that it do reseting in the meaning that I'm thinking about it!
                _nLogger.Info($")--Attempt to reset the detector");
                _device.SendCommand(DeviceCommands.aReset);
                if (DetStatus == DetectorStatus.ready) _nLogger.Info($")--Resetting was successful");
                else GenerateWarnOrErr(NLog.LogLevel.Warn, $")--Reset command was passed, but status is {DetStatus}");
            }
            catch (Exception ex) { HandleError(ex); }
        }

        public void Options(CanberraDeviceAccessLib.AcquisitionModes opt, int param)
        {
            _device.SpectroscopyAcquireSetup(opt, param);

        }


        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime, before this the device will be cleared.
        /// </summary>
        /// <param name="time"></param>
        public void Start()
        {
            try
            {
                _nLogger.Debug($")--Initializing of acquiring.");
                _device.Clear();

                if (DetStatus != DetectorStatus.ready)
                {
                    GenerateWarnOrErr(NLog.LogLevel.Warn, $")--Detector is not ready for acquiring. Status is {DetStatus}");
                    return;
                }
                _device.AcquireStart(); // already async
                _nLogger.Info($")--Acquiring in process...");
            }
            catch (Exception ex) { HandleError(ex); }
        }


        public void Continue()
        {
            if (DetStatus != DetectorStatus.ready)
            {
                GenerateWarnOrErr(NLog.LogLevel.Warn, $")--Detector is not ready for conitnue acquiring. Status is {DetStatus}");
                return;
            }
            _device.AcquireStart();
            _nLogger.Info($")--Acquiring will continue after pause");

        }


        /// <summary>
        /// Stops acquiring.
        /// </summary>
        public void Stop()
        {
            try
            {
                _nLogger.Info($")--Attempt to stop the acquiring");
                _device.AcquireStop();
                if (DetStatus == DetectorStatus.ready) _nLogger.Info($")--Stop was successful. Detector ready to acquire again");
                else GenerateWarnOrErr(NLog.LogLevel.Warn, $")--Stop command was passed, but status is {DetStatus}");
                //Save();
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        public void Clear()
        {
            try
            {
                _nLogger.Info($")--Clearing the detector");
                _device.Clear();
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        public void Dispose()
        {
            _nLogger.Info($")--Disposing the detector");
            if (!_disposed && DetStatus != DetectorStatus.off) Disconnect();
            _disposed = true;
            GC.SuppressFinalize(this);
            //NLog.LogManager.Shutdown();
        }

        /// <summary>
        /// Fill the sample information
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="type"></param>
        public void FillInfo(ref Sample sample, string mType, string operatorName, double height)
        {
            _nLogger.Info($")--Filling information about sample: {sample.ToString()}");
            _device.Param[ParamCodes.CAM_T_STITLE] = $"{sample.SampleSetIndex}-{sample.SampleNumber}";// title
            _device.Param[ParamCodes.CAM_T_SCOLLNAME] = operatorName; // operator's name
            DivideString(sample.Description);
            _device.Param[ParamCodes.CAM_T_SIDENT] = $"{sample.SetKey}"; // sample code
            _device.Param[ParamCodes.CAM_F_SQUANT] = sample.Weight; // weight
            _device.Param[ParamCodes.CAM_F_SQUANTERR] = 0; // err = 0
            _device.Param[ParamCodes.CAM_T_SUNITS] = "gram"; // units = gram
            _device.Param[ParamCodes.CAM_X_SDEPOSIT] = sample.IrradiationStartDateTime; // irr start date time
            _device.Param[ParamCodes.CAM_X_STIME] = sample.IrradiationFinishDateTime; // irr finish date time
            _device.Param[ParamCodes.CAM_F_SSYSERR] = 0; // Random sample error (%)
            _device.Param[ParamCodes.CAM_F_SSYSTERR] = 0; // Non-random sample error (%)
            _device.Param[ParamCodes.CAM_T_STYPE] = mType;
            _device.Param[ParamCodes.CAM_T_SGEOMTRY] = height.ToString();
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

        private void HandleError(Exception ex)
        {
            DetStatus = DetectorStatus.error;
            ErrorMessage = ex.Message;
            GenerateWarnOrErr(NLog.LogLevel.Error, $"Exception in Detector({_name}).{ex.Message}");
        }

        ~Detector()
        {
            _nLogger.Info($")--Deleting all data of the detector");
            Dispose();
        }


        private void GenerateWarnOrErr(NLog.LogLevel level, string text)
        {
            var dea = new DetectorEventsArgs();
            dea.level = level.Name;
            dea.text = text;
            _nLogger.Log(level, text);
            DetectorMessageEvent?.Invoke(this, dea);
        }

    }
    public class DetectorEventsArgs : EventArgs
    {
        public string level;
        public string text;
    }
}
