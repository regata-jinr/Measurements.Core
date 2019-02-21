using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;

using System.Collections.Generic;
//using System.Linq;
//using System.Text;

using System.Threading.Tasks;
//using CanberraSequenceAnalyzerLib;
//using CanberraDataDisplayLib;
//using CanberraDataAccessLib;
//using CanberraReporterLib;
using CanberraDeviceAccessLib;


//NOTE: Ending of task is not stop acquire. I should control it via VDM manager. I have a corresponding class here C:\GENIE2K\EXEFILES\ru
//NOTE: perhaps, add object of control class it's not a good idea. But it allow to not programming chain checkboxed checked event with some bool field in Detector.
namespace Measurements.Core.Classes
{
    enum Status {ready, off, busy, error}
    /// <summary>
    /// Detector is one of the main class, because detector is the main part of our experiment. It should be the most effective in all components: perfomance, safety, and so on. The good idea is add the implementation of auto calibration. Detector allows to manage real detector and should has protection from crashes.
    /// </summary>
    /// <seealso cref="CanberraDeviceAccessLib.DeviceAccessClass"/>
    /// TODO Edit XML Comment Template for Detector
    class Detector : DeviceAccessClass
    {
        /// <summary> Gets Status of detector. {ready, off, busy, error}. </summary>
        public Status Status { get; private set; }

        private readonly Dictionary<Status, Color> StatusColorDict = new Dictionary<Status, Color> { { Status.busy, Color.Gold }, { Status.ready, Color.LimeGreen }, { Status.off, Color.Gray }, { Status.error, Color.Red } };
        /// <summary> Gets the error string. </summary>
        public string ErrorStr { get; private set; }
        /// <summary>
        /// 
        ///
        /// </summary>
        public bool IsHV { get; private set; }

        /// <summary>Constructor of Detector class. By default ConnectOptions is ReadWrite.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        public Detector(string name) {
            try
            {
                Status = Status.off;
                this.Connect(name, ConnectOptions.aReadWrite);
                Status = Status.ready;
            }
            catch (System.Runtime.InteropServices.COMException ex) {
                if (ex.Message.Contains("278e2a")) Status = Status.busy;
                else
                {
                    Status = Status.error;
                    ErrorStr = ex.Message;        
                }
            }
        }

        /// <summary>Constructor of Detector class.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}</param>
        public Detector(string name, ConnectOptions option)
        {
            try
            {
                Status = Status.off;
                this.Connect(name, option);
                Status = Status.ready;
            }
            catch (System.Runtime.InteropServices.COMException ex) { if (ex.Message.Contains("278e2a")) Status = Status.busy; }
        }

        //TODO: connect should has a timeout exception. In case I turn off vdm manager it stucks on connecting.
        /// <summary>Overload method Connect from CanberraDeviceAccessLib. After second parametr always uses default values.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        /// <param name="option">CanberraDeviceAccessLib.ConnectOptions {aReadWrite, aContinue, aNoVerifyLicense, aReadOnly, aTakeControl, aTakeOver}</param>
        public void Connect(string name, ConnectOptions option) {
                this.Connect(name, option, AnalyzerType.aSpectralDetector, "", BaudRate.aUseSystemSettings);
        }

        //TODO: 0. Impelement class for control and run exe files; //pvopen, pvclose,...
        //TODO: 1. Check if run via putview;
        //TODO: 2. if yes, call pvclose;
        //TODO: 3. if no, find process and close it;
        //TODO: 4. then user should choose continue or rewrite;
        //TODO: 5. for error status might be wrong!;
        //TODO: 6. try to use VDM for reset detector;

        public void ReConnect() {
            if (Status == Status.ready) return;
            if (Status == Status.off) { Connect(Name); return; }
            // else for error and busy
            Connect(Name); return;
        }

        public Task Start()
        {
            return null;
        }
    }
}
