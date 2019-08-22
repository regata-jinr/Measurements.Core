using System.Linq;
using System;
using Xunit;

namespace Measurements.Core.Tests
{
    public class IrradiationInfoTest
    {
        [Fact]
        public void GetValues()
        {
            var ic = new IrradiationInfoContext();
            var i = ic.Irradiations.Where(qi => qi.Id == 17640).FirstOrDefault();
            Assert.Equal(17640, i.Id);
            Assert.Equal("GE", i.CountryCode);
            Assert.Equal("02", i.ClientNumber);
            Assert.Equal("14", i.Year);
            Assert.Equal("32", i.SetNumber);
            Assert.Equal("f", i.SetIndex);
            Assert.Equal("13", i.SampleNumber);
            Assert.Equal("LLI-1", i.Type);
            Assert.Equal(0.2903, (Double)i.Weight, 4);
            Assert.Equal(DateTime.Parse("2014 - 11 - 27 09:19:00.000"), i.DateTimeStart);
            Assert.Equal(345420, i.Duration);
            Assert.Equal(DateTime.Parse("2014 - 12 - 01 09:16:00.000"), i.DateTimeFinish);
            Assert.Equal(4, (int)i.Container);
            Assert.Equal(3, (int)i.Position);
            Assert.Equal(1, (int)i.Channel);
            Assert.Equal(68, i.LoadNumber);
            Assert.Equal("gundorina", i.Rehandler);
            Assert.Equal("vergel", i.Assistant);
            Assert.Null(i.Note);
        }
    }
  public class MeasurementInfoTest
    {
        [Fact]
        public void GetValues()
        {
            var mc = new MeasurementInfoContext();
            var m = mc.Measurements.Where(qm => qm.IrradiationId == 17640).FirstOrDefault();
            Assert.Equal(17640, m.Id);
            Assert.Equal(17640, m.IrradiationId);
            Assert.Equal("GE", m.CountryCode);
            Assert.Equal("02", m.ClientNumber);
            Assert.Equal("14", m.Year);
            Assert.Equal("32", m.SetNumber);
            Assert.Equal("f", m.SetIndex);
            Assert.Equal("13", m.SampleNumber);
            Assert.Equal("LLI-1", m.Type);
            Assert.Equal(DateTime.Parse("2014-12-05 00:00:00.000"), m.DateTimeStart);
            Assert.Null(m.Duration);
            //Assert.Null(m.DateTimeFinish);
            Assert.Null(m.Height);
            Assert.Equal("1102512", m.FileSpectra);
            Assert.Equal("D1", m.Detector);
            Assert.Null(m.Assistant);
            Assert.Null(m.Note);
        }
   }
}
