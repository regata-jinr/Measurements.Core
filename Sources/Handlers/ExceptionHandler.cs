using System;

namespace Measurements.Core.Handlers
{
    public static class ExceptionHandler
    {
        public static event EventHandler<ExceptionEventsArgs> ExceptionEvent;
        private static NLog.Logger _nLogger = SessionControllerSingleton.logger;

        public static void ExceptionNotify(object obj, ExceptionEventsArgs exceptionEventsArgs)
        {
            _nLogger.Log(exceptionEventsArgs.Level, $"Exception in the object {obj?.ToString()} has occurred. The message is {Environment.NewLine}{exceptionEventsArgs.Message}");
            ExceptionEvent?.Invoke(obj, exceptionEventsArgs);
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public NLog.LogLevel Level;
        public string Message;
    }
}
