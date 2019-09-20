using System;
using System.Collections.Generic;

namespace Measurements.Core.Handlers
{
    public enum ExceptionLevel { Error, Info, Warn }
    public static class ExceptionHandler
    {
        private static Dictionary<ExceptionLevel, NLog.LogLevel> ExceptionLevel_LogLevel = new Dictionary<ExceptionLevel, NLog.LogLevel>{ { ExceptionLevel.Error, NLog.LogLevel.Error  },  { ExceptionLevel.Warn, NLog.LogLevel.Warn },  {ExceptionLevel.Info, NLog.LogLevel.Info }  }; 

        public static event Action<ExceptionEventsArgs> ExceptionEvent;
        private static NLog.Logger _nLogger = SessionControllerSingleton.logger;



        public static void ExceptionNotify(object obj, Exception ex, ExceptionLevel lvl)
        {
            try
            {
                ex.Data.Add("Assembly", "Measurements.Core");
                if (obj is Detector)
                    _nLogger.WithProperty("ParamName", ((Detector)obj).Name);
                if (obj is Session)
                    _nLogger.WithProperty("ParamName", ((Session)obj).Name);

                if (ex != null && !string.IsNullOrEmpty(ex.Source) && !string.IsNullOrEmpty(ex.TargetSite.Name) && !string.IsNullOrEmpty(ex.StackTrace) && !string.IsNullOrEmpty(ex.Message))
                {
                    System.Text.StringBuilder currentMessage = new System.Text.StringBuilder($"{ex.Source} has generated exception in [{ex.TargetSite.DeclaringType}--{ex.TargetSite.MemberType}--{ex.TargetSite.Name}]. The message is '{ex.Message}'{Environment.NewLine}Stack trace is:'{ex.StackTrace}'");
                    var currentMessageString = "";

                    if (currentMessage.Length <= 1998)
                        currentMessageString = currentMessage.ToString();
                    else
                        currentMessageString = currentMessage.ToString(0, 1998);


                    _nLogger.Log(ExceptionLevel_LogLevel[lvl], currentMessageString);
                    ExceptionEvent?.Invoke(new ExceptionEventsArgs { Level = lvl, exception = ex });
                }
                else
                {
                    _nLogger.Log(ExceptionLevel_LogLevel[lvl], $"App has generated exception. The message is '{ex.Message}'");
                    ExceptionEvent?.Invoke(new ExceptionEventsArgs { Level = lvl, exception = ex });
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Assembly", "Measurements.Core");
                 _nLogger.Log(NLog.LogLevel.Error, $"{e.Source} has generated exception from method {e.TargetSite.Name}. The message is '{e.Message}'{Environment.NewLine}Stack trace is:'{e.StackTrace}'");
                ExceptionEvent?.Invoke(new ExceptionEventsArgs { Level = ExceptionLevel.Error, exception = ex});

            }
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public ExceptionLevel Level;
        public Exception exception;
    }
}
