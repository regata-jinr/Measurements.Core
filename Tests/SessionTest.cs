using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Measurements.Core.Tests
{

    public class SessionFixture
    {
        public ISession session;

        public SessionFixture()
        {
            //TODO: add test for sli and lli 
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";
            session = new Session();

            session.Type = "LLI-1";

            session.CurrentIrradiationDate = DateTime.Parse("24.05.2019");

            // TODO: here I break the order of measurement. Assign count mode and counts number before creation detectors. Add extension for such case!

            session.AttachDetector("D1");
            session.AttachDetector("D5");
            //session.AttachDetector("D6");

            session.SetAcquireDurationAndMode(20);
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
        void StartPauseContinuePauseSingleMeasurements()
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
                Assert.Equal(d.CountToRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
                Assert.NotEqual(0, realTime);
                detTime.Add(d.Name, realTime);
            }

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

            sessionFixture.session.ContinueMeasurements();

            System.Threading.Thread.Sleep(2000);

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));

            System.Threading.Thread.Sleep(4000);

            sessionFixture.session.PauseMeasurements();

            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
                Assert.NotEqual(detTime[d.Name], realTime);
            }
            
        }

        [Fact]
        void SpreadSamples()
        {
            
        }

        [Fact]
        void NextSample()
        {
            Assert.True(sessionFixture.session.ManagedDetectors.Any());

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.Null(d.CurrentSample.Assistant);
                Assert.Null(d.CurrentMeasurement.Assistant);
                Assert.False(sessionFixture.session.SpreadedSamples[d.Name].Any());
            }

            Assert.Equal(SpreadOptions.container, sessionFixture.session.SpreadOption);

            sessionFixture.session.SpreadSamplesToDetectors();            

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(0, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }

            IDetector det = null;
            foreach (IDetector d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                sessionFixture.session.NextSample(ref det);
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(1, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }

            foreach (IDetector d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                sessionFixture.session.NextSample(ref det);
                sessionFixture.session.NextSample(ref det);
                sessionFixture.session.NextSample(ref det);
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(4, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }
        }

        [Fact]
        void   PrevSample()
        {
            Assert.True(sessionFixture.session.ManagedDetectors.Any());

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.Null(d.CurrentSample.Assistant);
                Assert.Null(d.CurrentMeasurement.Assistant);
                Assert.False(sessionFixture.session.SpreadedSamples[d.Name].Any());
            }

            Assert.Equal(SpreadOptions.container, sessionFixture.session.SpreadOption);

            sessionFixture.session.SpreadSamplesToDetectors();            

            sessionFixture.session.MakeSamplesCurrentOnAllDetectorsByNumber(1);            

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(1, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }

            IDetector det = null;
            foreach (IDetector d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                sessionFixture.session.PrevSample(ref det);
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(0, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }

            foreach (IDetector d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                sessionFixture.session.PrevSample(ref det);
                Assert.Equal(0, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }


        }

        [Fact]
        void   MakeSampleCurrentOnDetector()
        {
            Assert.True(sessionFixture.session.ManagedDetectors.Any());

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.Null(d.CurrentSample.Assistant);
                Assert.Null(d.CurrentMeasurement.Assistant);
                Assert.False(sessionFixture.session.SpreadedSamples[d.Name].Any());
            }

            Assert.Equal(SpreadOptions.container, sessionFixture.session.SpreadOption);

            sessionFixture.session.SpreadSamplesToDetectors();

            IDetector det = null;

            foreach (IDetector d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                var ii = sessionFixture.session.SpreadedSamples[d.Name][10];
                sessionFixture.session.MakeSampleCurrentOnDetector(ref ii, ref det);
                Assert.NotNull(d.CurrentSample.Assistant);
                Assert.True(sessionFixture.session.SpreadedSamples[d.Name].Any());
                Assert.Equal(10, sessionFixture.session.SpreadedSamples[d.Name].IndexOf(d.CurrentSample));
            }
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

            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1" || d.Name == "D5").Any());
            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Name == "D1" || d.Name == "D5"));

            sessionFixture.session.AttachDetector("D6");

            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D6").Any());
            Assert.True(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D6").Any());

        }

        [Fact]
        void   DetachDetector()
        {
            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5").Any());
            Assert.True(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D5").Any());

            sessionFixture.session.DetachDetector("D5");

            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5").Any());
            Assert.False(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D5").Any());
        }

        [Fact]
        void   SpreadSamplesToDetectors()
        {

        }
    }
}
