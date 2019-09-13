/***************************************************************************
 *                                                                         *
 *                                                                         *
 * Copyright(c) 2017-2019, REGATA Experiment at FLNP|JINR                  *
 * Author: [Boris Rumyantsev](mailto:bdrum@jinr.ru)                        *
 * All rights reserved                                                     *
 *                                                                         *
 *                                                                         *
 ***************************************************************************/

using System.Collections.Generic;
using System.Data.SqlClient;
using CanberraDeviceAccessLib;
using System;
using System.Threading.Tasks;
using System.Linq;

// TODO:  add pin code instead of password. use windows credentials accounts
//        in case of connection falling but pin is correct local mode should be available
//        deny running of appliaction in case it already running

// FIXME: adding costura for merging dlls, but pay attention that it will break tests.
//        find out how to exclude test. exclude assemblies with xunit didn't help

/// <summary>
/// More information about the design of this core, general schemas and content see in the readme file of this repository:
/// https://github.com/regata-jinr/MeasurementsCore
/// </summary>
namespace Measurements.Core
{
    /// <summary>
    /// SessionControllerSingleton is the class that implemented singleton pattern and is used for control <seealso cref="Session"/> of measurements 
    /// </summary>
    public static class SessionControllerSingleton
    {
        /// <summary>
        /// For the logging we use NLog framework. Now we keep logs in the directory of application. 
        /// Logger creates 'MeasurementsLogs' folder nad file with name {short-date}.log.
        /// Also we consider implementation of web-monitor based on keeping logs in the data base.
        /// </summary>
        /// <see>
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// The list of created session managed by SessionController
        /// </summary>
        public static List<ISession>              ManagedSessions         { get; private set; }
        /// <summary>
        /// The list of MCA devices available for usage. This list forms via MCA databases provided by Canberra DeviceAccessClass
        /// </summary>
        public static List<IDetector>             AvailableDetectors      { get; private set; }

        /// <summary>
        /// We consider the opportunity to have two modes of measurement (local and remote). Now remote mode by default, but in case of errors
        /// local mode will turn on and keep measurement process till connection will restore. All data save to the local storage with the usage of serialization process.
        /// </summary>
        private static bool LocalMode;

        private static SqlConnectionStringBuilder _connectionStringBuilder; 
        /// <summary>
        /// Connection string builder allows to get all information via connection string: DBName, UserID, so on.
        /// UserID is used for getting assistant name
        /// </summary>
        public static SqlConnectionStringBuilder ConnectionStringBuilder
        {
            get { return _connectionStringBuilder; }
        }
        /// <summary>
        /// Here is the starting point of the application. Before the calling any method of this class, you have to call this one
        /// It will initialize all field of this class and allow you to go further
        /// </summary>
        /// <param name="connectionString"></param>
        public static void InitializeDBConnectionString(string connectionString)
        {
            _connectionStringBuilder.ConnectionString = connectionString;
            TestDBConnection();
            logger.WithProperty("ParamName", ConnectionStringBuilder.UserID);
        }

        /// <summary>
        /// This additional method for the checking the connection state. In case of returning false local mode will turn on
        /// </summary>
        /// <returns>True in case of connection with specified db is success</returns>
        public static bool TestDBConnection()
        {
            CheckSessionControllerInitialisation();

            bool isConnected = false;
            var sqlCon = new SqlConnection(ConnectionStringBuilder.ConnectionString);
            try
            {
                logger.Info("Test connection with database:");
                sqlCon.Open();

                isConnected = true;

                logger.Info("Connection successful");
            }

            catch (SqlException sqle)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, sqle, NLog.LogLevel.Warn);
                sqlCon.Dispose();
                if (!LocalMode)
                    Task.Run(() => ConnectionWaiter());
            }
            finally
            {
                sqlCon.Dispose();
            }

            return isConnected;
        }

        /// <summary>
        /// This event will occur when connection will be restore after falling
        /// </summary>
        public static event Action ConectionRestoreEvent;

        /// <summary>
        /// This internal action for the checking connection. Every 10 seconds it runs TestDBConnection
        /// in case of true it invoke ConnectionRestoreEvent
        /// </summary>
        private static void ConnectionWaiter()
        {
            logger.Info("Try to connect to db asynchronously");
            LocalMode = true;
            System.Threading.Thread.Sleep(10000);
            if (TestDBConnection())
            {
                logger.Info("Connection has restored!");
                LocalMode = false;
                ConectionRestoreEvent?.Invoke();
                return;
            }
            else ConnectionWaiter();
        }

        /// <summary>
        /// This internal method deny to use this class till InitializeDBConnectionString() has called
        /// </summary>
        private static void CheckSessionControllerInitialisation()
        {
            if (string.IsNullOrEmpty(_connectionStringBuilder.ConnectionString))
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new ArgumentNullException("First of all call InitializeDBConnection method!"), NLog.LogLevel.Error);
                throw new ArgumentNullException("First of all call InitializeDBConnection method!");
            }
        }
        /// <summary>
        /// This internal method add detector to the list of available detectors for usage
        /// with all checkings
        /// </summary>
        /// <param name="name">Name of the detector. You can see detectors name in MCA Input Defenition Editor</param>
        private static void AddDetectorToList(string name)
        {
            IDetector d = null;
            try
            {
                    d = new Detector(name);
                    if (d.IsConnected)
                        AvailableDetectors.Add(d);
 
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, NLog.LogLevel.Error);
                d?.Dispose(); 
            }
        }

        /// <summary>
        /// Add available detectors to list using internal MCA database
        /// </summary>
        private static void DetectorsInititalisation()
        {
            try
            {
                logger.Info("Initialization of detectors:");

                var _device = new DeviceAccessClass();
                var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

                foreach (var n in detNames)
                    AddDetectorToList(n.ToString());

            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, NLog.LogLevel.Error);
            }
        }

        /// <summary>
        /// Static constructor for initialization of all fields of the class
        /// </summary>
        static SessionControllerSingleton()
        {
            logger.Info("Inititalization of Session Controller instance has began");

            _isDisposed              = false;
            LocalMode                = false;
            _connectionStringBuilder = new SqlConnectionStringBuilder(); 
            AvailableDetectors       = new List<IDetector>();
            ManagedSessions          = new List<ISession>();

            DetectorsInititalisation();

            logger.Info("Creation of session controller instance has done");
        }


        /// <summary>
        /// Allows user to create session from the scratch
        /// </summary>
        /// <returns></returns>
        public static ISession Create()
        {
            CheckSessionControllerInitialisation();

            logger.Info("Creating of the new session instance");
            ISession session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        /// <summary>
        /// Allow users to load saved session from db
        /// </summary>
        /// <param name="sName">Name of saved session</param>
        /// <returns>Session object with filled information such as: Name of session, List of detectors, type of measurement, spreaded option, counts, countmode, height, assistant, note</returns>
        public static ISession Load(string sName)
        {
            CheckSessionControllerInitialisation();

            logger.Info($"Loading session with name '{sName}' from DB");
            try
            {
                var sessionContext = new InfoContext();

                var sessionInfo = sessionContext.Sessions.Where(s => s.Name == sName && (s.Assistant == null || s.Assistant == ConnectionStringBuilder.UserID)).FirstOrDefault();

                if (sessionInfo == null)
                    throw new ArgumentNullException("Such session doesn't exist. Check the name or create the new one");

                ISession session = new Session(sessionInfo);
                ManagedSessions.Add(session);

                return session;
            }
            catch (ArgumentException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ae, NLog.LogLevel.Warn );
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, NLog.LogLevel.Error);
            }
            return null;
        }

        /// <summary>
        /// Allows users to close session, remove it from the list of session managed by the SessionControllerSingleton and free resources correctly 
        /// </summary>
        /// <param name="session"></param>
        public static void CloseSession(ISession session)
        {
            try
            {
                if (session == null)
                {
                    logger.Info($"Session is null. Please, provide instance of Session type");
                    throw new ArgumentNullException("You try to remove null object instead of instanced session");
                }

                if (!ManagedSessions.Contains(session))
                {
                    logger.Info($"List of managed session doesn't contain such session - {session.Name}");
                    throw new InvalidOperationException($"Such session {session.Name} doesn't exist in the list of managed session by SessionControllerSingleton");
                }
                ManagedSessions.Remove(session);
                session.Dispose();
            }
            catch (ArgumentNullException ae)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ae, NLog.LogLevel.Warn);
            }
            catch (InvalidOperationException ie)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, ie, NLog.LogLevel.Error);
            }
            catch (Exception e)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, e, NLog.LogLevel.Error);
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
                    ManagedSessions.Clear();
                    foreach (var d in AvailableDetectors)
                        d.Dispose();
                    AvailableDetectors.Clear();
                }
            }
            _isDisposed = true;
        }

    } // class SessionControllerSingleton
} // namespace
