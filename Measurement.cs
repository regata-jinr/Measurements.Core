using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

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
        private DataTable mTable;
        private readonly Dictionary<string, string> QTJournalsDate = new Dictionary<string, string> { { "radioButtonSLI", "select distinct Date_Start  from table_SLI_Irradiation_Log order by Date_Start desc;" }, { "radioButtonLLI1", "select distinct Date_Start  from table_LLI_Irradiation_Log order by Date_Start desc;" }, { "radioButtonLLI2", "select distinct Date_Start  from table_LLI_Irradiation_Log order by Date_Start desc;" }, { "radioButtonBgrn", "" } };

        public List<DateTime> GetJournalsDates(string type)
        {
            List<DateTime> JList = new List<DateTime>();
            if (string.IsNullOrEmpty(type)) return JList;
            SqlCommand sCmd = new SqlCommand(QTJournalsDate[type], mCon);
            mCon.Open();
            SqlDataReader reader = sCmd.ExecuteReader();
            while (reader.Read()) JList.Add(reader.GetDateTime(0));
            mCon.Close();
            return JList;
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
