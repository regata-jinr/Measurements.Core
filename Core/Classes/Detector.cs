using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
//using CanberraSequenceAnalyzerLib;
//using CanberraDataDisplayLib;
//using CanberraDataAccessLib;
//using CanberraReporterLib;
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
    enum Status {ready, off, busy, error}
    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It allows to manage real detector and has protection from crashes. You can start, stop and do any basics operations which you have with detector via mvcg.exe. This software based on dlls provided by [Genie2000] (https://www.mirion.com/products/genie-2000-basic-spectroscopy-software) for interactions with [HPGE](https://www.mirion.com/products/standard-high-purity-germanium-detectors) detectors also from [Mirion Tech.](https://www.mirion.com). Personally we are working with [Standard Electrode Coaxial Ge Detectors](https://www.mirion.com/products/sege-standard-electrode-coaxial-ge-detectors)
    /// </summary>
    /// <seealso cref="https://www.mirion.com/products/genie-2000-basic-spectroscopy-software"/>
    class Detector : Interfaces.IDetector, IDisposable
    {
        private DeviceAccessClass _device;
        private bool _isConnected;
        private string _name;
        private ConnectOptions _conOption;
        private Status _status;
        private string _errorMessage;

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}.By default ConnectOptions is ReadWrite.</param>
        public Detector(string name, ConnectOptions option = ConnectOptions.aReadWrite)
        {
            _name = name;
            _conOption = option;
            _errorMessage = "";
            _device = new DeviceAccessClass();
            Connect();
            IsConnected = _device.IsConnected;
        }

        //TODO: connect should has a timeout exception. In case I turn off vdm manager it stucks on connecting.
        /// <summary>Overload method Connect from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}</param>
        void Connect()
        {
            try
            {
                Status = Status.off;
                var tsk = Task.Run(() => _device.Connect(_name, _conOption, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings));
                if (tsk.Wait(TimeSpan.FromSeconds(10))) Status = Status.ready;
                else
                {
                    Status = Status.error;
                    ErrorMessage = "Time out error";
                }
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                if (ex.Message.Contains("278e2a")) Status = Status.busy;
                else
                {
                    Status = Status.error;
                    ErrorMessage = ex.Message;
                }
            }

        }


        /// <summary> Returns status of detector. {ready, off, busy, error}. </summary>
        /// <seealso cref="Enum Status"/>
        public Status Status { get; private set; }

        /// <summary>
        /// Returns true if high voltage is on.
        /// </summary>
        public bool IsHV { get; }
        public bool IsConnected { get; private set; }

        public string ErrorMessage { get; private set; }

        //TODO: 0. Impelement class for control and run exe files; //pvopen, pvclose,...
        //TODO: 1. Check if run via putview;
        //TODO: 2. if yes, call pvclose;
        //TODO: 3. if no, find process and close it;
        //TODO: 4. then user should choose continue or rewrite;
        //TODO: 5. for error status might be wrong!;
        //TODO: 6. try to use VDM for reset detector;

        public void Reconnect()
        {
            if (Status == Status.ready) return;
            if (Status == Status.off) {Connect(); return; }
            // else for error and busy
        }

        public void Disconnect()
        {
        }

        public void Reset()
        {
        }

        public Task AStart()
        {
            return null;
        }

        public void AStop()
        {
        }
        public void AClear()
        {
        }


        void IDisposable.Dispose()
        {
            Disconnect();
        }

        public event EventHandler StatusChanged;
        public event EventHandler ACompleted;
        public event EventHandler AStoped;
        public event EventHandler AGotError;
        public event EventHandler HVOff;
    }
}
