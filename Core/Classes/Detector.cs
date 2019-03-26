using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using CanberraDeviceAccessLib;


//TODO: Add tests!
//TODO: Add logs!

/// <summary>
/// 
/// </summary>
namespace Measurements.Core.Classes
{
    /// <summary>
    ///  Enumeration of possible detector's working statuses
    ///  ready - Detector is enabled and ready for acquiring
    ///  off   - Detector is disabled
    ///  busy  - Detector is acquiring spectrum
    ///  error - Detector has porblems
    /// </summary>
    enum DetectorStatus { ready, off, busy, error}
    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>
    class Detector : IDisposable
    {

        protected delegate void ChangingStatusDelegate();
        protected delegate void AcquiringStatusDelegate();

        private DeviceAccessClass _device;
        private string _name;
        private string _type;
        private double _height;
        private DetectorStatus _detStatus;
        private ConnectOptions _conOption;
        private Sample _currentSample;
        protected delegate void EventsMethods();

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        protected Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite)
        {
            _name = name;
            Debug.WriteLine($"Current detector is {_name}");
            _conOption = option;
            ErrorMessage = "";
            _device = new DeviceAccessClass();
            _device.DeviceMessages += ProcessDeviceMessages;
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
            Debug.WriteLine($"Messages are got: {message}, {wParam}, {lParam}");
            Debug.WriteLine($"wParam: {wParam}");
            Debug.WriteLine($"lParam: {lParam}");
            if ((int)AdviseMessageMasks.amAcquireDone == lParam)
            {
                Debug.WriteLine($"Status will change to 'ready'");
                DetStatus = DetectorStatus.ready;
            }

            if ((int)AdviseMessageMasks.amAcquireStart == lParam)
            {
                Debug.WriteLine($"Det Status will change to 'busy'");
                DetStatus = DetectorStatus.busy;
            }

            if ((int)AdviseMessageMasks.amHardwareError == lParam)
            {
                Debug.WriteLine($"Det Status will change to 'error'");
                DetStatus = DetectorStatus.error;
                //TODO: figure out how to convert it to string
                // here I have MessageCodes. ... I sohuld catch what exactly code
                // I have and convert it to string via $"{_device.Message()}";
                ErrorMessage = $"{_device.Message(MessageCodes.PLC__HARD_ERR)}";
            }

        }

        //TODO: connect should has a timeout exception. In case I turn off vdm manager it stucks on connecting. But in such case with Task I can't be able to catch exception
        /// <summary>Overload method Connect from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}</param>
        protected async void ConnectAsync()
        {
            DetStatus = DetectorStatus.off;
            var task = new Task(() => _device.Connect(_name, _conOption, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings));
            if (await Task.WhenAny(task, Task.Delay(15000)) != task)
                HandleError("Time out error");
        }

        protected void Connect()
        {
            try
            {
                DetStatus = DetectorStatus.off;
                // thats already async
                _device.Connect(_name, _conOption, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings);
                DetStatus = DetectorStatus.ready;

            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("278e2a")) DetStatus = DetectorStatus.busy;
                else HandleError(ex.Message);
                
            }
            catch (Exception ex) { HandleError(ex.Message); }

        }

        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        protected DetectorStatus DetStatus
        {
            get { return _detStatus; }

            private set
            {
                if (_detStatus != value)
                {
                    _detStatus = value;
                    ChangingStatusEvent?.Invoke();
                    Debug.WriteLine($"Current detector status is {_detStatus}");
                }
            }
        }

        /// <summary>
        /// Returns true if high voltage is on.
        /// </summary>
        protected bool IsHV { get { return _device.HighVoltage.On; } }
        /// <summary>
        /// Returns true if detector connected successfully.
        /// </summary>
        protected bool IsConnected { get { return _device.IsConnected; } }
        /// <summary>
        /// Returns error message.
        /// </summary>
        protected string ErrorMessage { get; private set; }

        /// <summary>
        /// Recconects will trying to ressurect connection via detector. In case detector has status error or ready, it will do nothing. In case detector is off it will just call connect. In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
        protected void Reconnect()
        {
            if (_device.IsConnected) { Connect(); return; }
            Disconnect();
            Connect();
        }

        /// <summary>
        /// Save current session on device.
        /// </summary>
        protected void Save()
        {
            try
            {
                if (!_device.IsConnected) return;
                _device.Save(_name);
            }
            catch (Exception ex) { HandleError(ex.Message); }
        }

        /// <summary>
        /// Disconnects from detector. Change status to off. Reset ErrorMessage. Not clearing the detector.
        /// </summary>
        protected void Disconnect()
        {
            try
            {
                Save();
                _device.Disconnect();
                DetStatus = DetectorStatus.off;
                ErrorMessage = "";
            }
            catch (Exception ex) { HandleError(ex.Message); }
        }

        //TODO: find out how to call VDM.Control.Reset()
        /// <summary>
        /// 
        /// </summary>
        protected void Reset()
        {

        }
        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime, before this clear the device.
        /// </summary>
        /// <param name="time"></param>
        protected void AStart(int time)
        {
            try
            {
                _device.Clear();
                _device.AcquireStop();
                Debug.WriteLine($"Acquring is started with time {time}");
                DetStatus = DetectorStatus.busy;
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, time);
                _device.AcquireStart(); //already async

            }
            catch (Exception ex) { HandleError(ex.Message); }
        }
        /// <summary>
        /// Stops acquiring.
        /// </summary>
        protected void AStop()
        {
            try
            {
                _device.AcquireStop();
            }
            catch (Exception ex) { HandleError(ex.Message); }
        }
        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        protected void AClear()
        {
            try
            {
                _device.Clear();
            }
            catch (Exception ex) { HandleError(ex.Message); }
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        void IDisposable.Dispose()
        {
            Disconnect();
        }

        protected event Interfaces.ChangingStatusDelegate ChangingStatusEvent;
        protected event Interfaces.AcquiringStatusDelegate AcquiringCompletedEvent;

        /// <summary>
        /// Fill the sample information
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="type"></param>
        protected void FillSampleInfo(ref Sample sample, string type)
        {
            _device.Param[ParamCodes.CAM_T_STITLE] = $"{sample.SampleSetIndex}-{sample.SampleNumber}";// title
            //todo: dictionary for login - [last name] filling from db
            _device.Param[ParamCodes.CAM_T_SCOLLNAME] = FormLogin.user; // operator name
            _device.Param[ParamCodes.CAM_T_SDESC1] = sample.Description; // description 4 - row CAM_T_SDESC1-4
            _device.Param[ParamCodes.CAM_T_SIDENT] = $"{sample.SetKey}"; // sample code
            _device.Param[ParamCodes.CAM_F_SQUANT] = sample.Weight; // weight
            _device.Param[ParamCodes.CAM_F_SQUANTERR] = 0; // err = 0
            _device.Param[ParamCodes.CAM_T_SUNITS] = "gram"; // units = gram
            // type sample creation = irradiation by default
            _device.Param[ParamCodes.CAM_X_SDEPOSIT] = sample.IrradiationStartDateTime; // irr start date time
            _device.Param[ParamCodes.CAM_X_STIME] = sample.IrradiationFinishDateTime; // irr finish date time
            _device.Param[ParamCodes.CAM_F_SSYSERR] = 0; // Random sample error (%)
            _device.Param[ParamCodes.CAM_F_SSYSTERR] = 0; // Non-random sample error (%)
        }

        /// <summary>
        /// Type of current measurement.
        /// </summary>
        protected string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                _device.Param[ParamCodes.CAM_T_STYPE] = value;

            }
        }

        /// <summary>
        /// Property for geometry of sample. In our case this is height above detector.
        /// </summary>
        protected double Height
        {
            get {return _height;}
            set
            {
            _height = value;
            _device.Param[ParamCodes.CAM_T_SGEOMTRY] = value.ToString();
            } 
        }

        private void HandleError(string text)
        {
            DetStatus = DetectorStatus.error;
            ErrorMessage = text;
        }

    }
}
