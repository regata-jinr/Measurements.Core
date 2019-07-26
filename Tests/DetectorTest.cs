using Xunit;
using CanberraDataAccessLib;

namespace Measurements.Core.Tests
{
    //TODO: add stress-test!
    public class D1 : Detector
    {
        public D1() : base("D1") { }
    }

    public class D1Test : IClassFixture<D1>
    {
        D1 d1;
        DataAccess f1;
        string testDir = $"{System.IO.Directory.GetCurrentDirectory()}";

        public D1Test(D1 d1)
        {
            this.d1 = d1;
            f1 = new DataAccess();
            System.Threading.Thread.Sleep(1000);
        }

        [Fact]
        public void Names()
        {
            Assert.Equal("D1", d1.Name);
        }

        [Fact]
        public void Statuses()
        {
            Assert.Equal(DetectorStatus.ready, d1.DetStatus);
        }

        [Fact]
        public void Connections()
        {
            Assert.True(d1.IsConnected);
        }

        [Fact]
        public void Acquiring()
        {
            d1.CountToRealTime = 3;
            d1.Start();
            System.Threading.Thread.Sleep(1000);
            Assert.Equal(DetectorStatus.busy, d1.DetStatus);
            System.Threading.Thread.Sleep(4000);
        }

        [Fact]
        public void FillingInformation()
        {
            var sd = new Sample("RO", 2, 19, 12, 'b', 2, "b-2", 0.2, "bdrum", System.DateTime.Now, System.DateTime.Now.AddDays(1), "test2");

            // let's wait while acquire will finish
            System.Threading.Thread.Sleep(4000);

            d1.FillInfo(ref sd, "LLITest", "bdrum", 5.0);
            d1.Save($"{testDir}\\test_{d1.Name}.cnf");

            Assert.Equal(DetectorStatus.ready, d1.DetStatus);

            System.Threading.Thread.Sleep(1000);

            f1.Open($"{testDir}\\test_{d1.Name}.cnf");

            Assert.Equal($"{sd.SampleSetIndex}-{sd.SampleNumber}", f1.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
            Assert.Equal(sd.IrradiationOperator, f1.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name
            Assert.Equal(sd.Description, f1.Param[ParamCodes.CAM_T_SDESC1].ToString());
            Assert.Equal(sd.SetKey, f1.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
            Assert.Equal(sd.Weight.ToString(), f1.Param[ParamCodes.CAM_F_SQUANT].ToString()); // weight
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
            Assert.Equal("gram", f1.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
            Assert.Equal(sd.IrradiationStartDateTime.ToString(), f1.Param[ParamCodes.CAM_X_SDEPOSIT].ToString()); // irr start date time
            Assert.Equal(sd.IrradiationFinishDateTime.ToString(), f1.Param[ParamCodes.CAM_X_STIME].ToString()); // irr finish date time
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
            Assert.Equal("0", f1.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
            Assert.Equal("LLITest", f1.Param[ParamCodes.CAM_T_STYPE].ToString());
            Assert.Equal("5", f1.Param[ParamCodes.CAM_T_SGEOMTRY].ToString());
            Assert.Equal("3", f1.Param[ParamCodes.CAM_X_PREAL].ToString());

            f1.Close();
        }

        [Fact]
        public void Disconnections()
        {
            System.Threading.Thread.Sleep(2000);

            d1.Disconnect();

            Assert.False(d1.IsConnected);
            Assert.Equal(DetectorStatus.off, d1.DetStatus);

            d1.Connect();

        }
    }
}

