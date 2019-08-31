using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Measurements.Core.Tests
{

    public class SessionFixture
    {
        public Session session;

        public SessionFixture()
        {
            //TODO: add test for sli and lli (for sli spreading logic should be different!)
            //TODO: something wrong in filling info. looks like current sample on detector doesn't assign!
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";
            session = new Session();

            session.Type = "LLI-1";

            session.CurrentIrradiationDate = DateTime.Parse("24.05.2019");

            // TODO: here I break the order of measurement. Assign count mode and counts number before creation detectors. Add extension for such case!

            session.AttachDetector("D1");
            //session.AttachDetector("D5");
            //session.AttachDetector("D6");

            session.SetAcquireModeAndDuration(CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime, 10);
        }
    }

    public class SessionTest  : IClassFixture<SessionFixture>
    {

        public SessionFixture sessionFixture;

        public SessionTest(SessionFixture sessionFixture)
        {
            this.sessionFixture = sessionFixture;
        }


        [Fact]
        void SessionCreation()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            ISession localSession = new Session();

            Assert.False(localSession.IrradiationDateList.Any());
            Assert.False(localSession.IrradiationList.Any());
            Assert.False(localSession.MeasurementList.Any());

            localSession.Type = "SLI";

            Assert.True(localSession.IrradiationDateList.Any());

            localSession.CurrentIrradiationDate = DateTime.Parse("24.05.2019");

            Assert.True(localSession.IrradiationList.Any());
            Assert.True(localSession.MeasurementList.Any());

            localSession.Dispose();
        }


        [Fact]
        void StartPauseContinueStopSingleMeasurements()
        {

            sessionFixture.session.ClearMeasurements();
            System.Threading.Thread.Sleep(2000);
            sessionFixture.session.StartMeasurements();
            System.Threading.Thread.Sleep(2000);
            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));
            var detTime = new Dictionary<string, double>();
            sessionFixture.session.PauseMeasurements();
            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.Equal(10, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_L_PMREAL)), 2);

                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
                Assert.NotEqual(0, realTime);
                detTime.Add(d.Name, realTime);
            }

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

            sessionFixture.session.ContinueMeasurements();

            System.Threading.Thread.Sleep(2000);

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));

            sessionFixture.session.StopMeasurements();

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
                Assert.NotEqual(detTime[d.Name], realTime);
            }
            
        }

        [Fact]
        void   NextSample()
        {


        }

        [Fact]
        void   PrevSample()
        {

        }

        [Fact]
        void   MakeSampleCurrentOnDetector()
        {

        }

        [Fact]
        void   SaveSpectra()
        {

        }

        [Fact]
        void   SaveSession()
        {

        }

        [Fact]
        void   ContinueMeasurements()
        {

        }

        [Fact]
        void   ClearMeasurements()
        {

        }

        [Fact]
        void   Dispose()
        {

        }
        [Fact]

        void   AttachDetector()
        {

        }

        [Fact]
        void   DetachDetector()
        {

        }

        [Fact]
        void   SpreadSamplesToDetectors()
        {

        }
    }
}
