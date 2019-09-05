using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
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

            session.CurrentIrradiationDate = DateTime.Parse("26.05.2019");

            // TODO: here I break the order of measurement. Assign count mode and counts number before creation detectors. Add extension for such case!

            session.AttachDetector("D1");
            session.AttachDetector("D5");

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
        void StartPauseContinuePauseClearSingleMeasurements()
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

            sessionFixture.session.ClearMeasurements();
            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                Assert.Equal(d.CountToRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
                Assert.Equal(0, realTime);
            }
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
            IDetector det = null;

            sessionFixture.session.ClearMeasurements();
            sessionFixture.session.StartMeasurements();
            System.Threading.Thread.Sleep(2000);
            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));

            var ic = new InfoContext();
            sessionFixture.session.PauseMeasurements();

            foreach (var d in sessionFixture.session.ManagedDetectors)
            {
                det = d;
                sessionFixture.session.SaveSpectra(ref det);

                Assert.True(System.IO.File.Exists($"C:\\GENIE2K\\CAMFILES\\{det.CurrentMeasurement.FileSpectra}.cnf"));
                Assert.False(ic.Measurements.Where(m => m.FileSpectra == det.CurrentMeasurement.FileSpectra).Any()); 
            }

        }

        [Fact]
        void   SaveSession()
        {
            sessionFixture.session.SaveSession("duringTesting", true);
            
            var ic = new InfoContext();

            Assert.Single<SessionInfo>(ic.Sessions.Where(si => si.Name == "duringTesting").ToArray());

            var si1 = ic.Sessions.Where(si11 => si11.Name == "duringTesting").First();
  
            Assert.Equal(sessionFixture.session.Name, si1.Name);
            Assert.Equal(string.Join(",", sessionFixture.session.ManagedDetectors.Select(d => d.Name).ToArray()), si1.DetectorsNames);
            Assert.Equal(sessionFixture.session.CountMode.ToString(), si1.CountMode);
            Assert.Equal(sessionFixture.session.Counts, si1.Duration);
            Assert.Equal(sessionFixture.session.SpreadOption.ToString(), si1.SpreadOption);
            Assert.Equal(sessionFixture.session.Height, si1.Height);
            Assert.Equal(sessionFixture.session.Note, si1.Note);
            Assert.Null(si1.Assistant);

            ic.Sessions.Remove(si1);
            ic.SaveChanges();

            Assert.False(ic.Sessions.Any(ss => ss.Name == "duringTesting"));
        }

        [Fact]
        void   Dispose()
        {
            sessionFixture.session.Dispose();

            Assert.Empty(sessionFixture.session.ManagedDetectors);
            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5" || d.Name == "D1").Any());

            Assert.Empty(SessionControllerSingleton.ManagedSessions);
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
        void   SpreadSamplesToDetectorsContainerOption()
        {
            var ic = new InfoContext();
            if (sessionFixture.session.Type == "SLI") return;

            //TODO: should I sort it?
            var irD1 = ic.Irradiations.Where(ir1 => ir1.DateTimeStart.HasValue && ir1.DateTimeStart.Value.Date.ToShortDateString() == "26.05.2019" && ir1.Type == "LLI-1" &&  (ir1.Container.Value == 1 || ir1.Container.Value == 4 || ir1.Container.Value == 6)).OrderBy(ir1 => $"{ir1.SetKey}-{ir1.SampleNumber}").ToList();
            var irD5 = ic.Irradiations.Where(ir5 => ir5.DateTimeStart.HasValue && ir5.DateTimeStart.Value.Date.ToShortDateString() == "26.05.2019" && ir5.Type == "LLI-1" &&  (ir5.Container.Value == 3 || ir5.Container.Value == 5)).OrderBy(ir5 => $"{ir5.SetKey}-{ir5.SampleNumber}").ToList();

            sessionFixture.session.SpreadSamplesToDetectors();

            Assert.True(irD1.SequenceEqual(sessionFixture.session.SpreadedSamples["D1"].OrderBy(ird1 => $"{ird1.SetKey}-{ird1.SampleNumber}").ToList()));
            Assert.True(irD5.SequenceEqual(sessionFixture.session.SpreadedSamples["D5"].OrderBy(ird5 => $"{ird5.SetKey}-{ird5.SampleNumber}").ToList()));
        }

        [Fact]
        void SpreadSamplesToDetectorsUniformOption()
        {

        }

        [Fact]
        void SpreadSamplesToDetectorsInOrderOption()
        {

        }
    }
}
