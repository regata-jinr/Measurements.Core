using System;
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
               // cmdp.WaitForExit();
            }
            catch (Exception ex)
            {
                logWrite("Exception type " + ex.GetType() + Environment.NewLine +
         "Exception message: " + ex.Message + Environment.NewLine +
         "Stack trace: " + ex.StackTrace + Environment.NewLine, "Exception");
            }
        }

        public void runRex() //method for running the command and writing the logs
        {
            try
            {
                runProcess();
                cmdp.StandardInput.WriteLine("c:/ENTREXX/rexx.exe C:/GENIE2K/EXEFILES/M.REX");
                cmdp.StandardInput.Flush();
                cmdp.StandardInput.Close();
                cmdp.WaitForExit();
               

                logWrite(cmdp.StandardOutput.ReadToEnd(), "CmdOutPut");

               // cmdp.Close();
            }
            catch (Exception ex)
            {
                logWrite("Exception type " + ex.GetType() + Environment.NewLine +
        "Exception message: " + ex.Message + Environment.NewLine +
        "Stack trace: " + ex.StackTrace + Environment.NewLine, "Exception");
              
            }
        }

        public void logWrite(string output, string type)
        {
            using (StreamWriter sw = File.AppendText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/Measurements.log"))
            {
                sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm") + System.Environment.NewLine + type + System.Environment.NewLine + output);
            }

        }

    }
}
