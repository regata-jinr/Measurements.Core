using Xunit;
using CanberraDataAccessLib;
using System;
using Xunit.Abstractions;
using AutoMapper;

namespace Measurements.Core.Tests
{
   //TODO: add stress-test!
   //TODO: add correct sequence for tests
    public class Detectors
    {
        public Detector d1;

        public MeasurementInfo measurement1;
        public MeasurementInfo measurement2;
        public IrradiationInfo relatedIrradiation1;
        public IrradiationInfo relatedIrradiation2;

        public Detectors()
        {
            d1 = new Detector("D1");

            relatedIrradiation1 = new IrradiationInfo()
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
                DateTimeFinish = DateTime.Now.AddSeconds(3),
                Duration = 3
            };

            var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MeasurementsCore"));
            var mapper = new Mapper(configuration);
            measurement1 = mapper.Map<MeasurementInfo>(relatedIrradiation1);
            measurement1.Duration = 5;
            measurement1.Detector = "D1";
            measurement1.Height = 10;
            measurement1.Type = "SLI";
            measurement1.FileSpectra = "testD1";
            measurement1.Assistant = "bdrum";
            measurement1.Note = "bdrum-test";



            relatedIrradiation2 = new IrradiationInfo()
            {
                CountryCode = "HU",
                ClientNumber = "33",
                Year = "21",
                SetNumber = "02",
                SetIndex = "x",
                SampleNumber = "6",
                Weight = 0.33m,
                Assistant = "brumyant",
                Note = "test22",
                DateTimeStart = DateTime.Now.AddSeconds(33),
                DateTimeFinish = DateTime.Now.AddSeconds(73),
                Duration = 40
            };

            measurement2 = mapper.Map<MeasurementInfo>(relatedIrradiation2);
            measurement2.Duration = 55;
            measurement2.Detector = "D1";
            measurement2.Height = 20;
            measurement2.Type = "LLI-2";
            measurement2.FileSpectra = "testD1raz";
            measurement2.Assistant = "brumyant";
            measurement2.Note = "bdrum-test-2";

        }
    }

    public class DetectorsTest : IClassFixture<Detectors>
    {
        private readonly ITestOutputHelper output;
        public Detectors _detectors;
        DataAccess f1;

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
        public void Initialization()
        {
            Assert.Equal("D1", _detectors.d1.Name);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            Assert.True(_detectors.d1.IsConnected);
            Assert.Equal(CanberraDeviceAccessLib.AcquisitionModes.aCountToRealTime, _detectors.d1.AcquisitionMode);
            Assert.Equal(0, _detectors.d1.Counts);
            Assert.True(string.IsNullOrEmpty(_detectors.d1.ErrorMessage));
            Assert.False(_detectors.d1.IsPaused);
        }

        [Fact]
        public void Connection()
        {
            Assert.True(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            _detectors.d1.Disconnect();
            Assert.Equal(DetectorStatus.off, _detectors.d1.Status);
            Assert.False(_detectors.d1.IsConnected);
            _detectors.d1.Connect();
            Assert.True(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            _detectors.d1.Reconnect();
            Assert.True(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
        }

        [Fact]
        public void LoadInformationToDevice()
        {
            Assert.True(_detectors.d1.IsConnected);
            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);

            _detectors.d1.LoadMeasurementInfoToDevice(_detectors.measurement1, _detectors.relatedIrradiation1);

            Assert.Equal(_detectors.measurement1.SampleKey, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_STITLE));
            Assert.Equal(_detectors.measurement1.Assistant, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SCOLLNAME));
            Assert.Equal(_detectors.measurement1.Note, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SDESC1));
            Assert.Equal(_detectors.measurement1.SetKey, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SIDENT));
            Assert.Equal(_detectors.relatedIrradiation1.Weight.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SQUANT));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SQUANTERR));
            Assert.Equal("gram", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SUNITS));
            Assert.Equal(_detectors.relatedIrradiation1.DateTimeStart.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_SDEPOSIT));
            Assert.Equal(_detectors.relatedIrradiation1.DateTimeFinish.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_STIME));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SSYSERR));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SSYSTERR));
            Assert.Equal(_detectors.measurement1.Type, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_STYPE));
            Assert.Equal(_detectors.measurement1.Height.Value.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SGEOMTRY));
            Assert.Equal("IRRAD", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_BUILDUPTYPE));



            _detectors.d1.LoadMeasurementInfoToDevice(_detectors.measurement2, _detectors.relatedIrradiation2);


            Assert.Equal(_detectors.measurement2.SampleKey, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_STITLE));
            Assert.Equal(_detectors.measurement2.Assistant, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SCOLLNAME));
            Assert.Equal(_detectors.measurement2.Note, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SDESC1));
            Assert.Equal(_detectors.measurement2.SetKey, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SIDENT));
            Assert.Equal(_detectors.relatedIrradiation2.Weight.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SQUANT));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SQUANTERR));
            Assert.Equal("gram", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SUNITS));
            Assert.Equal(_detectors.relatedIrradiation2.DateTimeStart.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_SDEPOSIT));
            Assert.Equal(_detectors.relatedIrradiation2.DateTimeFinish.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_STIME));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SSYSERR));
            Assert.Equal("0", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_F_SSYSTERR));
            Assert.Equal(_detectors.measurement2.Type, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_STYPE));
            Assert.Equal(_detectors.measurement2.Height.Value.ToString(), _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_SGEOMTRY));
            Assert.Equal("IRRAD", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_BUILDUPTYPE));
        }


        [Fact]
        public void Calibration()
        {
            _detectors.d1.LoadMeasurementInfoToDevice(_detectors.measurement2, _detectors.relatedIrradiation2);

            Assert.Equal($"{_detectors.d1.Name}-H{_detectors.measurement2.Height.ToString().Replace('.', ',')}", _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_GEOMETRY));

            Assert.NotEqual(string.Empty, _detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_T_CALVERS));

        }

        [Fact]
        public void StartStopStartStopSave()
        {
           
          


            _detectors.d1.Start();

            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            Assert.NotEqual(0, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)), 2);
            _detectors.d1.Stop();

            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);
            Assert.Equal(2, (int)Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)));
            _detectors.d1.Start();
            System.Threading.Thread.Sleep(2000);
            Assert.Equal(DetectorStatus.busy, _detectors.d1.Status);
            Assert.NotEqual(2, Double.Parse(_detectors.d1.GetParameterValue(CanberraDeviceAccessLib.ParamCodes.CAM_X_EREAL)), 2);
            _detectors.d1.Stop();
            _detectors.d1.Save();

            Assert.Equal(DetectorStatus.ready, _detectors.d1.Status);

            System.Threading.Thread.Sleep(1000);
            Assert.True(System.IO.File.Exists(_detectors.d1.FullFileSpectraName));

            f1.Open(_detectors.d1.FullFileSpectraName);

            Assert.Equal($"{_detectors.d1.CurrentMeasurement.SampleKey}", f1.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
            Assert.Equal(_detectors.d1.CurrentMeasurement.Assistant, f1.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name
            Assert.Equal(_detectors.d1.CurrentMeasurement.Note, f1.Param[ParamCodes.CAM_T_SDESC1].ToString());
            Assert.Equal(_detectors.d1.CurrentMeasurement.SetKey, f1.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
            Assert.Equal(_detectors.d1.RelatedIrradiation.Weight.ToString(), f1.Param[ParamCodes.CAM_F_SQUANT].ToString()); // weight
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
            Assert.Equal("gram", f1.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
            Assert.Equal(_detectors.d1.RelatedIrradiation.DateTimeStart.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
            Assert.Equal(_detectors.d1.RelatedIrradiation.DateTimeFinish.ToString().Replace(" ", ""), f1.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
            Assert.Equal(_detectors.d1.CurrentMeasurement.Type, f1.Param[ParamCodes.CAM_T_STYPE].ToString());
            Assert.Equal(_detectors.d1.CurrentMeasurement.Height.Value, decimal.Parse(f1.Param[ParamCodes.CAM_T_SGEOMTRY].ToString()),2);

            f1.Close();

            System.IO.File.Delete(_detectors.d1.FullFileSpectraName);
            Assert.False(System.IO.File.Exists(_detectors.d1.FullFileSpectraName));
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

