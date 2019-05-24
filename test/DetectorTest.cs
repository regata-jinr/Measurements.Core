using Xunit;
using CanberraDataAccessLib;

namespace MeasurementsCore.Tests
{
    //FIXME: In case of splitting StateTest to few methods xunit will call constructor for each method as a result duplicate log messages. How to avoid it?
    //FIXME: some problem with D6. It works directly from CanberraDeviceAccess, but doesn't from my wrapper?!
    public class DetectorTest
    {
        Detector _det1, _det2, _det5, _det6, _det7;
        string testDir = @"D:\GoogleDrive\Job\flnp\dev\Measurements\MeasurementsCore\test";
        DataAccess f1, f5, f6, f7;

        public DetectorTest()
        {
            _det1 = new Detector("D1");
            _det2 = new Detector("D2");
            _det5 = new Detector("D5");
            _det6 = new Detector("D6");
            _det7 = new Detector("D7");

            f1 = new DataAccess();
            f5 = new DataAccess();
            f6 = new DataAccess();
            f7 = new DataAccess();

        }

        [Fact]
        public void StateTest()
        {
            //names
            Assert.Equal("D1", _det1.Name);
            Assert.NotEqual("D2", _det2.Name);
            Assert.Equal("D5", _det5.Name);
            Assert.Equal("D6", _det6.Name);
            Assert.Equal("D7", _det7.Name);

            //statuses
            Assert.Equal(_det1.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det2.DetStatus, DetectorStatus.error);
            Assert.Equal(_det5.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det6.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det7.DetStatus, DetectorStatus.ready);

            //Connections
            Assert.True(_det1.IsConnected);
            Assert.False(_det2.IsConnected);
            Assert.True(_det5.IsConnected);
            Assert.True(_det6.IsConnected);
            Assert.True(_det7.IsConnected);

            _det1.AClear();
            _det5.AClear();
            _det6.AClear();
            _det7.AClear();

            //Start acquiring
            _det1.CountToRealTime = 3;
            _det1.AStart();
            _det5.CountToRealTime = 3;
            _det5.AStart();
            _det6.CountToRealTime = 3;
            _det6.AStart();
            _det7.CountToRealTime = 3;
            _det7.AStart();

            var sd1 = new Sample("RU", 1, 19, 11, 'a', 1, "a-1", 0.1, "bdrum1", System.DateTime.Now, System.DateTime.Now.AddDays(1), "test1");
            var sd5 = new Sample("RO", 2, 19, 12, 'b', 2, "b-2", 0.2, "bdrum2", System.DateTime.Now, System.DateTime.Now.AddDays(1), "test2");
            var sd6 = new Sample("RA", 3, 19, 13, 'c', 3, "c-3", 0.3, "bdrum3", System.DateTime.Now, System.DateTime.Now.AddDays(1), "test3");
            var sd7 = new Sample("RI", 4, 19, 14, 'd', 4, "d-4", 0.4, "bdrum4", System.DateTime.Now, System.DateTime.Now.AddDays(1), "test4");

            Assert.Equal(_det1.DetStatus, DetectorStatus.busy);
            Assert.Equal(_det5.DetStatus, DetectorStatus.busy);
            Assert.Equal(_det6.DetStatus, DetectorStatus.busy);
            Assert.Equal(_det7.DetStatus, DetectorStatus.busy);


            // let's wait while acquire will finish
            System.Threading.Thread.Sleep(4000);

            _det1.FillInfo(ref sd1, "LLITest1", "bdrum1", 2.5);
            _det5.FillInfo(ref sd5, "LLITest2", "bdrum2", 5.0);
            _det6.FillInfo(ref sd6, "LLITest3", "bdrum3", 10.0);
            _det7.FillInfo(ref sd7, "LLITest4", "bdrum4", 20.0);

            _det1.Save($"{testDir}\\testD1.cnf");
            _det5.Save($"{testDir}\\testD5.cnf");
            _det6.Save($"{testDir}\\testD6.cnf");
            _det7.Save($"{testDir}\\testD7.cnf");

            Assert.Equal(_det1.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det5.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det6.DetStatus, DetectorStatus.ready);
            Assert.Equal(_det7.DetStatus, DetectorStatus.ready);


            //Disconnections
            _det1.Disconnect();
            _det5.Disconnect();
            _det6.Disconnect();
            _det7.Disconnect();
            Assert.False(_det1.IsConnected);
            Assert.False(_det5.IsConnected);
            Assert.False(_det6.IsConnected);
            Assert.False(_det7.IsConnected);

            Assert.Equal(_det1.DetStatus, DetectorStatus.off);
            Assert.Equal(_det5.DetStatus, DetectorStatus.off);
            Assert.Equal(_det6.DetStatus, DetectorStatus.off);
            Assert.Equal(_det7.DetStatus, DetectorStatus.off);



            f1.Open($"{testDir}\\testD1.cnf");
            f5.Open($"{testDir}\\testD5.cnf");
            f6.Open($"{testDir}\\testD6.cnf");
            f7.Open($"{testDir}\\testD7.cnf");


            Assert.Equal(f1.Param[ParamCodes.CAM_T_STITLE].ToString(), $"{sd1.SampleSetIndex}-{sd1.SampleNumber}");// title
            Assert.Equal(f1.Param[ParamCodes.CAM_T_SCOLLNAME].ToString(), sd1.IrradiationOperator); // operator's name
            Assert.Equal(f1.Param[ParamCodes.CAM_T_SDESC1].ToString(), sd1.Description);
            Assert.Equal(f1.Param[ParamCodes.CAM_T_SIDENT].ToString(), sd1.SetKey); // sd1 code
            Assert.Equal(f1.Param[ParamCodes.CAM_F_SQUANT].ToString(), sd1.Weight.ToString()); // weight
            Assert.Equal(f1.Param[ParamCodes.CAM_F_SQUANTERR].ToString(), "0"); // err, 0
            Assert.Equal(f1.Param[ParamCodes.CAM_T_SUNITS].ToString(), "gram"); // units, gram
            Assert.Equal(f1.Param[ParamCodes.CAM_X_SDEPOSIT].ToString(), sd1.IrradiationStartDateTime.ToString()); // irr start date time
            Assert.Equal(f1.Param[ParamCodes.CAM_X_STIME].ToString(), sd1.IrradiationFinishDateTime.ToString()); // irr finish date time
            Assert.Equal(f1.Param[ParamCodes.CAM_F_SSYSERR].ToString(), "0"); // Random sd1 error (%)
            Assert.Equal(f1.Param[ParamCodes.CAM_F_SSYSTERR].ToString(), "0"); // Non-random sd1 error (%)
            Assert.Equal(f1.Param[ParamCodes.CAM_T_STYPE].ToString(), "LLITest1");
            Assert.Equal(f1.Param[ParamCodes.CAM_T_SGEOMTRY].ToString(), "2.5");
            Assert.Equal(f1.Param[ParamCodes.CAM_X_PREAL].ToString(), "3");




            Assert.Equal(f5.Param[ParamCodes.CAM_T_STITLE].ToString(), $"{sd5.SampleSetIndex}-{sd5.SampleNumber}");// title
            Assert.Equal(f5.Param[ParamCodes.CAM_T_SCOLLNAME].ToString(), sd5.IrradiationOperator); // operator's name
            Assert.Equal(f5.Param[ParamCodes.CAM_T_SDESC1].ToString(), sd5.Description);
            Assert.Equal(f5.Param[ParamCodes.CAM_T_SIDENT].ToString(), sd5.SetKey); // sd5 code
            Assert.Equal(f5.Param[ParamCodes.CAM_F_SQUANT].ToString(), sd5.Weight.ToString()); // weight
            Assert.Equal(f5.Param[ParamCodes.CAM_F_SQUANTERR].ToString(), "0"); // err, 0
            Assert.Equal(f5.Param[ParamCodes.CAM_T_SUNITS].ToString(), "gram"); // units, gram
            Assert.Equal(f5.Param[ParamCodes.CAM_X_SDEPOSIT].ToString(), sd5.IrradiationStartDateTime.ToString()); // irr start date time
            Assert.Equal(f5.Param[ParamCodes.CAM_X_STIME].ToString(), sd5.IrradiationFinishDateTime.ToString()); // irr finish date time
            Assert.Equal(f5.Param[ParamCodes.CAM_F_SSYSERR].ToString(), "0"); // Random sd5 error (%)
            Assert.Equal(f5.Param[ParamCodes.CAM_F_SSYSTERR].ToString(), "0"); // Non-random sd5 error (%)
            Assert.Equal(f5.Param[ParamCodes.CAM_T_STYPE].ToString(), "LLITest2");
            Assert.Equal(f5.Param[ParamCodes.CAM_T_SGEOMTRY].ToString(), "5");
            Assert.Equal(f5.Param[ParamCodes.CAM_X_PREAL].ToString(), "3");


            Assert.Equal(f6.Param[ParamCodes.CAM_T_STITLE].ToString(), $"{sd6.SampleSetIndex}-{sd6.SampleNumber}");// title
            Assert.Equal(f6.Param[ParamCodes.CAM_T_SCOLLNAME].ToString(), sd6.IrradiationOperator); // operator's name
            Assert.Equal(f6.Param[ParamCodes.CAM_T_SDESC1].ToString(), sd6.Description);
            Assert.Equal(f6.Param[ParamCodes.CAM_T_SIDENT].ToString(), sd6.SetKey); // sd6 code
            Assert.Equal(f6.Param[ParamCodes.CAM_F_SQUANT].ToString(), sd6.Weight.ToString()); // weight
            Assert.Equal(f6.Param[ParamCodes.CAM_F_SQUANTERR].ToString(), "0"); // err, 0
            Assert.Equal(f6.Param[ParamCodes.CAM_T_SUNITS].ToString(), "gram"); // units, gram
            Assert.Equal(f6.Param[ParamCodes.CAM_X_SDEPOSIT].ToString(), sd6.IrradiationStartDateTime.ToString()); // irr start date time
            Assert.Equal(f6.Param[ParamCodes.CAM_X_STIME].ToString(), sd6.IrradiationFinishDateTime.ToString()); // irr finish date time
            Assert.Equal(f6.Param[ParamCodes.CAM_F_SSYSERR].ToString(), "0"); // Random sd6 error (%)
            Assert.Equal(f6.Param[ParamCodes.CAM_F_SSYSTERR].ToString(), "0"); // Non-random sd6 error (%)
            Assert.Equal(f6.Param[ParamCodes.CAM_T_STYPE].ToString(), "LLITest3");
            Assert.Equal(f6.Param[ParamCodes.CAM_T_SGEOMTRY].ToString(), "10");
            Assert.Equal(f6.Param[ParamCodes.CAM_X_PREAL].ToString(), "3");


            Assert.Equal(f7.Param[ParamCodes.CAM_T_STITLE].ToString(), $"{sd7.SampleSetIndex}-{sd7.SampleNumber}");// title
            Assert.Equal(f7.Param[ParamCodes.CAM_T_SCOLLNAME].ToString(), sd7.IrradiationOperator); // operator's name
            Assert.Equal(f7.Param[ParamCodes.CAM_T_SDESC1].ToString(), sd7.Description);
            Assert.Equal(f7.Param[ParamCodes.CAM_T_SIDENT].ToString(), sd7.SetKey); // sd7 code
            Assert.Equal(f7.Param[ParamCodes.CAM_F_SQUANT].ToString(), sd7.Weight.ToString()); // weight
            Assert.Equal(f7.Param[ParamCodes.CAM_F_SQUANTERR].ToString(), "0"); // err, 0
            Assert.Equal(f7.Param[ParamCodes.CAM_T_SUNITS].ToString(), "gram"); // units, gram
            Assert.Equal(f7.Param[ParamCodes.CAM_X_SDEPOSIT].ToString(), sd7.IrradiationStartDateTime.ToString()); // irr start date time
            Assert.Equal(f7.Param[ParamCodes.CAM_X_STIME].ToString(), sd7.IrradiationFinishDateTime.ToString()); // irr finish date time
            Assert.Equal(f7.Param[ParamCodes.CAM_F_SSYSERR].ToString(), "0"); // Random sd7 error (%)
            Assert.Equal(f7.Param[ParamCodes.CAM_F_SSYSTERR].ToString(), "0"); // Non-random sd7 error (%)
            Assert.Equal(f7.Param[ParamCodes.CAM_T_STYPE].ToString(), "LLITest4");
            Assert.Equal(f7.Param[ParamCodes.CAM_T_SGEOMTRY].ToString(), "20");
            Assert.Equal(f7.Param[ParamCodes.CAM_X_PREAL].ToString(), "3");


            f1.Close();
            f5.Close();
            f6.Close();
            f7.Close();


        }
    }
}