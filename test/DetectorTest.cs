using Xunit;


namespace MeasurementsCore.test
{
    //FIXME: In case of splitting StateTest fact to few methods xunit will call constructor for each method as a result duplicate log messages. How to avoid it?
    //FIXME: some problem with D6. It works directly from CanberraDeviceAccess, but doesn't from my wrapper?!
    public class DetectorTest
    {
        IDetector _det1, _det2, _det5, _det6, _det7;
        public DetectorTest()
        {
            _det1 = new Detector("D1");
            _det2 = new Detector("D2");
            _det5 = new Detector("D5");
            _det6 = new Detector("D6");
            _det7 = new Detector("D7");

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



            //Start acquiring
            _det1.CountToRealTime = 5;
            _det1.AStart();

            //TODO: check events
            //var rec = Record.ExceptionAsync(() => Assert.Raises<DetectorEventsArgs>(h => _det1.AStart +=h));


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

            



        }

        //[Fact]
        //void CountToRealTimeTest()
        //{

        //}

        //[Fact]
        //void CountToLiveTimeTest()
        //{

        //}

        //[Fact]
        //void CountNormalTest()
        //{

        //}



        //[Fact]
        //void DetStatusTest()
        //{

        //}

        //[Fact]
        //void ReconnectTest()
        //{

        //}

        //[Fact]
        //void SaveTest()
        //{

        //}

        //[Fact]
        //void DisconnectTest()
        //{

        //}

        //[Fact]
        //void ResetTest()
        //{

        //}

        //[Fact]
        //void AOptionsTest()
        //{
        //    //CanberraDeviceAccessLib.AcquisitionModes opt, int param
        //}

        //[Fact]
        //void AStartTest()
        //{

        //}

        //[Fact]
        //void AContinueTest()
        //{

        //}

        //[Fact]
        //void AStopTest()
        //{

        //}

        //[Fact]
        //void AClearTest()
        //{

        //}

        //[Fact]
        //void FillInfoTest()
        //{
        //    //ref Sample sample, string mType, string operatorName, float height

        //}
    }
}