using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
using System.Drawing;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System;
using CanberraDataAccessLib;


// TODO: MAIN TODO LIST AFTER HOLIDAYS:

// TODO: create new OO design, define objects and their behaviour
// TODO: Define the interfaces before implementations and use Interface and implement it in classes.
// TODO: Create new folders structure
// TODO: 
// TODO: add logging(NLog);
// TODO: add tests(?);
// TODO: add documentation(doxygen);

// NOTE: implementations problems and extensions see in concrete parts of code.

namespace Measurements.WinForms
{
    public partial class FaceForm : Form
    {
        // public NLog.LogManager
        private Core.Classes.Detector[] dets;
        internal Core.Classes.Measurement mes;
        private string NameOfCheckedTypeRB;
        
        private void InitialsSettings()
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text += version.Substring(0, version.Length - 2);
            Text += $" - [{FormLogin.user}]";

            ToolStripMenuItemLoggingAll.Checked  = Properties.Settings.Default.logAll;
            ToolStripMenuItemLoggingInfo.Checked = Properties.Settings.Default.logInfo;
            ToolStripMenuItemLoggingWarn.Checked = Properties.Settings.Default.logWarn;
            ToolStripMenuItemLoggingErr.Checked  = Properties.Settings.Default.LogErr;
           
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
            var dets = new Core.Classes.Detector[filePaths.Length];
            int i = 0;
            foreach (string file in filePaths)
            {
                dets[i] = new Core.Classes.Detector(Path.GetFileNameWithoutExtension(file));
                i++;
            }
            string fileName = @"C:\GENIE2K\CAMFILES\1000001.CNF";
            DataAccessClass da = new DataAccessClass();
            if (!da.FileExists(fileName)) return;
            da.Open(fileName);


            //dets[0].Clear();
            //dets[0].SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, 15);
            //dets[0].AcquireStart();
            //if (dets[0].AnalyzerStatus == CanberraDeviceAccessLib.DeviceStatus.aAcquireDone)
            //    Debug.WriteLine(dets[0].AnalyzerStatus.ToString());
            //dets[0].Disconnect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitFromApp(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.logAll = ToolStripMenuItemLoggingAll.Checked;
            Properties.Settings.Default.logInfo = ToolStripMenuItemLoggingInfo.Checked;
            Properties.Settings.Default.logWarn = ToolStripMenuItemLoggingWarn.Checked;
            Properties.Settings.Default.LogErr = ToolStripMenuItemLoggingErr.Checked;
            Application.Exit();
        }


        //TODO: split processes. Now after run already open application doesn't allow to connect with detectors, but if I'll close it, I can run putview DET:D1(TASK?)
        private void buttonMeasure_Click(object sender, System.EventArgs e)
        {
            GenieExeManager gm = new GenieExeManager();
            //SetListOfCommand 140page S560
            foreach (var d in dets)
            {
                    d.Clear();
                    d.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, 15);
                    d.AcquireStart();
                   // gm.PutView($"/NO_DATASRC");
                   // gm.PvOpen($"DET:{d.Name} /EXPAND");
            }
        }

        private void FillJournalsDate(object sender, System.EventArgs e)
        {
            NameOfCheckedTypeRB = groupBoxTypes.Controls.OfType<RadioButton>()
              .FirstOrDefault(n => n.Checked).Name;
        }

        private void FillDataGridView(object sender, System.EventArgs e)
        {
           // Debug.WriteLine((DateTime)listBoxJournals.SelectedValue);
            //foreach (var ind in mes.UniqSetsCnt.Values)
            //                i += ind;
          ///  dataGridViewMeasurements.Rows[0].Cells[0].Bor
        }

    }
}
