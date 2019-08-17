using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Measurements.Core
{

    //todo: change id in measurements and irradiations table to login
    class SessionController : ISessionController
    {

        public SessionController(string connectionString)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            _assistant = csb.UserID;
            _managedSessions = new List<ISession>();


        }

        private bool _isDBConnected;
        private List<ISession> _managedSessions;
        private readonly string _assistant;
        public string Assistant { get { return _assistant; } }


        private void SaveLocally()
        { }
        private void SaveRemotely()
        { }

        public ISession Create(string sName)
        {
            ISession session = new Session(Assistant);
            _managedSessions.Add(session);
            return session;
        }
        public ISession Load(string sName)
        {
            return null;
        }
        public List<IDetector> AvailableDetectors { get; private set; } // required checks inside of set
        public List<IDetector> AvailableIrradiationJournals { get; private set; } // required checks inside of set



    }
}
