using System;
using System.Diagnostics;
using System.IO;
using CDA = CanberraDeviceAccessLib;

//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Threading.Tasks;
//using CanberraSequenceAnalyzerLib;
//using CanberraDataDisplayLib;
//using CanberraDataAccessLib;
//using CanberraReporterLib;


namespace Measurements
{
    class Detector
    {
        public string name;
        //status
        public bool isOn { get; private set; }
        public bool isHV;
        public bool isCold;
        public bool isBusy;
        public bool isError; // then text
        //status
        public double efficiency;

        private CanberraDeviceAccessLib.DeviceAccess Det = new CanberraDeviceAccessLib.DeviceAccess();

        /// <summary>
        /// Initializes for new detector object
        /// </summary>
        private void Init() {
        }
        /// <summary>
        /// Checks work status
        /// </summary>
        private void Checks() {
            try
            {
                Det.Connect(name);
                if (Det.IsConnected) isOn = true;
            }
            catch (Exception ex) { }

        }

        public Detector(string name) {
            this.name = name;
            Init();
            Checks();
        }




    }
}
