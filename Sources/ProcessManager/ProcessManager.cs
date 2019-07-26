using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace Measurements.Core
{
    public class ProcessManager
    {
        private const int _timeOut = 5;

        //TODO: find out how to check if it open so don't open it again
        public static async Task<ProcessResult> ShowDetectorInMvcg(string det)
        {
            var result = await ExecuteShellCommand("putview.exe", $"DET:{det} /READ_ONLY");
            return result;  
        }

        public static async Task<ProcessResult> OpenDetector(string det)
        {
            var result = await ExecuteShellCommand("pvopen.exe", $"DET:{det} /READ_ONLY");
            return result;  
        }

        public static async Task<ProcessResult> CloseDetector(string det)
        {
            var result = await  ExecuteShellCommand("pvclose.exe", $"DET:{det}");
            return result;  
        }


        public static async Task<ProcessResult> ExecuteShellCommand(string command, string arguments)
        {
            var result = new ProcessResult();

            using (var process = new Process())
            {

                process.StartInfo.FileName = command;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = @"C:\GENIE2K\EXEFILES";
                // Подписка на события записи в выходные потоки процесса

                var outputBuilder = new StringBuilder();
                var outputCloseEvent = new TaskCompletionSource<bool>();

                process.OutputDataReceived += (s, e) =>
                                                {
                                                // Поток output закрылся (процесс завершил работу)
                                                if (string.IsNullOrEmpty(e.Data))
                                                    {
                                                        outputCloseEvent.SetResult(true);
                                                    }
                                                    else
                                                    {
                                                        outputBuilder.AppendLine(e.Data);
                                                    }
                                                };

                var errorBuilder = new StringBuilder();
                var errorCloseEvent = new TaskCompletionSource<bool>();

                process.ErrorDataReceived += (s, e) =>
                                                {
                                                // Поток error закрылся (процесс завершил работу)
                                                if (string.IsNullOrEmpty(e.Data))
                                                    {
                                                        errorCloseEvent.SetResult(true);
                                                    }
                                                    else
                                                    {
                                                        errorBuilder.AppendLine(e.Data);
                                                    }
                                                };

                bool isStarted;

                try
                {
                    isStarted = process.Start();
                }
                catch (Exception error)
                {
                    // Usually it occurs when an executable file is not found or is not executable

                    result.Completed = true;
                    result.ExitCode = -1;
                    result.Output = error.Message;

                    isStarted = false;
                }

                if (isStarted)
                {
                    // Reads the output stream first and then waits because deadlocks are possible
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Creates task to wait for process exit using timeout
                    var waitForExit = WaitForExitAsync(process, _timeOut);

                    // Create task to wait for process exit and closing all output streams
                    var processTask = Task.WhenAll(waitForExit, outputCloseEvent.Task, errorCloseEvent.Task);

                    // Waits process completion and then checks it was not completed by timeout
                    if (await Task.WhenAny(Task.Delay(_timeOut), processTask) == processTask && waitForExit.Result)
                    {
                        result.Completed = true;
                        result.ExitCode = process.ExitCode;

                        // Adds process output if it was completed with error
                        if (process.ExitCode != 0)
                        {
                            result.Output = $"{outputBuilder}{errorBuilder}";
                        }
                    }
                    else
                    {
                        try
                        {
                            // Kill hung process
                            process.Kill();
                        }
                        catch
                        {
                        }
                    }
                }
            }

            return result;
        }


        private static Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return Task.Run(() => process.WaitForExit(timeout));
        }


        public struct ProcessResult
        {
            public bool Completed;
            public int? ExitCode;
            public string Output;
        }

    }
}
