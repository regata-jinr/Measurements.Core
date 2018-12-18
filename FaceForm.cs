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
        private readonly Dictionary<Status, Color> StatusColorDict = new Dictionary<Status, Color> { { Status.busy, Color.Gold }, { Status.ready, Color.LimeGreen }, { Status.off, Color.Gray }, { Status.error, Color.Red } };
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


            dets[0].Clear();
            dets[0].SpectroscopyAcquireSetup(CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime, 15);
            dets[0].AcquireStart();
            if (dets[0].AnalyzerStatus == CanberraDeviceAccessLib.DeviceStatus.aAcquireDone)
                Debug.WriteLine(dets[0].AnalyzerStatus.ToString());
            dets[0].Disconnect();
         

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
            CheckBox chb = new CheckBox();
            chb.Name = $"checkBox{d.Name}";
            chb.Text = d.Name;
            chb.Location = new Point(6 + number * 48, 15);
            chb.Size = new Size(42, 19);
            PictureBox pb = new PictureBox();
            pb.Name = $"PictureBox{d.Name}";
            pb.BackColor = StatusColorDict[d.Status];
            pb.Location = new Point(6+number*48, 31);
            pb.Size = new Size(13, 14);
            ToolTip tt = new ToolTip();
            tt.SetToolTip(pb, $"Status of detector is {StatusColorDict.FirstOrDefault(x => x.Value == pb.BackColor).Key}");
            if (d.Status == Status.error) tt.SetToolTip(pb, $"Status of detector is {StatusColorDict.FirstOrDefault(x => x.Value == pb.BackColor).Key}. Error message: {d.ErrorStr}");

            groupBoxDetectors.Controls.Add(chb);
            groupBoxDetectors.Controls.Add(pb);
        }

        private void buttonMeasure_Click(object sender, System.EventArgs e)
        {
            //SetListOfCommand 140page S560
        }
    }
}
