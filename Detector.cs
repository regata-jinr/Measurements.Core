using System;
using System.Diagnostics;
using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Threading.Tasks;
//using CanberraSequenceAnalyzerLib;
//using CanberraDataDisplayLib;
//using CanberraDeviceAccessLib;
//using CanberraDataAccessLib;
//using CanberraReporterLib;


namespace Measurements //почитать про namespace
{
    class Detector
    {
                        
        private void addToM(string content)
        {
           File.AppendAllText("C:/GENIE2K/EXEFILES/M.REX", content);
                       
        }

        public void ShowGenie()
        {
            File.WriteAllText("C:/GENIE2K/EXEFILES/M.REX", ""); //clear file
            addToM(Measurements.Properties.Settings.Default.StartStrings + Measurements.Properties.Settings.Default.ShowGenie);
            
        }

        public void addDetector(string detname)
        {
            addToM(Measurements.Properties.Settings.Default.addDetectors.Replace("detname",detname));
            
        }


    }
}
