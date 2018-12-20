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
using System;


// TODO: add logging(NLog);
// TODO: add tests(?);
// TODO: add documentation(doxygen);

// NOTE: implementations problems and extensions see in concrete parts of code.

namespace Measurements
{
    public partial class FaceForm : Form
    {
        // public NLog.LogManager
        private Detector[] dets;
        internal Measurement mes;
        private string NameOfCheckedTypeRB;
        
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
            d.DetForm.Click += new System.EventHandler(this.ColorizeDataGridView);
        }


        //TODO: split processes. Now after run already open application doesn't allow to connect with detectors, but if I'll close it, I can run putview DET:D1(TASK?)
        private void buttonMeasure_Click(object sender, System.EventArgs e)
        {
            GenieExeManager gm = new GenieExeManager();
            //SetListOfCommand 140page S560
            foreach (var d in dets)
            {
                if (d.DetForm.Checked)
                {
                    d.Clear();
                    d.SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, 15);
                    d.AcquireStart();
                   // gm.PutView($"/NO_DATASRC");
                   // gm.PvOpen($"DET:{d.Name} /EXPAND");
                }
            }

            

        }

   

        private void FillJournalsDate(object sender, System.EventArgs e)
        {
            NameOfCheckedTypeRB = groupBoxTypes.Controls.OfType<RadioButton>()
              .FirstOrDefault(n => n.Checked).Name;
            listBoxJournals.DataSource = mes.GetJournalsDates(NameOfCheckedTypeRB);
        }

        private void FillDataGridView(object sender, System.EventArgs e)
        {
           // Debug.WriteLine((DateTime)listBoxJournals.SelectedValue);
            dataGridViewMeasurements.DataSource = mes.GetMeasurementsData((DateTime)listBoxJournals.SelectedValue, NameOfCheckedTypeRB);
            //foreach (var ind in mes.UniqSetsCnt.Values)
            //                i += ind;
          ///  dataGridViewMeasurements.Rows[0].Cells[0].Bor



        }


        private void ColorizeDataGridView(object sender, System.EventArgs e)
        {
            dataGridViewMeasurements.DefaultCellStyle.BackColor = Color.White;
            int i = 0;
            int numChDets = groupBoxDetectors.Controls.OfType<CheckBoxAndStatus>().Count(n => n.Checked);
            Debug.WriteLine(numChDets);
            var detColor = new Dictionary<int, Color> { { 1, Color.AliceBlue }, { 5, Color.LightSlateGray }, { 6, Color.OldLace } };
            while (i < dataGridViewMeasurements.Rows.Count - 1)
            {
                foreach (var d in dets)
                {
                    if (d.DetForm.Checked)
                    {
                        Debug.WriteLine($"i-{i}|Name{d.Name}");
                        dataGridViewMeasurements.Rows[i].Cells[7].Value = int.Parse(d.Name[1].ToString());
                        dataGridViewMeasurements.Rows[i].DefaultCellStyle.BackColor = detColor[int.Parse(d.Name[1].ToString())];
                       i++;
                    }
                }
                if (i == 0) break;
            }
        }




    }
}
