using Xunit;
using System;


namespace MeasurementsCore.test
{
    //todo: In such logic xuint create object DetectorTest for each method
    public class DetectorTest
    {
        IDetector _det1;
        IDetector _det2;
        IDetector _det5;
        IDetector _det6;
        public DetectorTest()
        {
            _det1 = new Detector("D1");
            _det2 = new Detector("D2");
            _det5 = new Detector("D5");
            _det6 = new Detector("D6");
        }

      //  [Fact]
      //public void NameTest()
      //{
      //    Assert.Equal("D1", _det1.Name);
      //    Assert.NotEqual("D2", _det2.Name);
      //    Assert.Equal("D5", _det5.Name);
      //    Assert.Equal("D6", _det6.Name);
      //}

        [Fact]
      public void ConnectTest()
      {
          Assert.True(_det1.IsConnected);
          Assert.False(_det2.IsConnected);
          Assert.True(_det5.IsConnected);
          Assert.True(_det6.IsConnected);
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
