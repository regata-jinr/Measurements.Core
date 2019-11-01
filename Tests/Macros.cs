//using Xunit;
//using System.Collections.Generic;
//using System;
//using System.IO;
//using System.Linq;
//using CanberraDataAccessLib;
//using Xunit.Abstractions;

//namespace Measurements.Core.Tests
//{
//    public class Macros
//    {

//        private readonly ITestOutputHelper output;
//        public Macros(ITestOutputHelper output)
//        {
//            this.output = output;
//        }

//        private int NumberSessionWhichHasDone = 0;

//        private void CallWhenSessionHasDone()
//        {
//            NumberSessionWhichHasDone++;
//        }

//        [Fact]
//        void MainFunctionalTest()
//        {
//            //SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;User Id=bdrum;");

//            //var r = new Random();
//            //var numberOfSession = (int)(4*r.NextDouble()+1);

//            //var sessionList = new List<ISession>();

//            //for (var i = 0; i < numberOfSession; ++i)
//            //{
//            //    sessionList.Add(new Session());
//            //    sessionList[i].Name = $"Session{i}";
//            //    sessionList[i].SessionComplete += CallWhenSessionHasDone;
//            //}

//            //var numberOfDetectorsForSessions = new List<int>();
//            //int restDetectors = 4;
//            //int randomDetectors = 0;

//            //for (var i = 0; i < numberOfSession; ++i)
//            //{
//            //    if (restDetectors == 0)
//            //        break;

//            //    randomDetectors = (int)(restDetectors * r.NextDouble() + 1);
//            //    numberOfDetectorsForSessions.Add(randomDetectors);
//            //    restDetectors -= randomDetectors;
//            //}

//            //int n = 0;
//            //foreach (var session in sessionList)
//            //{
//            //    for (var j = 0; n < numberOfDetectorsForSessions.Count && j < numberOfDetectorsForSessions[n]; ++j)
//            //        session.AttachDetector(SessionControllerSingleton.AvailableDetectors[0].Name);
//            //    n++;
//            //}

//            //var typesDict = new Dictionary<int, string>() { { 1, "SLI" }, { 2, "LLI-1" }, {3, "LLI-2" } };
//            //var spreadedOptionDict = new Dictionary<int, SpreadOptions>() { { 0, SpreadOptions.container }, { 1, SpreadOptions.inOrder }, {2, SpreadOptions.uniform } };

           
//            //foreach (var session in sessionList)
//            //{
//            //    var dir = new DirectoryInfo($"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{session.Type.ToLower()}");

//            //    foreach (var file in dir.GetFiles("*.json"))
//            //        file.Delete();
//            //}

//            //foreach (var session in sessionList)
//            //{
//            //    session.ClearMeasurements();
//            //    session.StartMeasurements();
//            //}

//            //while (NumberSessionWhichHasDone != sessionList.Count)
//            //{ }

//            //var ic = new InfoContext();

//            //foreach (var session in sessionList)
//            //{
//            //    var dir = new DirectoryInfo($"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{session.Type.ToLower()}");
//            //    DataAccess fileSpectra = new DataAccess();

//            //    foreach (var m in session.MeasurementList)
//            //    {
//            //        output.WriteLine($"Current measurement is {m} with id {m.IrradiationId}");
//            //        output.WriteLine($"Checking of file spectra {dir.FullName}\\{m.FileSpectra} -- {File.Exists($"{dir.FullName}\\{m.FileSpectra}.cnf")}");

//            //        var i = session.IrradiationList.Where(ir => ir.Id == m.IrradiationId).Last();

//            //        Assert.True(File.Exists($"{dir.FullName}\\{m.FileSpectra}.cnf"));

//            //        fileSpectra.Open(dir.GetFiles($"{m.FileSpectra}.cnf")[0].FullName);

//            //        Assert.Equal($"{i.SampleKey}", fileSpectra.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
//            //        Assert.Equal(m.Assistant, fileSpectra.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name

//            //        if (string.IsNullOrEmpty(i.Note))
//            //            Assert.True(string.IsNullOrEmpty(fileSpectra.Param[ParamCodes.CAM_T_SDESC1].ToString()));
//            //        else
//            //            Assert.Equal(i.Note, fileSpectra.Param[ParamCodes.CAM_T_SDESC1].ToString());

//            //        Assert.Equal(i.SetKey, fileSpectra.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
//            //        if (i.Weight.HasValue)
//            //            Assert.Equal(i.Weight.Value, Decimal.Parse(fileSpectra.Param[ParamCodes.CAM_F_SQUANT].ToString()),2); // weight
//            //        Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
//            //        Assert.Equal("gram", fileSpectra.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
//            //        Assert.Equal(i.DateTimeStart.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
//            //        Assert.Equal(i.DateTimeFinish.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
//            //        Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
//            //        Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
//            //        Assert.Equal(m.Type, fileSpectra.Param[ParamCodes.CAM_T_STYPE].ToString());
//            //        Assert.Equal(m.Height.Value.ToString(), fileSpectra.Param[ParamCodes.CAM_T_SGEOMTRY].ToString());

//            //        fileSpectra.Close();

//            //        output.WriteLine($"Checking of measurement for irradiated sample with id {m.IrradiationId}");
//            //        Assert.Single(ic.Measurements.Where(me =>
//            //                                                        me.IrradiationId == m.IrradiationId &&
//            //                                                        me.SetKey == m.SetKey &&
//            //                                                        me.SampleNumber == m.SampleNumber &&
//            //                                                        //me.Height.Value == m.Height.Value &&
//            //                                                        me.Type == m.Type &&
//            //                                                        me.Assistant == m.Assistant &&
//            //                                                        me.Detector == m.Detector &&
//            //                                                        me.Duration.Value == m.Duration.Value &&
//            //                                                        //me.DateTimeStart.Value == m.DateTimeStart.Value &&
//            //                                                        //me.DateTimeFinish.Value == m.DateTimeFinish.Value &&
//            //                                                        me.FileSpectra == m.FileSpectra
//            //                                                        //me.Note == m.Note
//            //                                             ).ToArray());
//            //    }
//            //}

//        }// MainFunctionalTest(


//        [Fact]
//        public void FastTest()
//        {
//            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
//            SessionControllerSingleton.ConnectionStringBuilder.UserID = "bdrum";

//            var iSession = SessionControllerSingleton.Create();

//            iSession.SessionComplete += CallWhenSessionHasDone;
//            iSession.AttachDetector("D1");
//            iSession.AttachDetector("D5");
//            iSession.AttachDetector("D7");
//            iSession.Type = "LLI-2";

//            List<MeasurementInfo> MeasurementList = null;
//            List<IrradiationInfo> IrradiationList = null;

//            using (var inf = new InfoContext())
//            {
//                MeasurementList = inf.Measurements.Where(m => m.DateTimeStart.HasValue && m.DateTimeStart.Value.Date == DateTime.Now.Date && m.Type == iSession.Type).ToList();

//                foreach (var m in MeasurementList)
//                    IrradiationList.Add(inf.Irradiations.Where(ir => ir.Id == m.IrradiationId).First());
//            }

//            if (MeasurementList.Count != IrradiationList.Count) throw new EntryPointNotFoundException("Какой-то из облученных образцов не был найден программой");

//            var spreadedSamples = new Dictionary<string, List<MeasurementInfo>>();

//            foreach (var d in iSession.ManagedDetectors)
//                spreadedSamples.Add(d.Name, MeasurementList.Where(m => m.Detector == d.Name).ToList());

//            iSession.MeasurementOfSampleDone += ISession_MeasurementOfSampleDone;

//            iSession.ClearMeasurements();

//            foreach (var m in MeasurementList)
//                m.Note = "TEST!";

//            foreach (var d in iSession.ManagedDetectors)
//            {
//                IDetector det = d;
//                var CurrentMeasurement = spreadedSamples[d.Name][0];
//                var CurrentIrradiations = IrradiationList.Where(ir => ir.Id == spreadedSamples[d.Name].First().IrradiationId).First();
//            }
            
//            iSession.StartMeasurements();

//            while (NumberSessionWhichHasDone != 1)
//            { }
//            var ic = new InfoContext();
//            var dir = new DirectoryInfo($"D:\\Spectra\\{DateTime.Now.Year}\\{DateTime.Now.Month.ToString("D2")}\\{iSession.Type.ToLower()}");
//            DataAccess fileSpectra = new DataAccess();
//            //foreach (var m in iSession.MeasurementList)
//            //{
//            //    output.WriteLine($"Current measurement is {m} with id {m.IrradiationId}");
//            //    output.WriteLine($"Checking of file spectra {dir.FullName}\\{m.FileSpectra} -- {File.Exists($"{dir.FullName}\\{m.FileSpectra}.cnf")}");

//            //    var i = iSession.IrradiationList.Where(ir => ir.Id == m.IrradiationId).Last();

//            //    Assert.True(File.Exists($"{dir.FullName}\\{m.FileSpectra}.cnf"));

//            //    fileSpectra.Open(dir.GetFiles($"{m.FileSpectra}.cnf")[0].FullName);

//            //    Assert.Equal($"{i.SampleKey}", fileSpectra.Param[ParamCodes.CAM_T_STITLE].ToString()); // title
//            //    Assert.Equal(m.Assistant, fileSpectra.Param[ParamCodes.CAM_T_SCOLLNAME].ToString()); // operator's name

//            //    if (string.IsNullOrEmpty(i.Note))
//            //        Assert.True(string.IsNullOrEmpty(fileSpectra.Param[ParamCodes.CAM_T_SDESC1].ToString()));
//            //    else
//            //        Assert.Equal(i.Note, fileSpectra.Param[ParamCodes.CAM_T_SDESC1].ToString());

//            //    Assert.Equal(i.SetKey, fileSpectra.Param[ParamCodes.CAM_T_SIDENT].ToString()); // sd code
//            //    if (i.Weight.HasValue)
//            //        Assert.Equal(i.Weight.Value, Decimal.Parse(fileSpectra.Param[ParamCodes.CAM_F_SQUANT].ToString()), 2); // weight
//            //    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SQUANTERR].ToString()); // err, 0
//            //    Assert.Equal("gram", fileSpectra.Param[ParamCodes.CAM_T_SUNITS].ToString()); // units, gram
//            //    Assert.Equal(i.DateTimeStart.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_SDEPOSIT].ToString().Replace(" ", "")); // irr start date time
//            //    Assert.Equal(i.DateTimeFinish.ToString().Replace(" ", ""), fileSpectra.Param[ParamCodes.CAM_X_STIME].ToString().Replace(" ", "")); // irr finish date time
//            //    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSERR].ToString()); // Random sd error (%)
//            //    Assert.Equal("0", fileSpectra.Param[ParamCodes.CAM_F_SSYSTERR].ToString()); // Non-random sd error 
//            //    Assert.Equal(m.Type, fileSpectra.Param[ParamCodes.CAM_T_STYPE].ToString());
//            //    Assert.Equal(m.Height.ToString(), fileSpectra.Param[ParamCodes.CAM_T_SGEOMTRY].ToString());

//            //    fileSpectra.Close();

//            //    output.WriteLine($"Checking of measurement for irradiated sample with id {m.IrradiationId}");
//            //    Assert.Single(ic.Measurements.Where(me =>
//            //                                                    me.IrradiationId == m.IrradiationId &&
//            //                                                    me.SetKey == m.SetKey &&
//            //                                                    me.SampleNumber == m.SampleNumber &&
//            //                                                    //me.Height.Value == m.Height.Value &&
//            //                                                    me.Type == m.Type &&
//            //                                                    me.Assistant == m.Assistant &&
//            //                                                    me.Detector == m.Detector &&
//            //                                                    me.Duration.Value == m.Duration.Value &&
//            //                                                    //me.DateTimeStart.Value == m.DateTimeStart.Value &&
//            //                                                    //me.DateTimeFinish.Value == m.DateTimeFinish.Value &&
//            //                                                    me.FileSpectra == m.FileSpectra
//            //                                         //me.Note == m.Note
//            //                                         ).ToArray());
//            //}
//        }

//        private void ISession_MeasurementOfSampleDone(MeasurementInfo obj)
//        {
//            //IDetector det = ;
//            //var CurrentMeasurement = spreadedSamples[d.Name][0];
//            //var CurrentIrradiations = IrradiationList.Where(ir => ir.Id == spreadedSamples[d.Name].First().IrradiationId).First();
//            //iSession.StartMeasurements(ref det, ref CurrentMeasurement, ref CurrentIrradiations);
//        }
//    } // test class
//} // test namespace
