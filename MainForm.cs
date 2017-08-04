using System;
using System.Diagnostics;
using System.IO;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Runtime.InteropServices;

namespace Measurements
{
    public partial class MainForm : Form
    {


        public MainForm()
        {
           
            InitializeComponent();
            this.Text = "Measurements " + Application.ProductVersion;

           
            //Device.Connect("D1", ConnectOptions.aReadWrite, AnalyzerType.aSpectralDetector, "", CanberraDeviceAccessLib.BaudRate.aUseSystemSettings);
           // Device.Disconnect();
        

       
            try
            {
                //Debug.WriteLine("Start program!");
                Detector d = new Detector();

                // MessageBox.Show("Hi");
                d.ShowGenie();

                // d.StartGenie();
                d.addDetector("D1");
                d.addDetector("D5");


            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
            }

        private void ButStartMeasure_Click(object sender, EventArgs e)
        {
            cmd com = new cmd();

            com.runRex();
        }
    }
}
