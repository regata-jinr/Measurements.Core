using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace Measurements
{
    class GenieExeManager
    {
        //TODO: make sure that it doesn't creaty plenty processes.
        private Process proc;
        // private const string baseDir = @"C:\GENIE2K\EXEFILES";

        public GenieExeManager() {
            proc = new Process();
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding(1251); // иначе в файле будет греческий зал вместо русских символов
            proc.StartInfo.WorkingDirectory = @"C:\GENIE2K\EXEFILES";
        }

        // run processes method
        public void RunExe(string fileName, string args)
        {
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.Arguments = args;
            proc.Start();
        }

        public void PutView(string args) {RunExe("putview", args);}
        public void PvOpen(string args) {RunExe("pvopen", args);}
        public void PvClose(string args) {RunExe("pvclose", args);}
        public void StartMca(string args) {RunExe("startmca", args);}
        public void StopMca(string args) {RunExe("stopmca", args); }


        private void runProcess() //constructor create cmd process
        {
            try
            {
                proc.StartInfo.FileName = "cmd.exe";
                proc.Start();
            }
            catch (Exception ex) {logWrite("Exception", $"Exception type {ex.GetType()} {Environment.NewLine} Exception message: {ex.Message} {Environment.NewLine} Stack trace: {ex.StackTrace}");}
        }

          public void logWrite(string type, string message)
        {
            
            if (String.IsNullOrEmpty(FormLogin.user)) MessageBox.Show("Выберите пользователя!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Measurements.log"))
            {
                sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}  {FormLogin.user}  {type} {message}");
            }

        }

    }
}
