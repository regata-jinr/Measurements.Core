using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Measurements
{
    class Measurement
    {
        private SqlConnection mCon;
        public string Type { get; set; }
        public int FileNumber { get; set; }
        public string mOperatorName { get; set; }
        public int mDuration { get; set; }
        public DateTime mDateStart { get; set; }
        public DateTime mTimeStart { get; set; }
        public DateTime mDateFinish { get; set; }
        public DateTime mTimeFinish { get; set; }
        public float Height { get; set; }

        private readonly Dictionary<string, string> QTJournalsDate = new Dictionary<string, string> { { "radioButtonSLI", "SLI" }, { "radioButtonLLI1", "LLI" }, { "radioButtonLLI2", "LLI" } };

        //private Dictionary<string, int> uniqSetsCnt;

        public Dictionary<string, int> UniqSetsCnt { get; private set; }

        public List<DateTime> GetJournalsDates(string type)
        {
            List<DateTime> JList = new List<DateTime>();
            if (string.IsNullOrEmpty(type)) return JList;
            SqlCommand sCmd = new SqlCommand($"select distinct Date_Start  from table_{QTJournalsDate[type]}_Irradiation_Log order by Date_Start desc;", mCon);
            mCon.Open();
            SqlDataReader reader = sCmd.ExecuteReader();
            while (reader.Read()) JList.Add(reader.GetDateTime(0));
            mCon.Close();
            return JList;
        }

        // TODO: this should refactor. only for demonstartion!
        public DataTable GetMeasurementsData(DateTime date, string type)
        {
            DataTable mTable = new DataTable();
            SqlCommand sCmd = new SqlCommand($"select Country_Code + '-' + Client_Id + '-' + Year + '-' + Sample_Set_ID + '-' + Sample_Set_Index as SampleSetId, Sample_ID, null Date_Start, null Time_Start, null Date_Finish, null Time_Finish, null Duration, null DetectorN, null CellN, null FileN, null as Height, null as Weight from table_{QTJournalsDate[type]}_Irradiation_Log where Date_Start = '{date.ToShortDateString()}'", mCon);
            mCon.Open();
            SqlDataAdapter a = new SqlDataAdapter(sCmd);
            a.Fill(mTable);
            
            sCmd.CommandText = $"select SampleSetId, count(*) from ( {sCmd.CommandText}) as a group by SampleSetId";
            SqlDataReader reader = sCmd.ExecuteReader();
            UniqSetsCnt = new Dictionary<string, int>();
            while (reader.Read())
                UniqSetsCnt.Add(reader[0].ToString(), (int)reader[1]);
            mCon.Close();
            return mTable;
        }

        public Measurement(SqlConnection con)
        {
            mCon = con;
          
        }
        public void Start() { }
        public void Continue() { }
        public void Reset() { }
        public void Stop() { }
        public void OpenMVCG() { }
        public void SetSampleInfo(Sample s) { }
        public void InitializeFields() { }
        public bool CheckDependencies() { return false; } //check Detectors, Samples, FileNumbers, SampleChangers, System, DBConnections,
    }
}
