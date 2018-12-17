using System;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Drawing;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Runtime.InteropServices;
using System.Deployment.Application;


// TODO: 
// add save of last measurments statement
// samples info should be automatically created
// should remember last exepimentator
// experimentator should not be empty, because it will use in log
// perhaps we should use db connections for authorizing
// add tooltip if mouse hover picturebox tooltip shows status

namespace Measurements
{
    public partial class MainForm : Form
    {

               
        public MainForm()
        {
           
            InitializeComponent();

            //if (String.IsNullOrEmpty(Properties.Settings.Default.user))



            //comboBoxExperimentator.Text = Properties.Settings.Default.user;

            var detectorsList = new List<Detector>();

            foreach (CheckBox CurCh in groupBoxDetectors.Controls.OfType<CheckBox>())
                detectorsList.Add(new Detector(CurCh.Text));

            foreach (Detector d in detectorsList)
                if (d.isOn) {
                    foreach (PictureBox CurPic in groupBoxDetectors.Controls.OfType<PictureBox>())
                        if (d.name.ToLower() == CurPic.Tag.ToString())
                            CurPic.BackColor = Color.Green;
                } 

            this.Text = "Measurements " + Application.ProductVersion;

            try
            {
               
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }


            //update message
            var UpdMsg = $"";
            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment current = ApplicationDeployment.CurrentDeployment;
                if (current.IsFirstRun) MessageBox.Show($"В новой версии программы {Application.ProductVersion} {Environment.NewLine} {UpdMsg}","Обновление программы измерений", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }

        private void ButStartMeasure_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxExperimentator_SelectionChangeCommitted(object sender, EventArgs e)
        {
        //    Properties.Settings.Default.user = comboBoxExperimentator.Text;
        }
    }
}
