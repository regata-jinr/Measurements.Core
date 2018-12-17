using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Measurements
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {
            try
            {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = "";
            con.Open();
            con.Close();
            var fm = new NewFaceForm();
            this.Hide();
            fm.Show();
            }
            catch (SqlException sqlEx) { MessageBox.Show($"SQL exception:\n {sqlEx.ToString()}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
