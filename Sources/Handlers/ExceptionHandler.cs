using System;

namespace Measurements.Core.Handlers
{
    public static class ExceptionHandler
    {
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

                    _nLogger.Log(lvl, $"{ex.Source} has generated exception in [{ex.TargetSite.DeclaringType}--{ex.TargetSite.MemberType}--{ex.TargetSite.Name}]. The message is '{ex.Message}'{Environment.NewLine}Stack trace is:'{ex.StackTrace}'");
                    ExceptionEvent?.Invoke(obj, new ExceptionEventsArgs { Level = lvl, Message = ex.Message, Source = ex.Source, StackTrace = ex.StackTrace, TargetSite = ex.TargetSite.Name });
                }
                else
                {
                    _nLogger.Log(lvl, $"App has generated exception. The message is '{ex.Message}'");
                    ExceptionEvent?.Invoke(obj, new ExceptionEventsArgs { Level = lvl, Message = ex.Message, Source = "", StackTrace = "", TargetSite = "" });
                }
            }
            catch (Exception e)
            {
                 _nLogger.Log(NLog.LogLevel.Error, $"{e.Source} has generated exception from method {e.TargetSite.Name}. The message is '{e.Message}'{Environment.NewLine}Stack trace is:'{e.StackTrace}'");
                ExceptionEvent?.Invoke(null, new ExceptionEventsArgs { Level = NLog.LogLevel.Error, Message = e.Message, Source = e.Source, StackTrace = e.StackTrace, TargetSite = e.TargetSite.Name });

            }
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public NLog.LogLevel Level;
        public string Message;
        public string StackTrace;
        public string Source;
        public string TargetSite;
    }
}
