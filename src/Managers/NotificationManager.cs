// TODO: I think that have to provide two ways for notification:
//          1. By hand in case of success or warning messages
//          2. Automatically based on exception
// TODO: How to convert exception to notificationeventsargs
// TODO: To process notification (show it via message box, or via email, user should subscribe event)


using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Regata.Measurements.Managers
{
    public enum NotificationLevel { Error, Info, Warning, Success }
    public static class NotificationManager
    {

        public static List<string> _mailSubs;

        public static void SubscribeEmail(string email)
        {
            
        }

        public static void CancelSubscription(string email)
        {
            
        }

        private static Dictionary<NotificationLevel, NLog.LogLevel> ExceptionLevel_LogLevel = new Dictionary<NotificationLevel, NLog.LogLevel>{ { NotificationLevel.Error, NLog.LogLevel.Error  },  { NotificationLevel.Warning, NLog.LogLevel.Warn },  { NotificationLevel.Info, NLog.LogLevel.Info }  }; 

        // FIXME: in case of one of the available detector has already opened (e.g. by hand not in read only mode)
        //       application will not run. The problem related with this static event!
        public static event Action<Notification> NotificationEvent;
        private static NLog.Logger _nLogger = AppManager.logger;


        private static Notification ConvertExceptionToNotification<TExcp>(TExcp ex, NotificationLevel nl) where TExcp : Exception
        {
            var notif =  new Notification { Level = nl, TechBody = ex.ToString(), BaseBody = ex.Message, Title = new StackTrace(ex).GetFrame(0).GetMethod().Name };

            if (notif.TechBody.Length > 1998)
                notif.TechBody = notif.TechBody.Substring(0, 1998);
            return notif;
        }

        public static void Notify<TObj, TExcp>(TObj sender, TExcp ex, NotificationLevel lvl)
            where TExcp : Exception
        {
            try
            {
                _nLogger.WithProperty("Sender", typeof(TObj).Name);

                var notif = ConvertExceptionToNotification<TExcp>(ex, lvl);

                _nLogger.Log(ExceptionLevel_LogLevel[lvl], notif.TechBody);
                 
                Notify(notif);
            }
            catch (Exception e)
            {
                if (NotificationEvent == null) return;
                e.Data.Add("Assembly", "Measurements.Core");
                var tstr = e.ToString();
                if (tstr.Length <= 1998)
                    _nLogger.Log(NLog.LogLevel.Error, e.ToString());
                else
                    _nLogger.Log(NLog.LogLevel.Error, e.ToString().Substring(0, 1998));
            }
        }

        public static void Notify(Notification nea)
        {
                NotificationEvent?.Invoke(nea);
        }

        private static void NotifyByEmail(Notification nea)
        {
            
        }


    }


    public class Notification : EventArgs
    {
        public NotificationLevel Level;
        public string Title;
        public string BaseBody;
        public string TechBody;

        public override string ToString()
        {
            return $"[{Level}] {Title}";
        }
    }
}
