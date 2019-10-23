using System.Linq;
using System;
using Xunit;
using System.Data.SqlClient;

namespace Measurements.Core.Tests
{
    public class IrradiationInfoTest
    {
        [Fact]
        public void GetValues()
        {
            int id = -1;
            var ni = new IrradiationInfo();
            using (var sqw = new SqlConnection(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;"))
            {
                sqw.Open();
                using (var sqm = new SqlCommand($"select max(Id) from Irradiations", sqw))
                {
                    Random r = new Random();
                    id = r.Next(1, (int)sqm.ExecuteScalar());
                }
                using (var sqc = new SqlCommand($"select * from Irradiations where Id={id}", sqw))
                {
                    using (var sqr = sqc.ExecuteReader())
                    {
                        sqr.Read();
                        ni.Id             = (int)      sqr.GetValue(0);
                        ni.CountryCode    = (string)   sqr.GetValue(1);
                        ni.ClientNumber   = (string)   sqr.GetValue(2);
                        ni.Year           = (string)   sqr.GetValue(3);
                        ni.SetNumber      = (string)   sqr.GetValue(4);
                        ni.SetIndex       = (string)   sqr.GetValue(5);
                        ni.SampleNumber   = (string)   sqr.GetValue(6);
                        ni.Type           = (string)   sqr.GetValue(7);
                        if (!DBNull.Value.Equals(sqr.GetValue(8)))
                            ni.Weight         = (decimal)  sqr.GetValue(8);
                        ni.DateTimeStart  = (DateTime) sqr.GetValue(9);
                        ni.Duration       = (int)      sqr.GetValue(10);
                        ni.DateTimeFinish = (DateTime) sqr.GetValue(11);
                        if (!DBNull.Value.Equals(sqr.GetValue(12)))
                            ni.Container      = (short?)   sqr.GetValue(12);
                        if (!DBNull.Value.Equals(sqr.GetValue(13)))
                            ni.Position       = (short?)   sqr.GetValue(13);
                        if (!DBNull.Value.Equals(sqr.GetValue(14)))
                            ni.Channel        = (short?)   sqr.GetValue(14);
                        if (!DBNull.Value.Equals(sqr.GetValue(15)))
                            ni.LoadNumber     = (int)      sqr.GetValue(15);
                        if (!DBNull.Value.Equals(sqr.GetValue(16)))
                            ni.Rehandler      = (string)   sqr.GetValue(16);
                        if (!DBNull.Value.Equals(sqr.GetValue(17)))
                            ni.Assistant      = (string)   sqr.GetValue(17);

                        if (!DBNull.Value.Equals(sqr.GetValue(18)))
                            ni.Note       = (string)   sqr.GetValue(18);
                    }
                }
            }

            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            var ic = new InfoContext();
            var i = ic.Irradiations.Where(qi => qi.Id == id).FirstOrDefault();

            Assert.Equal(ni.Id,                    i.Id);
            Assert.Equal(ni.CountryCode,           i.CountryCode);
            Assert.Equal(ni.ClientNumber,          i.ClientNumber);
            Assert.Equal(ni.Year,                  i.Year);
            Assert.Equal(ni.SetNumber,             i.SetNumber);
            Assert.Equal(ni.SetIndex,              i.SetIndex);
            Assert.Equal(ni.SampleNumber,          i.SampleNumber);
            Assert.Equal(ni.Type,                  i.Type);
            if (ni.Weight.HasValue)
                Assert.Equal((double)ni.Weight.Value,  (double)i.Weight.Value, 4);
            if (ni.Weight.HasValue)
            Assert.Equal(ni.DateTimeStart.Value,   i.DateTimeStart.Value);
            if (ni.DateTimeStart.HasValue)
            Assert.Equal(ni.Duration.Value,        i.Duration.Value);
            if (ni.DateTimeFinish.HasValue)
            Assert.Equal(ni.DateTimeFinish.Value,  i.DateTimeFinish.Value);
            if (ni.Container.HasValue)
            Assert.Equal(ni.Container.Value,       i.Container.Value);
            if (ni.Position.HasValue)
            Assert.Equal(ni.Position.Value,        i.Position.Value);
            if (ni.Channel.HasValue)
            Assert.Equal(ni.Channel.Value,         i.Channel.Value);
            if (ni.LoadNumber.HasValue)
            Assert.Equal(ni.LoadNumber.Value,      i.LoadNumber.Value);
            Assert.Equal(ni.Rehandler,             i.Rehandler);
            Assert.Equal(ni.Assistant,             i.Assistant);
            Assert.Equal(ni.Note,                  i.Note);
        }
    }
  public class MeasurementInfoTest
    {
        [Fact]
        public void GetValues()
        {
            int id = -1;
            var mi = new MeasurementInfo();
            using (var sqw = new SqlConnection(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;"))
            {
                sqw.Open();
                using (var sqm = new SqlCommand($"select max(Id) from Measurements", sqw))
                {
                    Random r = new Random();
                    id = r.Next(1, (int)sqm.ExecuteScalar());
                }
                using (var sqc = new SqlCommand($"select * from Measurements where Id={id}", sqw))
                {
                    using (var sqr = sqc.ExecuteReader())
                    {
                        sqr.Read();
                        mi.Id                 = (int)      sqr.GetValue(0);
                        mi.IrradiationId      = (int)      sqr.GetValue(1);
                        mi.CountryCode        = (string)   sqr.GetValue(2);
                        mi.ClientNumber       = (string)   sqr.GetValue(3);
                        mi.Year               = (string)   sqr.GetValue(4);
                        mi.SetNumber          = (string)   sqr.GetValue(5);
                        mi.SetIndex           = (string)   sqr.GetValue(6);
                        mi.SampleNumber       = (string)   sqr.GetValue(7);
                        mi.Type               = (string)   sqr.GetValue(8);
                        mi.DateTimeStart      = (DateTime)  sqr.GetValue(9);

                        if (!DBNull.Value.Equals(sqr.GetValue(10)))
                            mi.Duration       = (int)      sqr.GetValue(10);

                        if (!DBNull.Value.Equals(sqr.GetValue(11)))
                            mi.DateTimeFinish = (DateTime) sqr.GetValue(11);

                        if (!DBNull.Value.Equals(sqr.GetValue(12)))
                            mi.Height         = (short)    sqr.GetValue(12);

                        mi.FileSpectra        = (string)   sqr.GetValue(13);
                        mi.Detector           = (string)   sqr.GetValue(14);

                        if (!DBNull.Value.Equals(sqr.GetValue(15)))
                            mi.Assistant      = (string)sqr.GetValue(15);

                        if (!DBNull.Value.Equals(sqr.GetValue(16)))
                            mi.Note           = (string)sqr.GetValue(16);
                    }
                }
            }

            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");
            var mc = new InfoContext();
            var m = mc.Measurements.Where(qm => qm.Id == id).FirstOrDefault();

            Assert.Equal(mi.Id,                    m.Id);
            Assert.Equal(mi.IrradiationId,         m.IrradiationId);
            Assert.Equal(mi.CountryCode,           m.CountryCode);
            Assert.Equal(mi.ClientNumber,          m.ClientNumber);
            Assert.Equal(mi.Year,                  m.Year);
            Assert.Equal(mi.SetNumber,             m.SetNumber);
            Assert.Equal(mi.SetIndex,              m.SetIndex);
            Assert.Equal(mi.SampleNumber,          m.SampleNumber);
            Assert.Equal(mi.Type,                  m.Type);
            if (mi.DateTimeStart.HasValue)
            Assert.Equal(mi.DateTimeStart.Value,   m.DateTimeStart.Value);
            if (mi.Duration.HasValue)
            Assert.Equal(mi.Duration.Value,        m.Duration.Value);
            if (mi.DateTimeFinish.HasValue)
            Assert.Equal(mi.DateTimeFinish.Value,  m.DateTimeFinish.Value);
            if (mi.Height.HasValue)
            Assert.Equal(mi.Height.Value,          m.Height.Value);
            Assert.Equal(mi.FileSpectra,           m.FileSpectra);
            Assert.Equal(mi.Detector,              m.Detector);
            Assert.Equal(mi.Assistant,             m.Assistant);
            Assert.Equal(mi.Note,                  m.Note);
        }
   }
    public class SessionInfoTest
    {
        [Fact]
        public void GetValues()
        {
            var si = new SessionInfo();
            using (var sqw = new SqlConnection(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;"))
            {
                sqw.Open();
                using (var sqc = new SqlCommand($"select * from Sessions where Name='bdrum-test'", sqw))
                {
                    using (var sqr = sqc.ExecuteReader())
                    {
                        sqr.Read();
                        si.Name           = (string)  sqr.GetValue(0);
                        si.DetectorsNames = (string)  sqr.GetValue(1);
                        si.Type           = (string)  sqr.GetValue(2);
                        si.CountMode      = (string)  sqr.GetValue(3);
                        si.Assistant      = (string)  sqr.GetValue(7);
                        si.Note           = (string)  sqr.GetValue(8);
                    }
                }
            }

            SessionControllerSingleton.InitializeDBConnectionString(@"Server=RUMLAB\REGATALOCAL;Database=NAA_DB_TEST;Trusted_Connection=True;");

            var sc = new InfoContext();
            var s = sc.Sessions.Where(s1 => s1.Name == "bdrum-test").FirstOrDefault();
            Assert.Equal(si.Name,           s.Name);
            Assert.Equal(si.DetectorsNames, s.DetectorsNames);
            Assert.Equal(si.Type,           s.Type);
            Assert.Equal(si.CountMode,      s.CountMode);
            Assert.Equal(si.Assistant,      s.Assistant);
            Assert.Equal(si.Note,           s.Note);
        }
    }
 
}
