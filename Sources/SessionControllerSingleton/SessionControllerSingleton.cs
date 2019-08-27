using System.Collections.Generic;
using System.Data.SqlClient;
using CanberraDeviceAccessLib;
using System;

namespace Measurements.Core
{
    //TODO:  add docs
    //TODO:  add tests
    //TODO:  move logger from detector to here
    //TODO:  deny running of appliaction in case it already running
    //FIXME: adding costura for merging dlls, but pay attention that it will break tests.
    //       find out how to exclude test. exclude assemblies with xunit didn't help
    static class SessionControllerSingleton
    {

        //TODO: <incapsulate this>
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static SqlConnectionStringBuilder ConnectionStringBuilder { get; set; }
        public static List<Session> ManagedSessions { get; set; }
        public static List<Detector> AvailableDetectors { get; set; }
        //TODO: </incapsulate this>

        static SessionControllerSingleton()
        {
            logger.Info("Inititalisation of Session Controller instance has began");
            ConnectionStringBuilder = new SqlConnectionStringBuilder(/*put known part of con. string here*/);
            AvailableDetectors = new List<Detector>();
            try
            {
                logger.Info("Test connection with database:");
                var sqlCon = new SqlConnection(ConnectionStringBuilder.ConnectionString);
                sqlCon.Open();
                sqlCon.Close();
                logger.Info("Connection successful");

                ManagedSessions = new List<Session>();

                var _device = new DeviceAccessClass();
                var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

                logger.Info("Initialisation of detectors:");
                foreach (var n in detNames)
                {
                    var d = new Detector(n.ToString());
                    if (d.IsConnected)
                        AvailableDetectors.Add(d);
                }
                logger.Info("Creation of session controller instance has done");
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = $"){ex.Message}", Level = NLog.LogLevel.Error });
            }
        }


        public static ISession Create(string sName)
        {
            logger.Info("Creating of the new session intance");
            var session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        //todo: add table to db and implement this:
        public static ISession Load(string sName)
        {
            
            logger.Info("Loading session parameters from DB");
            try
            {
                throw new Exception();
            }
            catch
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = $"Функция пока не доступна", Level = NLog.LogLevel.Warn });
            }
            return null;
        }
       
    }
}

