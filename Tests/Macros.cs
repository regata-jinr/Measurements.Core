using Xunit;
using System;
using System.Linq;

namespace Measurements.Core.Tests
{
    public class Macros
    {
        //TODO: extend test for random data, read and compare files content
        [Fact]
        public void FastMeasurements()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

            var iSession = SessionControllerSingleton.Load("bdrum-test");
            iSession.Type = "LLI-2";
            iSession.CurrentIrradiationDate = DateTime.Parse("18.06.2012");
            foreach(var m in iSession.MeasurementList)
                m.Note = "TEST!";
            iSession.SetAcquireDurationAndMode(5);
            iSession.ClearMeasurements();
            iSession.StartMeasurements();

            System.Threading.Thread.Sleep(iSession.Counts*iSession.IrradiationList.Count*1000 + iSession.IrradiationList.Count*1000);

            var ic = new InfoContext();

            Assert.Equal(iSession.IrradiationList.Count, ic.Measurements.Where(m => m.DateTimeStart.Value.Date == DateTime.Now.Date && m.Type == iSession.Type).Count());
        }
    }
}
