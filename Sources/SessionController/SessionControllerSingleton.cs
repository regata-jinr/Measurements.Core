using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;
using CanberraDeviceAccessLib;
using System;

namespace Measurements.Core
{
    // TODO: change id in measurements and irradiations table to login (int ==> varchar(15?))
    // TODO: deny to create few SessionController class that means deny to run two apps
    static class SessionControllerSingleton
    {

        //TODO: <how to incapsulate this>
        public static SqlConnectionStringBuilder ConnectionStringBuilder { get; set; }
        public static List<Session> ManagedSessions { get; set; }
        public static List<Detector> AvailableDetectors { get; set; }
        //TODO: </how to incapsulate this>

        static SessionControllerSingleton()
        {
            ConnectionStringBuilder = new SqlConnectionStringBuilder(/*put known part of con. string here*/);
            AvailableDetectors = new List<Detector>();
            try
            {
                var sqlCon = new SqlConnection(ConnectionStringBuilder.ConnectionString);
                sqlCon.Open();
                sqlCon.Close();

                ManagedSessions = new List<Session>();

                var _device = new DeviceAccessClass();
                var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

                foreach (var n in detNames)
                {
                    var d = new Detector(n.ToString());
                    if (d.IsConnected)
                        AvailableDetectors.Add(d);
                }
            }
            catch (Exception ex)
            {
                Handlers.ExceptionHandler.ExceptionNotify(null, new Handlers.ExceptionEventsArgs { Message = $"){ex.Message}", Level = NLog.LogLevel.Error });
            }
        }


        public static ISession Create(string sName)
        {
            var session = new Session();
            ManagedSessions.Add(session);
            return session;
        }

        //todo: add table to db and implement this:
        public static ISession Load(string sName)
        {
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
