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
using System.Net.Http;
using System.Threading;
using System.IO;
using NLog.Fluent;

namespace Regata.Measurements.Managers
{
    public static partial class AppManager
    {
        public static InfoContext DbContext { get; private set; }
        private static readonly SqlConnectionStringBuilder _mainConnectionStringBuilder;

        public static string UserId { get; private set; }

        public const string MainDbCredTarget = "MeasurementsMainConnectionString";
        public const string LogDbCredTarget = "MeasurementsLogConnectionString";
        public const string MailServiceTarget = "RegataMail";
        public const string DiskJinrTarget = "MeasurementsDiskJinr";

        public static async Task Login(string user = "", string PinOrPass = "")
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
                await IsDbConnectedAsync();

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
            if (DbContext == null || !DbContext.Database.CanConnect())
                throw new ArgumentNullException("Before pin creating you have to login to the app");

            logger.Info($"Pin creation for user {UserId}");
            if (pin.Length != 4 || !int.TryParse(pin, out _))
                throw new ArgumentException("Pin should have olny 4 digits");

            SecretsManager.SetCredential($"Pin_{UserId}", UserId, pin);
            SecretsManager.SetCredential($"Password_{UserId}", UserId, _mainConnectionStringBuilder.Password);
            logger.Info($"Pin has created successfully");
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
            if (DbContext == null || !DbContext.Database.CanConnect())
                throw new ArgumentNullException("Before pin removing you have to login to the app");

            logger.Info($"Pin removing for user {UserId}");
            if (IsPinExist())
                SecretsManager.RemoveCredentials($"Pin_{UserId}");
            logger.Info($"Pin has removed successfully");
        }

        public static ushort ConnectionsTimeOut = 10;

        public static CancellationToken TimeOutCancToken
        {
            get
            {
                var tok = new CancellationTokenSource();
                tok.CancelAfter(TimeSpan.FromSeconds(ConnectionsTimeOut));
                return tok.Token;
            }
        }

        public static async Task<bool> IsCloudStorageConnectedAsync()
        {
            logger.Info("Checks cloud storage connection state...");
            try
            {
                HttpClient client = new HttpClient();
                var checkingResponse = await client.GetAsync(@"https://disk.jinr.ru/", TimeOutCancToken);
                if (checkingResponse.IsSuccessStatusCode)
                {
                    logger.Info("Connection with cloud storage is successful");
                    return true;
                }
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Warning, Sender);
            }
            logger.Info("Can't connect with cloud storage");
            return false;
        }

        public static async Task<bool> IsDbConnectedAsync()
        {
            logger.Info("Checks db connection state...");
            try
            {
                var isConnected = await DbContext.Database.CanConnectAsync(TimeOutCancToken);
                if (isConnected)
                {
                    logger.Info("Connection with DB is successful");
                    return true;
                }
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Warning, Sender);
            }
            logger.Info("Can't connect with DB");
            return false;
        }

        /// <summary>
        /// Upload local file to the database in order to keep data after connection lost. 
        /// In case of success loading local files wil be delete.
        /// </summary>
        private static void UploadLocalDataToDB()
        {
            var fileList = LoadMeasurementsFiles();
            if (fileList.Count == 0) return;

            try
            {
                logger.Info($"AppManager has found {fileList.Count} of files in the local data directory. It will be deserialize: load into db and then delete from local storage");

                DbContext.Measurements.UpdateRange(fileList);
                DbContext.SaveChanges();
                var dir = new DirectoryInfo(@"D:\LocalData");
                var files = dir.GetFiles("*.json");
                foreach (var file in files)
                    file.Delete();
                logger.Info("Deserialization has done");
            }
            catch (Exception e)
            {
                NotificationManager.Notify(e, NotificationLevel.Error, Sender);
            }
        }
    } // public static partial class AppManager
} // namespace Regata.Measurements.Managers
