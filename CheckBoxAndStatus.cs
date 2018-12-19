using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Measurements
{
    public partial class CheckBoxAndStatus : UserControl
    {

        //TODO: add event CheckedChanged
        private ToolTip tt;
        public void SetStatusColor(Color clr)
        {
            pictureBox1.BackColor = clr;
        }

        public Color GetStatusColor()
        {
            return pictureBox1.BackColor;
        }

        public CheckBoxAndStatus()
        {
            InitializeComponent();
            tt = new ToolTip();
        }

        public bool Checked
        {
            get
            {
                return checkBox1.Checked;
            }
        }

        public void SetCheckBoxText(string name)
        {
            checkBox1.Name = $"CheckBoxAndStatusChB{name}";
            pictureBox1.Name = $"CheckBoxAndStatusPB{name}";
            checkBox1.Text = name;
        }

        public void SetTootTipText(string status)
        {
            tt.SetToolTip(this.pictureBox1, $"Status of detector is {status}");
        }
        public string GetCheckBoxName()
        {
            return checkBox1.Name;
        }

    
    }
}
