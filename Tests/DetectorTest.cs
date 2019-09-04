using Xunit;
using CanberraDataAccessLib;
using System;
using Xunit.Abstractions;

namespace Measurements.Core.Tests
{
   //TODO: add stress-test!
   //TODO: add correct sequence for tests
    public class Detectors
    {
        public Detector d1;
        //public Detector d5;
        //public Detector d6;
        //public Detector d7;

        public Detectors()
        {
            d1 = new Detector("D1");
            //d5 = new Detector("D5");
            //d6 = new Detector("D6");
            //d7 = new Detector("D7");
        }
    }

    public class DetectorsTest : IClassFixture<Detectors>
    {
        private readonly ITestOutputHelper output;
        public Detectors _detectors;
        DataAccess f1;
        string testDir = @"C:\GENIE2K\CAMFILES";

        public DetectorsTest(Detectors dets, ITestOutputHelper output)
        {
            _detectors = dets;
            f1 = new DataAccess();
            System.Threading.Thread.Sleep(1000);
            this.output = output;
        }

        [Fact]
        public void Logs()
        {
            Assert.True(System.IO.File.Exists($"{System.IO.Directory.GetCurrentDirectory()}\\MeasurementsLogs\\{DateTime.Now.ToString("yyyy-MM-dd")}.log"));
        }

        [Fact]
        public void Names()
        {
            Assert.Equal("D1", _detectors.d1.Name);
        }

        [Fact]
        public void Statuses()
        {
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
        }

        [Fact]
        public void Connections()
        {
            Assert.True(_detectors.d1.IsConnected);
        }

        [Fact]
        public void StartPauseContinueClear()
        {
            _detectors.d1.SetAcqureCountsAndMode(5, CanberraDeviceAccessLib.AcquisitionModes.aCountToLiveTime);
            Assert.Equal(5, _detectors.d1.CountToLiveTime);
            _detectors.d1.SetAcqureCountsAndMode(5);
            Assert.Equal(5, _detectors.d1.CountToRealTime);

            _detectors.d1.Start();
            Assert.False(_detectors.d1.IsPaused);
            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            _detectors.d1.Pause();
            Assert.True(_detectors.d1.IsPaused);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            Assert.NotEqual(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
            double prev = Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL));
            _detectors.d1.Start();
            Assert.False(_detectors.d1.IsPaused);
            System.Threading.Thread.Sleep(2000);
            Assert.NotEqual(prev, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
            _detectors.d1.Pause();
            _detectors.d1.Clear();
            System.Threading.Thread.Sleep(1000);
            Assert.Equal(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
        }

       [Fact]
        public void StartStopContinue()
        {
            Assert.False(_detectors.d1.IsPaused);
            _detectors.d1.SetAcqureCountsAndMode(3);
            _detectors.d1.Start();
            Assert.False(_detectors.d1.IsPaused);
            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            Assert.NotEqual(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
            Assert.False(_detectors.d1.IsPaused);
            _detectors.d1.Stop();
            Assert.False(_detectors.d1.IsPaused);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            Assert.Equal(0,Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
            _detectors.d1.Start();
            Assert.False(_detectors.d1.IsPaused);
            System.Threading.Thread.Sleep(1000);
            Assert.NotEqual(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)),2);
        }

        [Fact]
        public void StartStopSave()
        {
            System.IO.File.Delete($"{testDir}\\test{_detectors.d1.Name}.cnf");
            Assert.False(System.IO.File.Exists($"{testDir}\\test{_detectors.d1.Name}.cnf"));

            var sd = new IrradiationInfo()
            {
                CountryCode = "RO",
                ClientNumber = "2",
                Year = "19",
                SetNumber = "12",
                SetIndex = "b",
                SampleNumber = "2",
                Weight = 0.2m,
                Assistant = "bdrum",
                Note = "test2",
                DateTimeStart = DateTime.Now,
                DateTimeFinish = DateTime.Now.AddSeconds(3)
            };

            _detectors.d1.CurrentSample = sd;
            _detectors.d1.CurrentMeasurement.Height = 10;
            _detectors.d1.CurrentMeasurement.Type = "SLI";
            _detectors.d1.CurrentMeasurement.FileSpectra = "testD1.cnf";
            _detectors.d1.SetAcqureCountsAndMode(3);

            _detectors.d1.Start();
            _detectors.d1.CurrentMeasurement.Assistant = "bdrum";

            System.Threading.Thread.Sleep(2000);
            
            _detectors.d1.Stop();

            _detectors.d1.Save();

            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);

            System.Threading.Thread.Sleep(1000);
            Assert.True(System.IO.File.Exists($"{testDir}\\test{_detectors.d1.Name}.cnf"));

            f1.Open($"{testDir}\\test{_detectors.d1.Name}.cnf");

            Assert.Equal($"{_detectors.d1.CurrentSample.SampleKey}", f1.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
            Assert.Equal(_detectors.d1.CurrentSample.Assistant, f1.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name
            Assert.Equal(_detectors.d1.CurrentSample.Note, f1.Param[ParamCodes.CAM_T_SDESC1].ToString());
            Assert.Equal(_detectors.d1.CurrentSample.SetKey, f1.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
            Assert.Equal(_detectors.d1.CurrentSample.Weight.ToString(), f1.Param[ParamCodes.CAM_F_SQUANT].ToString()); // weight
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
            Assert.Equal("gram", f1.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
            Assert.Equal(_detectors.d1.CurrentSample.DateTimeStart.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
            Assert.Equal(_detectors.d1.CurrentSample.DateTimeFinish.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
            Assert.Equal(_detectors.d1.CurrentMeasurement.Type, f1.Param[ParamCodes.CAM_T_STYPE].ToString());
            Assert.Equal(_detectors.d1.CurrentMeasurement.Height.ToString(), f1.Param[ParamCodes.CAM_T_SGEOMTRY].ToString());

            f1.Close();
        }

        [Fact]
        public void Disconnections()
        {
            System.Threading.Thread.Sleep(2000);

            Assert.True(_detectors.d1.IsConnected);
            _detectors.d1.Disconnect();

            System.Threading.Thread.Sleep(2000);

            Assert.False(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.off, _detectors.d1.Status);

            _detectors.d1.Connect();

        }
    }

}

