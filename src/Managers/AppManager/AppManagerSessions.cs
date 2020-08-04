/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System.Collections.Generic;
using System;
using Regata.Measurements;

namespace Regata.Measurements.Managers
{
  public static partial class AppManager
  {
    /// <summary>
    /// The list of created session managed by SessionController
    /// </summary>
    public static Dictionary<string, Session> ActiveSessions { get; private set; }

    /// <summary>
    /// Allows user to create session from the scratch
    /// </summary>
    /// <returns></returns>
    public static Session CreateNewSession()
    {
      logger.Info("Creating of the new session instance");
      var session = new Session();
      ActiveSessions.Add(session.Name, session);
      return session;
    }

    /// <summary>
    /// Allow users to show active session
    /// </summary>
    /// <param name="sName">Name of saved session</param>
    /// <returns>Session object with filled information such as: Name of session, List of detectors, type of measurement, spreaded option, counts, countmode, height, assistant, note</returns>
    public static void ShowSession(string sName)
    {

      logger.Info($"Loading session with name '{sName}' from DB");
      try
      {
        if (string.IsNullOrEmpty(sName))
          throw new ArgumentNullException("Such session doesn't exist. Check the name or create the new one");

        ActiveSessions[sName].Show();
      }
      catch (ArgumentException ae)
      {
        NotificationManager.Notify(ae, NotificationLevel.Warning, Sender);
      }
      catch (Exception e)
      {
        NotificationManager.Notify(e, NotificationLevel.Error, Sender);
      }
    }

  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
