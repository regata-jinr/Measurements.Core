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
        public static string user;
        public FormLogin()
        {
            InitializeComponent();
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Text += version.Substring(0, version.Length - 2);
           this.ActiveControl = textBoxUser;
        }

        private void buttonEnter_Click(object sender, EventArgs e)
        {
            try
            {
                user = textBoxUser.Text;
                SqlConnection con = new SqlConnection();
                con.ConnectionString = $"{Properties.Resources.conString} User ID={user};Password={textBoxPass.Text};";
                con.Open();
                con.Close();
             
                var fm = new WinForms.FaceForm();
                Hide();
                fm.Show();
            }
            catch (SqlException) { MessageBox.Show($"Неверный логин или пароль.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void EnterPressed(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
                buttonEnter.PerformClick();
        }


    }
}
