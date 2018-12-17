using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Measurements
{
    class cmd
    {
        private Process cmdp;

        private void runProcess() //constructor create cmd process
        {
            try
            {
                cmdp = new Process();
                cmdp.StartInfo.FileName = "cmd.exe";
                cmdp.StartInfo.RedirectStandardInput = true;
                cmdp.StartInfo.RedirectStandardOutput = true;
                cmdp.StartInfo.CreateNoWindow = true;
                cmdp.StartInfo.UseShellExecute = false;
                cmdp.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding(866); //иначе в файле будет греческий зал вместо русских символов
                cmdp.Start();
            }
            catch (Exception ex) {logWrite("Exception", $"Exception type {ex.GetType()} {Environment.NewLine} Exception message: {ex.Message} {Environment.NewLine} Stack trace: {ex.StackTrace}");}
        }

          public void logWrite(string type, string message)
        {
            if (String.IsNullOrEmpty(Properties.Settings.Default.user)) MessageBox.Show("Выберите пользователя!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Measurements.log"))
            {
                sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}  {Properties.Settings.Default.user}  {type} {message}");
            }

        }

    }
}
