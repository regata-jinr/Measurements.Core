﻿using System;

namespace Measurements.Core.Handlers
{
    //fixme: now exception doesn't contain information about calling object!

    public static class ExceptionHandler
    {
        public static event EventHandler<ExceptionEventsArgs> ExceptionEvent;
        private static NLog.Logger _nLogger = SessionControllerSingleton.logger;

        public static void ExceptionNotify(object obj, ExceptionEventsArgs exceptionEventsArgs)
        {
            if (obj is Detector)
               SessionControllerSingleton.logger.WithProperty("DetName", ((Detector)obj).Name);

            _nLogger.Log(exceptionEventsArgs.Level, exceptionEventsArgs.Message);
            ExceptionEvent?.Invoke(obj, exceptionEventsArgs);
        }
    }

    public class ExceptionEventsArgs : EventArgs
    {
        public NLog.LogLevel Level;
        public string Message;
    }
}
