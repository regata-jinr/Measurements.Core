/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2020, REGATA Experiment at FLNP|JINR                       *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Regata.Measurements.Models;

namespace Regata.Measurements.Managers
{
  public enum DbConnectionStatus { Off, On };
  public static partial class AppManager
  {
    public static InfoContext DbContext { get; private set; }
    private static readonly SqlConnectionStringBuilder _mainConnectionStringBuilder;

    public static string UserId { get; private set; }

    public const string MainDbCredTarget = "MeasurementsMainConnectionString";
    public const string LogDbCredTarget = "MeasurementsLogConnectionString";
    public const string MailServiceTarget = "RegataMail";
    public const string DiskJinrTarget = "MeasurementsDiskJinr";

    public async static Task Login(string user = "", string PinOrPass = "")
    {
      try
      {
        logger.Info($"Application has started by user {user}");
        UserId = user;
        logger.SetProperty("Assistant", UserId);

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(PinOrPass))
        {
          logger.Info($"Empty user|passwords. Go to local mode");
          LocalMode = true;
          return;
        }

        _mainConnectionStringBuilder.UserID = user;

        if (IsPinExist() && PinOrPass == SecretsManager.GetCredential($"Pin_{user}")?.Secret)
        {
          logger.Info($"Check correction of pin");
          _mainConnectionStringBuilder.Password = SecretsManager.GetCredential($"Password_{user}").Secret;
        }
        else
        {
          logger.Info($"Enter via password");
          _mainConnectionStringBuilder.Password = PinOrPass;
        }

        logger.Info($"Trying to connect");
        DbContext = new InfoContext(_mainConnectionStringBuilder.ConnectionString);

        logger.Info($"Connection with DB successful");
      }
      catch (SqlException se)
      {
        if (se.Message.Contains("network"))
        {
          NotificationManager.Notify(new Notification { Level = NotificationLevel.Warning });
          LocalMode = true;
        }
        if (se.Message.Contains("Login failed"))
        {
          logger.Warn($"Connection failed. Wrong user or password.");
          NotificationManager.Notify(new Notification { Title = "Connection failed", BaseBody = "Wrong user name or password" });
          throw se;
        }
      }

    }

    public static void CreatePin(string pin)
    {
      if (pin.Length != 4 || !int.TryParse(pin, out _))
        throw new ArgumentException("Pin should have olny 4 digits");

      SecretsManager.SetCredential($"Pin_{UserId}", UserId, pin);
    }

    public static bool IsPinExist()
    {
      var upin = SecretsManager.GetCredential($"Pin_{UserId}");
      if (upin != null)
        return true;
      return false;
    }

    public static void RemovePin()
    {
      if (IsPinExist())
        SecretsManager.RemoveCredentials($"Pin_{UserId}");
    }

    /// <summary>
    /// This event will occur after reconnection with DB
    /// </summary>
    public static event Action DbConnectionStateChanged;

    private static DbConnectionStatus _dbConnCurrentState = DbConnectionStatus.Off;
    public static DbConnectionStatus DbConnectionState
    {
      get
      {
        var _newDbConnectionState = DbContext.Database.CanConnect() ? DbConnectionStatus.On : DbConnectionStatus.Off;

        if (_newDbConnectionState != _dbConnCurrentState)
        {
          DbConnectionStateChanged?.Invoke();
          LocalMode = (_newDbConnectionState == DbConnectionStatus.Off);
          return _newDbConnectionState;
        }
        return _dbConnCurrentState;
      }
    }

    /// <summary>
    /// Upload local file to the database in order to keep data after connection lost. 
    /// In case of success loading local files wil be delete.
    /// </summary>
    private static void UploadLocalDataToDB()
    {
      // var fileList = LoadMeasurementsFiles();
      // if (fileList.Count == 0) return;

      // using (var ic = new InfoContext())
      // {
      //   try
      //   {
      //     _nLogger.Info($"Local data has found. It will deserialize, load into db and then delete from local storage");

      //     ic.Measurements.UpdateRange(fileList);
      //     ic.SaveChanges();
      //     var dir = new DirectoryInfo(@"D:\LocalData");
      //     var files = dir.GetFiles("*.json").ToList();
      //     foreach (var file in files)
      //       file.Delete();
      //   }
      //   catch (Exception e)
      //   {
      //     NotificationManager.Notify(e, NotificationLevel.Error, Sender);
      //   }
      // }
    }
  } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
