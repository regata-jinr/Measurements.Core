using System.Collections.Generic;
using System.Data.SqlClient;
using CanberraDeviceAccessLib;
using System;
using System.Linq;

namespace Measurements.Core
{
    // TODO:  add docs
    // TODO:  add functional tests in order to check working sequence, 
    //        such test should simulate program working in general manner and more (for clearify logs) 
    // TODO:  improve readability of logs
    // TODO:  deny running of appliaction in case it already running
    // FIXME: adding costura for merging dlls, but pay attention that it will break tests.
    //        find out how to exclude test. exclude assemblies with xunit didn't help

    public static class SessionControllerSingleton
    {

        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static List<ISession>              ManagedSessions         { get; private set; }
        public static List<IDetector>             AvailableDetectors      { get; private set; }

        private static SqlConnectionStringBuilder _connectionStringBuilder; 
        public static SqlConnectionStringBuilder ConnectionStringBuilder
        {
            get { return _connectionStringBuilder; }
        }
        public static void InitializeDBConnectionString(string connectionString)
        {
            _connectionStringBuilder.ConnectionString = connectionString;
            TestDBConnection();
        }

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

                sqlCon.Dispose();
                logger.Info("Connection successful");
            }

            catch (SqlException sqlex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = $"{sqlex.Message}{Environment.NewLine}Program will work in the local mode", Level = NLog.LogLevel.Warn });
                sqlCon.Dispose();
            }

            return isConnected;
        }

        private static void CheckSessionControllerInitialisation()
        {
            if (string.IsNullOrEmpty(_connectionStringBuilder.ConnectionString))
                throw new ArgumentNullException("First of all call InitializeDBConnection method!");
        }

        private static void AddDetectorToList(string name)
        {
            IDetector d = null;
            try
            {
                    d = new Detector(name);
                    if (d.IsConnected)
                        AvailableDetectors.Add(d);
 
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
                d?.Dispose(); 
            }
        }

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
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }
        }

        static SessionControllerSingleton()
        {
            logger.Info("Inititalization of Session Controller instance has began");

            _isDisposed = false;
            _connectionStringBuilder = new SqlConnectionStringBuilder(); 
            AvailableDetectors = new List<IDetector>();
            ManagedSessions    = new List<ISession>();

            DetectorsInititalisation();

            logger.Info("Creation of session controller instance has done");
        }


        public static ISession Create()
        {
            CheckSessionControllerInitialisation();

            logger.Info("Creating of the new session instance");
            ISession session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        public static ISession Load(string sName)
        {
            CheckSessionControllerInitialisation();

            logger.Info("Loading session parameters from DB");
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
            catch (ArgumentException arg)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = arg.Message, Level = NLog.LogLevel.Warn });
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = ex.Message, Level = NLog.LogLevel.Error });
            }
            return null;
        }

        public static void CloseSession(ISession session)
        {
            session.Dispose();
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
