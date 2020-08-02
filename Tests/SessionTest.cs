//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Linq;
//using Xunit;

//namespace Measurements.Core.Tests
//{

//    public class SessionFixture
//    {
//        public ISession session;

//        public List<MeasurementInfo> measurementInfos;
//        public List<IrradiationInfo> irradiationInfos;

//        public SessionFixture()
//        {
//            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
//            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";
//            session = new Session();
//            session.AttachDetector("D1");
//            session.AttachDetector("D5");
//            session.Type = "LLI-1";

//            using (var ic = new InfoContext())
//            {

//                measurementInfos = ic.Measurements.Where(m => m.Type == session.Type && m.DateTimeStart.Value.Date == DateTime.Parse("19.10.2019")).ToList();
//                irradiationInfos = ic.Irradiations.Where(ir => measurementInfos.Select(m => m.IrradiationId).Contains(ir.Id)).ToList();
//            }
//            int n = 0;
//            foreach (var d in session.ManagedDetectors)
//            {
//                measurementInfos[n].Detector = d.Name;
//                measurementInfos[n].Duration = 10;
//                measurementInfos[n].Note = "TEST!";
//                d.FillSampleInformation(measurementInfos[n], irradiationInfos[n]);
//                n++;
//            }
//        }
//    }

//    public class SessionTest : IClassFixture<SessionFixture>
//    {

//        public SessionFixture sessionFixture;

//        public SessionTest(SessionFixture sessionFixture)
//        {
//            this.sessionFixture = sessionFixture;
//        }


//        [Fact]
//        void StartPauseContinuePauseClearSingleMeasurements()
//        {

//            sessionFixture.session.ClearMeasurements();
//            System.Threading.Thread.Sleep(1000);
//            sessionFixture.session.StartMeasurements();
//            System.Threading.Thread.Sleep(2000);
//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));
//            var detTime = new Dictionary<string, double>();
//            sessionFixture.session.PauseMeasurements();
//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                Assert.Equal(d.PresetRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

//                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
//                Assert.NotEqual(0, realTime);
//                detTime.Add(d.Name, realTime);
//            }

//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

//            sessionFixture.session.ContinueMeasurements();

//            System.Threading.Thread.Sleep(2000);

//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));

//            System.Threading.Thread.Sleep(4000);

//            sessionFixture.session.PauseMeasurements();

//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
//                Assert.NotEqual(detTime[d.Name], realTime);
//            }

//            sessionFixture.session.ClearMeasurements();
//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                Assert.Equal(d.PresetRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

//                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
//                Assert.Equal(0, realTime);
//            }
//        }

      
//        [Fact]
//        void SaveSpectra()
//        {
//            IDetector det = null;

//            sessionFixture.session.ClearMeasurements();
//            sessionFixture.session.StartMeasurements();
//            System.Threading.Thread.Sleep(2000);
//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));

//            var ic = new InfoContext();
//            sessionFixture.session.PauseMeasurements();

//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                det = d;
//                sessionFixture.session.SaveSpectraOnDetectorToFile(ref det);

//                Assert.True(System.IO.File.Exists(det.FullFileSpectraName));
//                Assert.False(ic.Measurements.Where(m => m.FileSpectra == det.CurrentMeasurement.FileSpectra).Any());
//            }

//        }

//        [Fact]
//        void SaveSession()
//        {
//            sessionFixture.session.SaveSession("duringTesting", true);

//            var ic = new InfoContext();

//            Assert.Single<SessionInfo>(ic.Sessions.Where(si => si.Name == "duringTesting").ToArray());

//            var si1 = ic.Sessions.Where(si11 => si11.Name == "duringTesting").First();

//            Assert.Equal(sessionFixture.session.Name, si1.Name);
//            Assert.Equal(string.Join(",", sessionFixture.session.ManagedDetectors.Select(d => d.Name).ToArray()), si1.DetectorsNames);
//            Assert.Equal(sessionFixture.session.CountMode.ToString(), si1.CountMode);
//            Assert.Equal(sessionFixture.session.Note, si1.Note);
//            Assert.Null(si1.Assistant);

//            ic.Sessions.Remove(si1);
//            ic.SaveChanges();

//            Assert.False(ic.Sessions.Any(ss => ss.Name == "duringTesting"));
//        }

//        [Fact]
//        void Dispose()
//        {
//            sessionFixture.session.Dispose();

//            Assert.Empty(sessionFixture.session.ManagedDetectors);
//            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5" || d.Name == "D1").Any());

//            Assert.Empty(SessionControllerSingleton.ManagedSessions);
//        }

//        [Fact]

//        void AttachDetector()
//        {

//            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D1" || d.Name == "D5").Any());
//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Name == "D1" || d.Name == "D5"));

//            sessionFixture.session.AttachDetector("D6");

//            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D6").Any());
//            Assert.True(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D6").Any());

//        }

//        [Fact]
//        void DetachDetector()
//        {
//            Assert.False(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5").Any());
//            Assert.True(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D5").Any());

//            sessionFixture.session.DetachDetector("D5");

//            Assert.True(SessionControllerSingleton.AvailableDetectors.Where(d => d.Name == "D5").Any());
//            Assert.False(sessionFixture.session.ManagedDetectors.Where(d => d.Name == "D5").Any());
//        }

//        [Fact]
//        void SaveMeasurementRemotely()
//        {
//            sessionFixture.session.ClearMeasurements();
//            System.Threading.Thread.Sleep(2000);
//            sessionFixture.session.StartMeasurements();
//            System.Threading.Thread.Sleep(4000);
//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));
//            sessionFixture.session.PauseMeasurements();
//            IDetector det = null;

//            var ic = new InfoContext();

//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                det = d;
//                Assert.Equal(d.PresetRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

//                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
//                Assert.NotEqual(0, realTime);
//                sessionFixture.session.SaveSpectraOnDetectorToFile(ref det);
//                Assert.False(ic.Measurements.Where(m => m.FileSpectra == d.CurrentMeasurement.FileSpectra).Any());
//                sessionFixture.session.SaveMeasurement(ref det);
//                Assert.True(ic.Measurements.Where(m => m.FileSpectra == d.CurrentMeasurement.FileSpectra).Any());
//                Assert.True(ic.Measurements.Where(m => m.FileSpectra == d.CurrentMeasurement.FileSpectra && m.Note == "TEST!").Any());
//            }

//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.ready));

//        }
//        [Fact]
//        void SaveMeasurementLocallyAndThenUploadAutomatically()
//        {
//            sessionFixture.session.ClearMeasurements();
//            System.Threading.Thread.Sleep(2000);
//            sessionFixture.session.StartMeasurements();
//            System.Threading.Thread.Sleep(4000);
//            Assert.True(sessionFixture.session.ManagedDetectors.All(d => d.Status == DetectorStatus.busy));
//            sessionFixture.session.PauseMeasurements();
//            IDetector det = null;

//            var ic = new InfoContext();

//            Assert.True(Directory.Exists(@"D:\LocalData"));
//            Assert.False(Directory.GetFiles(@"D:\LocalData", "*.json").Any());

//            SessionControllerSingleton.ConnectionStringBuilder.DataSource = "1";
//            foreach (var d in sessionFixture.session.ManagedDetectors)
//            {
//                det = d;
//                Assert.Equal(d.PresetRealTime, double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_PREAL)), 2);

//                double realTime = double.Parse(d.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
//                Assert.NotEqual(0, realTime);
//                sessionFixture.session.SaveSpectraOnDetectorToFile(ref det);
//                d.CurrentMeasurement.Note = "TEST!";
//                sessionFixture.session.SaveMeasurement(ref det);
//            }

//            Assert.True(Directory.GetFiles(@"D:\LocalData", "*.json").Any());
//            SessionControllerSingleton.ConnectionStringBuilder.DataSource = "RUMLAB\\REGATALOCAL";
//            System.Threading.Thread.Sleep(13000);


//            foreach (var d in sessionFixture.session.ManagedDetectors)
//                Assert.True(ic.Measurements.Where(m => m.FileSpectra == d.CurrentMeasurement.FileSpectra && m.Note == "TEST!").Any());


//            Assert.False(Directory.GetFiles(@"D:\LocalData", "*.json").Any());
//        }
//    }
//}
