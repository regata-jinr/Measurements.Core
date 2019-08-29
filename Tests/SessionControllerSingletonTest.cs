using System;
using System.Collections.Generic;
using System.Linq;
using CanberraDeviceAccessLib;
using Xunit;

namespace Measurements.Core.Tests
{
    public class SessionControllerSingletonTest
    {
        [Fact]
        public void WithoutInitialization()
        {
            Assert.Throws<ArgumentNullException>(SessionControllerSingleton.Create);
            Assert.Throws<ArgumentNullException>(() => { return SessionControllerSingleton.TestDBConnection(); });
            Assert.Throws<ArgumentNullException>(() => SessionControllerSingleton.Load("bdrum-test"));
        }

        [Fact]
        public void AvailableDetectors()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            var _device = new DeviceAccessClass();
            var detNames = (IEnumerable<object>)_device.ListSpectroscopyDevices;

            Assert.True(SessionControllerSingleton.AvailableDetectors.Select(n => n.Name).SequenceEqual(detNames.Select(o => o.ToString())));

            SessionControllerSingleton.Dispose();
        }

        [Fact]
        public void SessionCreate()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");

            var iSession = SessionControllerSingleton.Create();
            iSession.AttachDetector("D1");

            Assert.Single<Session>(SessionControllerSingleton.ManagedSessions);

            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1").Any());
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5").Any());

            SessionControllerSingleton.Dispose();
        }

        [Fact]
        public void SessionLoad()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

            var iSession = SessionControllerSingleton.Load("bdrum-test");

            Assert.Single<Session>(SessionControllerSingleton.ManagedSessions);

            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1" || d.Name == "D5").Any());
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D6").Any());

            Assert.Equal(AcquisitionModes.aCountToRealTime, iSession.CountMode);
            Assert.Equal(3, iSession.Counts);
            Assert.Equal("bdrum-test", iSession.Name);
            Assert.Equal("LLI-1", iSession.Type);
            Assert.Equal("for tests", iSession.Note);
            Assert.Equal(2.5, (double)iSession.Height, 2);

            SessionControllerSingleton.Dispose();
        }

        [Fact]
        public void SessionClose()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

            var iSession = SessionControllerSingleton.Load("bdrum-test");

            Assert.Single<Session>(SessionControllerSingleton.ManagedSessions);
            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1" || d.Name == "D5").Any());
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D6").Any());

            iSession.Dispose();

            Assert.Empty(SessionControllerSingleton.ManagedSessions);
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1" || d.Name == "D5").Any());
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D6").Any());

            SessionControllerSingleton.Dispose();
        }
 
    }
}
