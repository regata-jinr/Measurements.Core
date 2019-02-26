using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using CanberraDeviceAccessLib;


//NOTE: Ending of task is not stop acquire. I should control it via VDM manager. I have a corresponding class here C:\GENIE2K\EXEFILES\ru
//NOTE: The good idea is add the implementation of auto calibration.
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
    enum AcquringStatus { done, started, stoped, error }
    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>
    class Detector : Interfaces.IDetector, IDisposable
    {
        private DeviceAccessClass _device;
        private string _name;
        private DetectorStatus _detStatus;
        private AcquringStatus _acqStatus;
        private ConnectOptions _conOption;
        public delegate void EventsMethods();

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite)
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
           // var dm = new DeviceMessages();
            var mm = new AdviseMessageMasks();
            mm = AdviseMessageMasks.amAcquireDone;

            if ((int)mm == lParam)
            {
                Debug.WriteLine($"Status will change to 'ready'");
                DetStatus = DetectorStatus.ready;
            }


        }

        //TODO: connect should has a timeout exception. In case I turn off vdm manager it stucks on connecting. But in such case with Task I can't be able to catch exception
        /// <summary>Overload method Connect from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}</param>
        async void ConnectAsync()
        {
                await Task.Run(() => Connect());
            //if (tsk.Wait(TimeSpan.FromSeconds(10))) Status = Status.ready;
            //else
            //{
            //    Status = Status.error;
            //    ErrorMessage = "Time out error";
            //}
        }

        void Connect()
        {
            try
            {
                DetStatus = DetectorStatus.off;
                _device.Connect(_name, _conOption, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings);
                DetStatus = DetectorStatus.ready;
                // var tsk = new Task(() => _device.Connect(_name, _conOption, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings));
                //tsk.RunSynchronously();
                //if (tsk.Wait(TimeSpan.FromSeconds(10))) Status = Status.ready;
                //else
                //{
                //    Status = Status.error;
                //    ErrorMessage = "Time out error";
                //}
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("278e2a")) DetStatus = DetectorStatus.busy;
                else
                {
                    DetStatus = DetectorStatus.error;
                    ErrorMessage = ex.Message;
                }
            }
            catch (Exception ex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = ex.Message;
            }

        }

        //TODO: for DetStatus and AcqStatus I have the same code. How to combine it into one?
        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        public DetectorStatus DetStatus
        {
            get { return _detStatus; }

            private set
            {
                if (_detStatus != value)
                {
                    _detStatus = value;
                    ChangedStatusEvent?.Invoke();
                    Debug.WriteLine($"Current detector status is {_detStatus}");
                }
            }
        }

        /// <summary>
        ///  Returns status of acquring process. {done, started, stoped, error}
        /// </summary>
        public AcquringStatus AcqStatus
        {
            get { return _acqStatus; }

            private set
            {
                if (_acqStatus != value)
                {
                    _acqStatus = value;
                    AcquiringCompletedEvent?.Invoke();
                    Debug.WriteLine($"Current acquiring status is {_acqStatus}");
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

        //TODO: 0. Impelement class for control and run exe files; //pvopen, pvclose,...
        //TODO: 1. Check if run via putview;
        //TODO: 2. if yes, call pvclose;
        //TODO: 3. if no, find process and close it;
        //TODO: 4. then user should choose continue or rewrite;
        //TODO: 5. for error status might be wrong!;
        //TODO: 6. try to use VDM for reset detector;
        //TODO: 7. So that connect to the busy detector, I've two ways - open it (actually it very usefull) and reset it and connect again.

        /// <summary>
        /// Recconects will trying to ressurect connection via detector. In case detector has status error or ready, it will do nothing. In case detector is off it will just call connect. In case status is busy, it will run recursively before 3 attempts with 5sec pausing.
        public void Reconnect()
        {
            if (_device.IsConnected) { Connect(); return; }
            Disconnect();
            Connect();
        }

        //TODO: add hard disconnect for dispose or maybe let it be soft disconnection by dispose should be hard disconnection
        /// <summary>
        /// Disconnects from detector. Change status to off. Reset ErrorMessage. Not clearing the detector.
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _device.Disconnect();
                DetStatus = DetectorStatus.off;
                ErrorMessage = "";
            }
            catch (Exception ex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = ex.Message;
            }
        }

        public void Reset()
        {
        }
        /// <summary>
        ///  Starts acquiring with specified aCountToLiveTime.
        /// </summary>
        /// <param name="time"></param>
        public void AStart(int time)
        {
            try
            {
                _device.Clear();
                _device.AcquireStop();
                Debug.WriteLine($"Acquring is started with time {time}");
                DetStatus = DetectorStatus.busy;
                _device.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, time);
                _device.AcquireStart();
                //await Task.Run(() =>_device.AcquireStart());
               // Status = Status.ready;
                

            }
            catch (Exception ex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = ex.Message;
            }
        }
        /// <summary>
        /// Stops acquiring.
        /// </summary>
        public void AStop()
        {
            try { _device.AcquireStop(); }
            catch (Exception ex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = ex.Message;
            }
        }
        /// <summary>
        /// Clears current acquiring status.
        /// </summary>
        public void AClear()
        {
            try { _device.Clear(); }
            catch (Exception ex)
            {
                DetStatus = DetectorStatus.error;
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Disconnects from detector. Changes status to off. Resets ErrorMessage. Clears the detector.
        /// </summary>
        void IDisposable.Dispose()
        {
            AClear();
            Disconnect();
        }

        public event Interfaces.ChangedStatusDelegate ChangedStatusEvent;
        public event Interfaces.AcquiringCompletedDelegate AcquiringCompletedEvent;
        public event EventHandler AStoped;
        public event EventHandler AErrorOccured;
        public event EventHandler ErrorOccured;
        public event EventHandler HVOff;
    }
}
