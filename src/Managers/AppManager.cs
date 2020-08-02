/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2020, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using CanberraDeviceAccessLib;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.SqlClient;
using Measurements.Core;
using Polly;

/// <summary>
/// 
/// More information about the design of this core, general schemas and content see in the readme file of this repository:
/// https://github.com/regata-jinr/MeasurementsCore
/// </summary>
namespace Regata.Measurements.Managers
{
    public enum DbConnectionStatus {Off, On };
    /// <summary>
    /// AppManager is a singleton class that contains the enter point to application, configurator that provides connection string for the data base and cloud storage, defines logging rules and provides control to general behaviour of application. 
    /// </summary>
    public static class AppManager
    {
        public static bool LocalMode { get; private set; }

        public  static InfoContext DbContext { get; private set; }
        private static readonly SqlConnectionStringBuilder _mainConnectionStringBuilder;

        public static string UserId { get; private set; }


        private const string MainDbCredTarget  = "MeasurementsMainConnectionString";
        private const string LogDbCredTarget   = "MeasurementsLogConnectionString";
        private const string MailServiceTarget = "RegataMail";

        public async static Task Login(string user = "", string PinOrPass = "")
        {
            UserId = user;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(PinOrPass))
            {
                LocalMode = true;

                return;
            }

            _mainConnectionStringBuilder.UserID = user;

            if (IsPinExist() && PinOrPass == SecretsManager.GetCredential($"Pin_{user}")?.Secret)
            {
                _mainConnectionStringBuilder.Password = SecretsManager.GetCredential($"Password_{user}").Secret;
                await CheckConnectionState();
                if (DbConnectionState == DbConnectionStatus.Off)
                    LocalMode = true;
                return;
            }
            else
                _mainConnectionStringBuilder.Password = PinOrPass;

            DbContext = new InfoContext(_mainConnectionStringBuilder.ConnectionString);

        }

        public static bool CheckLocalStorage()
        {
            
        }

        public static bool IsAssmAlreadyRunning(string AssmName)
        {
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
        /// For the logging we use NLog framework. Now we keep logs in the directory of application. 
        /// Logger creates 'MeasurementsLogs' folder nad file with name {short-date}.log.
        /// Also we consider implementation of web-monitor based on keeping logs in the data base.
        /// </summary>
        /// <see>
        public static NLog.Logger logger;
        /// <summary>
        /// The list of created session managed by SessionController
        /// </summary>
        public static ObservableCollection<ISession> ActiveSessions { get; private set; }

        /// <summary>
        /// The list of MCA devices and Xemoes that available for usage. This list forms via MCA databases provided by Canberra DeviceAccessClass. Each xemo device linked with concrete detector.
        /// </summary>
        public static ObservableCollection<IDetector> AvailableDevices { get; private set; }


        /// <summary>
        /// This event will occur when connection will be restore after falling
        /// </summary>
        public static event Action DBConectionStateChanged;

        public static DbConnectionStatus DbConnectionState { get; private set; }

        private async static Task CheckConnectionState()
        {
            var prevState = DbConnectionState;
            var IsConnect = await DbContext.Database.CanConnectAsync();

            if (IsConnect)
                DbConnectionState = DbConnectionStatus.On;
            else
                DbConnectionState = DbConnectionStatus.Off;

            if (prevState != DbConnectionState)
                DBConectionStateChanged?.Invoke();
        }

        /// <summary>
        /// This internal method add detector to the list of available detectors for usage
        /// with all checkings
        /// </summary>
        /// <param name="name">Name of the detector. You can see detectors name in MCA Input Defenition Editor</param>
        private static void AddDevicesToList(string name)
        {
            IDetector d = null;
            try
            {
                d = new Detector(name);
                if (d.IsConnected)
                {
                    AvailableDevices.Add(d);
                    d.Disconnect();
                    // check xemo connection here also
                }
 
            }
            catch (Exception e)
            {
                NotificationManager.Notify<AppManager, Exception>(null, e, NotificationLevel.Warning);
                d?.Dispose(); 
            }
        }

        /// <summary>
        /// Add available detectors to list using internal MCA database
        /// </summary>
        public static void GetDevicesList()
        {
            try
            {
                logger.Info("Initialization of detectors:");

                var _device = new DeviceAccessClass();
                var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

                foreach (var n in detNames)
                    AddDevicesToList(n.ToString());

            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
            }
        }

        /// <summary>
        /// Static constructor for initialization of all fields of the class
        /// </summary>
        static AppManager()
        {
            NLog.GlobalDiagnosticsContext.Set("LogConnectionString", SecretsManager.GetCredential(LogDbCredTarget).Secret);
            logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Inititalization of Session Controller instance has begun");

            _isDisposed              = false;
            LocalMode                = false;
            _mainConnectionStringBuilder = new SqlConnectionStringBuilder();
            _mainConnectionStringBuilder.ConnectionString = SecretsManager.GetCredential(MainDbCredTarget).Secret;
            AvailableDevices       = new ObservableCollection<IDetector>();
            ActiveSessions = new ObservableCollection<ISession>();

            GetDevicesList();

            Policy
                .Handle<SqlException>(ex => ex.InnerException.Message.Contains("network"))
                .RetryForeverAsync();

            Policy.
                Handle<SqlException>(ex => ex.Message.Contains("Login failed"))
                .RetryAsync(3);

            logger.Info("App inititalisation has done!");
        }

        /// <summary>
        /// Allows user to create session from the scratch
        /// </summary>
        /// <returns></returns>
        public static ISession CreateNewSession()
        {
            logger.Info("Creating of the new session instance");
            ISession session = new Session();
            ActiveSessions.Add(session);
            return session;
        }

        /// <summary>
        /// Allow users to load saved session from db
        /// </summary>
        /// <param name="sName">Name of saved session</param>
        /// <returns>Session object with filled information such as: Name of session, List of detectors, type of measurement, spreaded option, counts, countmode, height, assistant, note</returns>
        public static void ShowActiveSession(string sName)
        {

            logger.Info($"Loading session with name '{sName}' from DB");
            try
            {
                if (string.IsNullOrEmpty(sName))
                    throw new ArgumentNullException("Such session doesn't exist. Check the name or create the new one");

                ActiveSessions.Where(s => s.Name == sName).First().Show();
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ae, Handlers.ExceptionLevel.Warn );
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, Handlers.ExceptionLevel.Error);
            }
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
                    foreach (var d in AvailableDevices)
                        d.Dispose();
                    AvailableDevices.Clear();
                    DbContext.Dispose();
                }
            }
            _isDisposed = true;
        }

    } // class SessionControllerSingleton
} // namespace
