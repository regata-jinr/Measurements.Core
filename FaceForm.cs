//using System;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Measurements
{
    public partial class FaceForm : Form
    {
        // public NLog.LogManager
        private Detector[] dets;
        internal Measurement mes;
        
        private void InitialsSettings()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text += version.Substring(0, version.Length - 2);
            Text += $" - [{FormLogin.user}]";

            ToolStripMenuItemLoggingAll.Checked = Properties.Settings.Default.logAll;
            ToolStripMenuItemLoggingInfo.Checked = Properties.Settings.Default.logInfo;
            ToolStripMenuItemLoggingWarn.Checked = Properties.Settings.Default.logWarn;
            ToolStripMenuItemLoggingErr.Checked = Properties.Settings.Default.LogErr;

           
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Debug("Hello World");

           
        }
        //TODO: add background worker for adding detectros controls
        //TODO: catch finishing of acquiring
        public FaceForm()
        {
            InitializeComponent();
            InitialsSettings();
            string[] filePaths = Directory.GetFiles(@"C:\GENIE2K\MIDFILES", "*.MID", SearchOption.TopDirectoryOnly);
            dets = new Detector[filePaths.Length];

            int i = 0;
            foreach (string file in filePaths)
            {
                dets[i] = new Detector(Path.GetFileNameWithoutExtension(file));
                AddDetectorControl(dets[i], i);
                i++;
            }

            

            //dets[0].Clear();
            //dets[0].SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, 15);
            //dets[0].AcquireStart();
            //if (dets[0].AnalyzerStatus == CanberraDeviceAccessLib.DeviceStatus.aAcquireDone)
            //    Debug.WriteLine(dets[0].AnalyzerStatus.ToString());
            //dets[0].Disconnect();
         

        }

        private void ExitFromApp(object sender, FormClosingEventArgs e)
        {

            Properties.Settings.Default.logAll = ToolStripMenuItemLoggingAll.Checked;
            Properties.Settings.Default.logInfo = ToolStripMenuItemLoggingInfo.Checked;
            Properties.Settings.Default.logWarn = ToolStripMenuItemLoggingWarn.Checked;
            Properties.Settings.Default.LogErr = ToolStripMenuItemLoggingErr.Checked;
            Application.Exit();

        }

        //TODO: size and location will not work for auto scale(maximize, grow and shrink...)
        private void AddDetectorControl(Detector d, int number)
        {
            d.DetForm.Location = new Point(6 + number * 48, groupBoxDetectors.Location.Y - d.DetForm.Size.Height/2 + 4);
            groupBoxDetectors.Controls.Add(d.DetForm);
            ToolStripMenuItemDetectors.DropDownItems.Add(d.Name);
        }

        private void buttonMeasure_Click(object sender, System.EventArgs e)
        {
            //SetListOfCommand 140page S560
            foreach (var det in dets)
            {
                Debug.WriteLine($"{det.Name} is Checked = {det.DetForm.Checked}");
            }
        }

   

        private void FillJournalsDate(object sender, System.EventArgs e)
        {
            listBoxJournals.DataSource = mes.GetJournalsDates(groupBoxTypes.Controls.OfType<RadioButton>()
              .FirstOrDefault(n => n.Checked).Name);
        }
    }
}
