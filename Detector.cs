using System;
using System.Diagnostics;
using System.IO;

//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Threading.Tasks;
//using CanberraSequenceAnalyzerLib;
//using CanberraDataDisplayLib;
//using CanberraDataAccessLib;
//using CanberraReporterLib;
using CanberraDeviceAccessLib;

namespace Measurements
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
        
        /// <summary> Gets the error string. </summary>
        public string ErrorStr { get; private set; }
        //status
        public bool IsHV { get; private set; }
        /// <summary>Initializes for new detector object</summary>
        private void Init() {
            IsHV = this.HighVoltage.On;
            if (this.IsConnected) Status = Status.ready;
           
        }

        /// <summary>Constructor of Detector class. By default ConnectOptions is ReadWrite.</summary>
        /// <param name="name">Name of detector. Without path.</param>
        public Detector(string name) {
            try
            {
                Status = Status.off;
                this.Connect(name, ConnectOptions.aReadWrite);
                Init();
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
                Init();
            }
            catch (System.Runtime.InteropServices.COMException ex) { if (ex.Message.Contains("278e2a")) Status = Status.busy; }
        }

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
        public void ReConnect() {
            if (Status == Status.ready) return;
            if (Status == Status.off) { Connect(Name); return; }
            // else for error and busy

            Process[] proc = Process.GetProcessesByName("mvcg");
            if (proc[0] != null)
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = @"C:\Ge";
            }
            proc[0].Close();
        }




    }
}
