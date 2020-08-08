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
using System.Diagnostics;
using System.Linq;
using Microsoft.Data.SqlClient;

/// <summary>
/// More information about the design of this core, general schemas and content see in the readme file of this repository:
/// https://github.com/regata-jinr/MeasurementsCore
/// </summary>
namespace Regata.Measurements.Managers
{
  public static partial class AppManager
  {
    public static string Sender
    {
      get
      {
        var fr = new StackTrace().GetFrames().TakeLast(2).First();
        return $"{fr.GetMethod().ReflectedType}::{fr.GetMethod().Name}";
      }
    }

    public static bool IsAssmAlreadyRunning(string AssmName)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// For the logging we use NLog framework. Now we keep logs in the directory of application. 
    /// Logger creates 'MeasurementsLogs' folder nad file with name {short-date}.log.
    /// Also we consider implementation of web-monitor based on keeping logs in the data base.
    /// </summary>
    /// <see>
    public static NLog.Logger logger;

    /// <summary>
    /// Static constructor for initialization of all fields of the class
    /// </summary>
    static AppManager()
    {
      NLog.GlobalDiagnosticsContext.Set("LogConnectionString", SecretsManager.GetCredential(LogDbCredTarget).Secret);
      logger = NLog.LogManager.GetCurrentClassLogger();
      logger.SetProperty("Sender", typeof(AppManager).Name);
      logger.Info("Initialization of Measurements application has begun");

      _isDisposed = false;
      LocalMode = false;
      _mainConnectionStringBuilder = new SqlConnectionStringBuilder();
      _mainConnectionStringBuilder.ConnectionString = SecretsManager.GetCredential(MainDbCredTarget).Secret;
      ActiveSessions = new Dictionary<string, Session>();

      // TODO: in case of db update error local mode
      // TODO: in case of network problem local mode should be turning on automatically and this have to lead to local mode behaviour
      // Policy
      //     .Handle<SqlException>(ex => ex.InnerException.Message.Contains("network"))
      //     .RetryForeverAsync();

      // Policy.
      //     Handle<SqlException>(ex => ex.Message.Contains("Login failed"))
      //     .RetryAsync(3);

      logger.Info("App initialization has done!");
    }

    private static bool _isDisposed;

    public static void Dispose()
    {
      CleanUp(true);
    }

    private static void CleanUp(bool isDisposing)
    {

      if (!_isDisposed)
      {
        if (isDisposing)
        {
          ActiveSessions.Clear();
          DbContext.Dispose();
        }
      }
      _isDisposed = true;
    }

  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
