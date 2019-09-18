using System;
using System.Collections.Generic;

namespace Measurements.Core.Handlers
{
    public enum ExceptionLevel { error, info, warning }
    public static class ExceptionHandler
    {
        private static Dictionary<NLog.LogLevel, ExceptionLevel> LogLevel_ExceptionLevel = new Dictionary<NLog.LogLevel, ExceptionLevel>{ {NLog.LogLevel.Error, ExceptionLevel.error },  {NLog.LogLevel.Warn, ExceptionLevel.warning },  {NLog.LogLevel.Info, ExceptionLevel.info }  }; 
        public static event EventHandler<ExceptionEventsArgs> ExceptionEvent;
        private static NLog.Logger _nLogger = SessionControllerSingleton.logger;

        public static void ExceptionNotify(object obj, Exception ex, NLog.LogLevel lvl)
        {
            try
            {
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


                    _nLogger.Log(lvl, currentMessageString);
                    ExceptionEvent?.Invoke(obj, new ExceptionEventsArgs { Level = LogLevel_ExceptionLevel[lvl], Message = ex.Message, Source = ex.Source, StackTrace = ex.StackTrace, TargetSite = ex.TargetSite.Name });
                }
                else
                {
                    _nLogger.Log(lvl, $"App has generated exception. The message is '{ex.Message}'");
                    ExceptionEvent?.Invoke(obj, new ExceptionEventsArgs { Level = LogLevel_ExceptionLevel[lvl], Message = ex.Message, Source = "", StackTrace = "", TargetSite = "" });
                }
            }
            catch (Exception e)
            {
                 _nLogger.Log(NLog.LogLevel.Error, $"{e.Source} has generated exception from method {e.TargetSite.Name}. The message is '{e.Message}'{Environment.NewLine}Stack trace is:'{e.StackTrace}'");
                ExceptionEvent?.Invoke(null, new ExceptionEventsArgs { Level = ExceptionLevel.error, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace, TargetSite = e.TargetSite.Name });

            }
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public ExceptionLevel Level;
        public string Message;
        public string StackTrace;
        public string Source;
        public string TargetSite;
    }
}
