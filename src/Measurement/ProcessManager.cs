using System;
using System.Diagnostics;

namespace MeasurementsCore
{
    class ProcessManager : IDisposable
    {
        //TODO: to make sure that it doesn't create plenty processes.
        private Process proc;
        private bool _isSpawned;
        // private const string baseDir = @"C:\GENIE2K\EXEFILES";
        public string OutputText { get; set; }
        public ProcessManager() {
            proc = new Process();
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            //NOTE: here might be problems with non-kyrillic localisations
            proc.StartInfo.StandardOutputEncoding = System.Text.Encoding.GetEncoding(1251);
            proc.StartInfo.WorkingDirectory = @"C:\GENIE2K\EXEFILES";
            _isSpawned = false;
            OutputText = "";

    }

    //TODO: find out how to check if it open so don't open it again
    public bool ShowDetectorInMvcg(string det)
        {
            var file = $"putview";
            var args = $"DET:{det} /read_only";
            var succeed = false;
            if (Process.GetProcessesByName("mvcg").Length == 0)
            {
                file = "putview.exe";
                _isSpawned = true;
            }
            else
            {
                file = "pvopen.exe";
            }

            proc.StartInfo.FileName = file;
            proc.StartInfo.Arguments = args;
            succeed = proc.Start();
            OutputText = proc.StandardOutput.ReadToEnd();
            return succeed;
        }

        public void Dispose()
        {
            //TODO: I can't use it, because detector opened from dlls. I should check disconnecting from detector close devise in mvcg or not.
            //TODO: It should be called only for last measurement process
            proc.StartInfo.FileName = "endview.exe";
            if (_isSpawned) proc.Start();
            proc.Dispose();
        }

    }
}
