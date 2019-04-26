using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CanberraDeviceAccessLib;

//TODO: set up db target https://knightcodes.com/.net/2016/05/25/logging-to-a-database-wth-nlog.html

/// <summary>
/// This namespace contains implementations of core interfaces or internal classes.
/// </summary>
namespace MeasurementsCore
{
    /// <summary>
    ///  Enumeration of possible detector's working statuses
    ///  ready - Detector is enabled and ready for acquiring
    ///  off   - Detector is disabled
    ///  busy  - Detector is acquiring spectrum
    ///  error - Detector has porblems
    /// </summary>
    public enum DetectorStatus { ready, off, busy, error }

    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>
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

        public string Name
        {
            get { return _name; }
            set
            {
                var detsList = (IEnumerable<object>)_device.ListSpectroscopyDevices;
                if (detsList.Contains(value))
                {
                    _name = value;
                    logger.Info($"Detector with name '{value}' was found in the MID wizard list and will be use.");
                }
                else
                {
                    DetStatus = DetectorStatus.error;
                    ErrorMessage = $"Detector with name '{value}' didn't define by the MID wizard. Status will change to 'error'.";
                    GenerateWarnOrErr(NLog.LogLevel.Error, $"Detector({_name}). {ErrorMessage}");
                }
            }
        }
        public int CountToRealTime
        {
            get { return _countToRealTime; }
            set
            {
                logger.Info($"Detector({_name}).CountToRealTime::Acquiring will start with aCountToRealTime = {value}");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime, value);
            }
        }

        public int CountToLiveTime
        {
            get { return _countToLiveTime; }
            set
            {
                logger.Info($"Detector({_name}).CountToLiveTime::Acquiring will start with aCountToLiveTime = {value}");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, value);
            }
        }

        public int CountNormal
        {
            get { return _countNormal; }
            set
            {
                logger.Info($"Detector({_name}).CountNormal::Acquiring will start with aCountNormal = {value}");
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountNormal, value);
            }
        }

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite, int timeOutLimitSeconds = 5)
        {

            logger.Info($"Detector({name}, {option.ToString()}, {timeOutLimitSeconds})::Starts to initialising of detector {name}");
            _disposed = false;
            _conOption = option;
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
            Connect();
        }

        /// <summary>
        ///
        ///
        ///  |Advise Mask        |Description                                        |int value    |
        ///  |:-----------------:|:-------------------------------------------------:|:-----------:|
        ///  |DisplaySetting     | Display settings have changed                     |  1          |
        ///  |ExternalStart      | Acquisition has been started externall            |	1048608    |
        ///  |CalibrationChange  | A calibration parameter has changed               |	4          |
        ///  |AcquireStart       | Acquisition has been started                      |  134217728  |
        ///  |AcquireDone        | Acquisition has been stopped                      | -2147483648 |
        ///  |DataChange         | Data has been changes (occurs after AcquireClear) |	67108864   |
        ///  |HardwareError      | Hardware error                                    |	2097152    |
        ///  |HardwareChange     | Hardware setting has changed                      |	268435456  |
        ///  |HardwareAttention  | Hardware is requesting attention                  |	16777216   |
        ///  |DeviceUpdate       | Device settings have been updated                 |	8388608    |
        ///  |SampleChangerSet   | Sample changer set                                |	1073741824 |
        ///  |SampleChangeAdvance| Sample changer advanced                           |	4194304    |

        /// </summary>
        /// <param name="message">DeviceMessages type from CanberraDeviceAccessLib</param>
        /// <param name="wParam">The first parameter of information associated with the message.</param>
        /// <param name="lParam">The second parameter of information associated with the message</param>
        private void ProcessDeviceMessages(int message, int wParam, int lParam)
        {
            if ((int)AdviseMessageMasks.amAcquireDone == lParam)
            {
                logger.Debug($"Detector({_name}).ProcessDeviceMessages({message},{wParam},{lParam})::From detector {_name} got message amAcquireDone. Status will change to 'ready'");
                DetStatus = DetectorStatus.ready;
            }

            if ((int)AdviseMessageMasks.amAcquireStart == lParam)
            {
                logger.Debug($"Detector({_name}).ProcessDeviceMessages({message},{wParam},{lParam})::From detector {_name} got message amAcquireStart. Status will change to 'busy'");
                DetStatus = DetectorStatus.busy;
            }

            if ((int)AdviseMessageMasks.amHardwareError == lParam)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = $"{_device.Message((MessageCodes)lParam)}";
                GenerateWarnOrErr(NLog.LogLevel.Error, $"Detector({_name}).ProcessDeviceMessages({message},{wParam},{lParam}). From detector {_name} got message amHardwareError. Status will change to 'error'. Error Message is [{ErrorMessage}]");
            }

        }

        /// <summary>Overload Connect method from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        public void Connect()
        {
            try
            {
                logger.Info($"Detector({_name}).Connect()::Starts connect to detector {_name}. Timeout value is {_timeOutLimitSeconds / 1000} sec.");
               // ConnectInternal();
               //FIXME: it's crash program.
                var task = new Task(() => ConnectInternal());
                if (!task.Wait(TimeSpan.FromMilliseconds(_timeOutLimitSeconds)))
                    throw new TimeoutException("Connection timeout");

                logger.Info($"Detector({_name}).Connect()::Connection to detector {_name} is successful");

            }
            catch (TimeoutException tex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = "Connection timeout";
                logger.Error(tex, $"Exception in Detector({_name}).Connect()::{tex.Message}");
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
                logger.Debug($"Detector({_name}).ConnectInternal()::Starts internal connection to detector {_name}");

                DetStatus = DetectorStatus.off;
                _device.Connect(_name, _conOption);
                DetStatus = DetectorStatus.ready;

                logger.Debug($"Detector({_name}).ConnectInternal()::Detector {_name} internal connection is successful");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("278e2a")) DetStatus = DetectorStatus.busy;
                else HandleError(ex);
            }
            catch (Exception ex)
            {
                HandleError(ex);
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
                    logger.Info($"Detector({_name}).DetStatus::Detector {_name} status changed from {_detStatus } to {value}");
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
            logger.Info($"Detector({_name}).Reconnect()::Attempt to reconnect to detector {_name}.");
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
                if (!_device.IsConnected) return;
                if (string.IsNullOrEmpty(fileName))
                {
                    logger.Info($"Detector({_name}).Save({fileName})::Attempt to save current acquiring session");
                    _device.Save(_name);
                }
                else
                {
                    logger.Info($"Detector({_name}).Save({fileName})::Attempt to save current acquiring session in file {fileName}");
                    if (System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(fileName))) _device.Save(fileName, true);
                    else
                    {
                        GenerateWarnOrErr(NLog.LogLevel.Warn, $"Detector({_name}).Save({fileName}).Such directory doesn't exist. File will be save to C:\\GENIE2K\\CAMFILES\\");
                        _device.Save($"C:\\GENIE2K\\CAMFILES\\{System.IO.Path.GetFileName(fileName)}", true);
                    }
                }
                logger.Info($"Detector({_name}).Save({fileName})::Saving is successful");
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
                logger.Info($"Detector({_name}).Disconnect()::Disconnecting from the detector");
                Save();
                _device.Disconnect();
                DetStatus = DetectorStatus.off;
                ErrorMessage = "";
                logger.Info($"Detector({_name}).Disconnect()::Disconnecting is successful");

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
                logger.Info($"Detector({_name}).Reset()::Attempt to reset the detector");
                _device.SendCommand(DeviceCommands.aReset);
                if (DetStatus == DetectorStatus.ready) logger.Info($"Detector({_name}).Reset()::Resetting is successful");
                else GenerateWarnOrErr(NLog.LogLevel.Warn, $"Detector({_name}).Reset().Reset command was passed, but status is {DetStatus}");
            }
            catch (Exception ex) { HandleError(ex); }
        }

        public void AOptions(CanberraDeviceAccessLib.AcquisitionModes opt, int param)
        {
            _device.SpectroscopyAcquireSetup(opt, param);

        }


        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime, before this clear the device.
        /// </summary>
        /// <param name="time"></param>
        public void AStart()
        {
            try
            {
                logger.Info($"Detector({_name}).AStart()::Initialising starting of acquire. Clearing the device:");
                _device.Clear();
                logger.Info($"Detector({_name}).AStart()::Clearing was successful");

                if (DetStatus != DetectorStatus.ready)
                {
                    GenerateWarnOrErr(NLog.LogLevel.Warn, $"Detector({_name}).AStart(). Detector is not ready for acquiring. Status is {DetStatus}");
                    return;
                }
                _device.AcquireStart(); // already async
                DetStatus = DetectorStatus.busy;
                logger.Info($"Detector({_name}).AStart()::Acquiring in process...");

            }
            catch (Exception ex) { HandleError(ex); }
        }


        public void AContinue()
        {
            if (DetStatus != DetectorStatus.ready)
            {
                GenerateWarnOrErr(NLog.LogLevel.Warn, $"Detector({_name}).AContinue(). Detector is not ready for acquiring. Status is {DetStatus}");
                return;
            }
            logger.Info($"Detector({_name}).AContinue()::Acquiring will continue after pause");
            _device.AcquireStart();
        }


        /// <summary>
        /// Stops acquiring.
        /// </summary>
        public void AStop()
        {
            try
            {
                logger.Info($"Detector({_name}).AStop()::Attempt to stop the acquiring");
                _device.AcquireStop();
                if (DetStatus == DetectorStatus.ready) logger.Info($"Detector({_name}).AStop()::Stopping was successful. Detector ready to acquire again");
                else GenerateWarnOrErr(NLog.LogLevel.Warn, $"Detector({_name}).AStop(). Stop command was passed, but status is {DetStatus}");
                Save();
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        public void AClear()
        {
            try
            {
                logger.Info($"Detector({_name}).AClear()::Clearing the detector");
                _device.Clear();
            }
            catch (Exception ex) { HandleError(ex); }
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        public void Dispose()
        {
            logger.Info($"Detector({_name}).Dispose()::Disposing the detector");
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
        public void FillInfo(ref Sample sample, string mType, string operatorName, float height)
        {
            logger.Info($"Detector({_name}).FillSampleInfo()::Filling information about sample: {sample.ToString()}");
            _device.Param[ParamCodes.CAM_T_STITLE] = $"{sample.SampleSetIndex}-{sample.SampleNumber}";// title
            //todo: dictionary for login - [last name] filling from db
            _device.Param[ParamCodes.CAM_T_SCOLLNAME] = operatorName; // operator's name
            DivideString(sample.Description);
            _device.Param[ParamCodes.CAM_T_SIDENT] = $"{sample.SetKey}"; // sample code
            _device.Param[ParamCodes.CAM_F_SQUANT] = sample.Weight; // weight
            _device.Param[ParamCodes.CAM_F_SQUANTERR] = 0; // err = 0
            _device.Param[ParamCodes.CAM_T_SUNITS] = "gram"; // units = gram
            // type sample creation = irradiation by default
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
            logger.Info($"~Detector({_name})::Deleting all data of the detector");
            Dispose();
        }


        private void GenerateWarnOrErr(NLog.LogLevel level, string text)
        {
            var dea = new DetectorEventsArgs();
            dea.level = level.Name;
            dea.text = text;
            logger.Log(level, text);
            DetectorMessageEvent?.Invoke(this, dea);
        }

    }
    public class DetectorEventsArgs : EventArgs
    {
        public string level;
        public string text;
    }
}

