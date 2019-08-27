using System.Collections.Generic;
using System.Data.SqlClient;
using CanberraDeviceAccessLib;
using System;
using System.Linq;

namespace Measurements.Core
{
    //TODO:  add docs
    //TODO:  add tests
    //TODO:  improve readability of logs
    //TODO:  deny running of appliaction in case it already running
    //FIXME: adding costura for merging dlls, but pay attention that it will break tests.
    //       find out how to exclude test. exclude assemblies with xunit didn't help
    static class SessionControllerSingleton
    {

        public static NLog.Logger                logger = NLog.LogManager.GetCurrentClassLogger();

        //TODO: <incapsulate this>
        private static SqlConnectionStringBuilder _connectionStringBuilder; 
        public static SqlConnectionStringBuilder ConnectionStringBuilder
        {
            get { return _connectionStringBuilder; }
        }

        public static void InitializeDBConnection(string connectionString)
        {
            _connectionStringBuilder.ConnectionString = connectionString;
            TestDBConnection();
        }
        public static List<Session>              ManagedSessions         { get; private set; }
        public static List<Detector>             AvailableDetectors      { get; private set; }
        //TODO: </incapsulate this>

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
            Detector d = null;
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
            AvailableDetectors = new List<Detector>();
            ManagedSessions    = new List<Session>();

            DetectorsInititalisation();

            logger.Info("Creation of session controller instance has done");
        }


        public static ISession Create()
        {
            CheckSessionControllerInitialisation();

            logger.Info("Creating of the new session instance");
            var session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        public static ISession Load(string sName)
        {
            
            CheckSessionControllerInitialisation();

            logger.Info("Loading session parameters from DB");
            try
            {
                var sessionContext = new SessionInfoContext();

                var sessionInfo = sessionContext.Sessions.Where(s => s.Name == sName && (s.Assistant == null || s.Assistant == ConnectionStringBuilder.UserID)).FirstOrDefault();

                if (sessionInfo == null)
                    throw new ArgumentNullException("Such session doesn't exist. Check the name or create the new one");

                var session = new Session(sessionInfo);
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
