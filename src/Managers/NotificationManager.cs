/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

// TODO: I think that have to provide two ways for notification:
//          1. By hand in case of success or warning messages
//          2. Automatically based on exception
// TODO: How to convert exception to notificationeventsargs
// TODO: To process notification (show it via message box, or via email, user should subscribe event)

using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net;

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

    private static Dictionary<NotificationLevel, NLog.LogLevel> ExceptionLevel_LogLevel = new Dictionary<NotificationLevel, NLog.LogLevel> { { NotificationLevel.Error, NLog.LogLevel.Error }, { NotificationLevel.Warning, NLog.LogLevel.Warn }, { NotificationLevel.Info, NLog.LogLevel.Info } };

    // FIXME: in case of one of the available detector has already opened (e.g. by hand not in read only mode)
    //       application will not run. The problem related with this static event!
    public static event Action<Notification> NotificationEvent;
    private static NLog.Logger _nLogger = AppManager.logger;


    private static Notification ConvertExceptionToNotification<TExcp>(TExcp ex, NotificationLevel nl, string sender) where TExcp : Exception
    {
      var notif = new Notification
      {
        Level = nl,
        TechBody = ex.ToString(),
        BaseBody = ex.Message,
        Title = $"({typeof(TExcp).Name}) {sender}"
      };

      if (notif.TechBody.Length > 1998)
        notif.TechBody = notif.TechBody.Substring(0, 1998);
      return notif;
    }

    public static void Notify<TExcp>(TExcp ex, NotificationLevel lvl, string sender, bool NotifyByEmail = false)
        where TExcp : Exception
    {
      try
      {
        _nLogger.WithProperty("Sender", sender);

        var notif = ConvertExceptionToNotification(ex, lvl, sender);

        _nLogger.Log(ExceptionLevel_LogLevel[lvl], notif.TechBody);

        Notify(notif, NotifyByEmail || lvl == NotificationLevel.Error);
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

    public static void Notify(Notification nea, bool NotifyByEmail = false)
    {
      NotificationEvent?.Invoke(nea);

      if (NotifyByEmail)
        SendNotificationByEmail(nea);
    }

    // FIXME in case of network error will this suspend the app despite of SendAsync?
    private static void SendNotificationByEmail(Notification nea)
    {
      var mail = SecretsManager.GetCredential(AppManager.MailServiceTarget).Name;
      var mailPass = SecretsManager.GetCredential(AppManager.MailServiceTarget).Secret;
      using (var smtp = new SmtpClient
      {
        Host = "smtp.jinr.ru",
        Port = 465,
        EnableSsl = true,
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        Credentials = new NetworkCredential(mail, mailPass)
      })
      {
        foreach (var toAddress in _mailSubs)
        {
          using (var message = new MailMessage(mail, toAddress)
          {
            Subject = nea.ToString(),
            Body = string.Join(
                nea.BaseBody,
                Environment.NewLine,
                "===*******TECH INFO*******==",
                Environment.NewLine,
                nea.TechBody
                )
          })
          {
            smtp.SendAsync(message, null);
          }
        }
      }
    }
  } // public static class NotificationManager

  public class Notification : EventArgs
  {
    public NotificationLevel Level;
    public string BaseBody;
    public string TechBody;
    public string Title;
    public override string ToString() => $"[{Level}] {Title}";
  }
} // namespace Regata.Measurements.Managers
