using Xunit;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using CanberraDataAccessLib;

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

            var iSession = SessionControllerSingleton.Create();
            iSession.AttachDetector("D1");
            iSession.AttachDetector("D5");
            iSession.AttachDetector("D7");
            iSession.Type = "LLI-2";
            iSession.CurrentIrradiationDate = DateTime.Parse("18.06.2012");
            iSession.Height = 2.5m;
            foreach (var m in iSession.MeasurementList)
                m.Note = "TEST!";
            iSession.SetAcquireDurationAndMode(3);
            iSession.ClearMeasurements();
            iSession.StartMeasurements();

            System.Threading.Thread.Sleep(iSession.Counts*iSession.IrradiationList.Count*1000 + iSession.IrradiationList.Count*1000);

            var ic = new InfoContext();

            Assert.Equal(iSession.IrradiationList.Count, ic.Measurements.Where(m => m.DateTimeStart.Value.Date == DateTime.Now.Date && m.Type == iSession.Type).Count());
        }

        [Fact]
        void FewSessionInParallel()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

            var iSession = SessionControllerSingleton.Create();
            iSession.AttachDetector("D6");
            iSession.AttachDetector("D7");
            iSession.Type = "LLI-2";
            iSession.CurrentIrradiationDate = DateTime.Parse("18.06.2012");
            iSession.Height = 2.5m;
            foreach (var m in iSession.MeasurementList)
                m.Note = "TEST!Session1";
            iSession.SetAcquireDurationAndMode(5);
            iSession.ClearMeasurements();
            iSession.StartMeasurements();


            var iSession1 = SessionControllerSingleton.Load("bdrum-test");
            iSession1.Type = "SLI";
            iSession1.CurrentIrradiationDate = DateTime.Parse("19.12.2014");
            iSession1.Height = 5m;
            foreach (var m in iSession1.MeasurementList)
                m.Note = "TEST!Session2";
            iSession1.SetAcquireDurationAndMode(3);
            iSession1.ClearMeasurements();
            iSession1.StartMeasurements();

            System.Threading.Thread.Sleep(iSession.Counts*iSession.IrradiationList.Count*1000 + iSession.IrradiationList.Count*1000);
            System.Threading.Thread.Sleep(iSession1.Counts*iSession1.IrradiationList.Count*1000 + iSession1.IrradiationList.Count*1000);

            var ic = new InfoContext();


        }


        [Fact]
        void MainFunctionalTest()
        {
            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

            var r = new Random();
            var numberOfSession = (int)(4*r.NextDouble()+1);

            var sessionList = new List<ISession>();

            for (var i = 0; i < numberOfSession; ++i)
                sessionList.Add(new Session());

            var numberOfDetectorsForSessions = new List<int>();
            int restDetectors = 4;
            int randomDetectors = 0;

            for (var i = 0; i < numberOfSession; ++i)
            {
                if (restDetectors == 0)
                    break;

                randomDetectors = (int)(restDetectors * r.NextDouble() + 1);
                numberOfDetectorsForSessions.Add(randomDetectors);
                restDetectors -= randomDetectors;
            }

            int n = 0;
            foreach (var session in sessionList)
            {
                for (var j = 0; n < numberOfDetectorsForSessions.Count && j < numberOfDetectorsForSessions[n]; ++j)
                    session.AttachDetector(SessionControllerSingleton.AvailableDetectors[0].Name);
                n++;
            }

            var typesDict = new Dictionary<int, string>() { { 1, "SLI" }, { 2, "LLI-1" }, {3, "LLI-2" } };
            var spreadedOptionDict = new Dictionary<int, SpreadOptions>() { { 0, SpreadOptions.container }, { 1, SpreadOptions.inOrder }, {2, SpreadOptions.uniform } };

            foreach (var session in sessionList)
            {
                session.Type = typesDict[(int)(2 * r.NextDouble() + 1)];
                session.SpreadOption = spreadedOptionDict[(int)(3 * r.NextDouble())];
                session.CurrentIrradiationDate = session.IrradiationDateList[(int)((session.IrradiationDateList.Count - 1) * r.NextDouble())].Value;
                session.Height = (decimal)(20 * r.NextDouble() + 1);
                session.SetAcquireDurationAndMode((int)(4 * r.NextDouble() + 2));
               
            }


            foreach (var session in sessionList)
            {
                var dir = new DirectoryInfo($"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{session.Type.ToLower()}");

                foreach (var file in dir.GetFiles("*.json"))
                    file.Delete();
            }


            foreach (var session in sessionList)
            {
                session.ClearMeasurements();
                session.StartMeasurements();
            }

            var maxCounts = sessionList.Max(s => s.IrradiationList.Count);
            var maxSession = sessionList.Where( s=> s.IrradiationList.Count == maxCounts).First();
            System.Threading.Thread.Sleep(maxSession.IrradiationList.Count*1000 + maxSession.IrradiationList.Count*1000);

            var ic = new InfoContext();

            foreach (var session in sessionList)
            {
                var dir = new DirectoryInfo($"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{session.Type.ToLower()}");
                DataAccess fileSpectra = new DataAccess();
                foreach (var m in session.MeasurementList)
                {
                    var i = session.IrradiationList.Where(ir => ir.Id == m.IrradiationId).First();
                    Assert.Single(dir.GetFiles($"{m.FileSpectra}.json"));



                    fileSpectra.Open(dir.GetFiles($"{m.FileSpectra}.json")[0].FullName);

                    Assert.Equal($"{i.SampleKey}", fileSpectra.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
                    Assert.Equal(i.Assistant, fileSpectra.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name
                    Assert.Equal(i.Note, fileSpectra.Param[ParamCodes.CAM_T_SDESC1].ToString());
                    Assert.Equal(i.SetKey, fileSpectra.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
                    Assert.Equal(i.Weight.ToString(), fileSpectra.Param[ParamCodes.CAM_F_SQUANT].ToString()); // weight
                    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
                    Assert.Equal("gram", fileSpectra.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
                    Assert.Equal(i.DateTimeStart.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
                    Assert.Equal(i.DateTimeFinish.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
                    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
                    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
                    Assert.Equal(m.Type, fileSpectra.Param[ParamCodes.CAM_T_STYPE].ToString());
                    Assert.Equal(m.Height.ToString(), fileSpectra.Param[ParamCodes.CAM_T_SGEOMTRY].ToString());

                    fileSpectra.Close();


                    Assert.Single(ic.Measurements.Where(me =>
                                                                    me.IrradiationId == m.IrradiationId &&
                                                                    me.SetKey == m.SetKey &&
                                                                    me.SampleNumber == m.SampleNumber &&
                                                                    me.Height.Value == m.Height.Value &&
                                                                    me.Type == m.Type &&
                                                                    me.Assistant == m.Assistant &&
                                                                    me.Detector == m.Detector &&
                                                                    me.Duration.Value == m.Duration.Value &&
                                                                    me.DateTimeStart.Value == m.DateTimeStart.Value &&
                                                                    me.DateTimeFinish.Value == m.DateTimeFinish.Value &&
                                                                    me.FileSpectra == m.FileSpectra &&
                                                                    me.Note == m.Note
                                                                 ).ToArray());
                }

            }

        }

    }
}
