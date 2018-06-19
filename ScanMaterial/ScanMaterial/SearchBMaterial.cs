using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace Phicomm_WMS.DB
{
    public class SearchBMaterial : BaseDbQuery
    {
        private DataTable _dt = new DataTable();

        public SearchBMaterial(Dictionary<string, object> dic)
            : base(
                "select material_desc, is_soider_paste_kp, material_no, material_level, process, material_type, material_subtype, user_date, usedate_delay from b_material", DbName)
        {
            _dt.Columns.Add("material_desc", typeof(string));
            _dt.Columns.Add("is_soider_paste_kp", typeof(string));
            _dt.Columns.Add("material_no", typeof(string));
            _dt.Columns.Add("material_level", typeof(string));
            _dt.Columns.Add("process", typeof(string));
            _dt.Columns.Add("material_type", typeof(string));
            _dt.Columns.Add("material_subtype", typeof(string));
            _dt.Columns.Add("user_date", typeof(string));
            _dt.Columns.Add("usedate_delay", typeof(string));

            if (dic != null && dic.Count > 0)
            {
                Sql += " Where ";
                foreach (KeyValuePair<string, object> obj in dic)
                {
                    Sql += " " + obj.Key + "='" + obj.Value + "' AND ";
                }
                Sql = Sql.Substring(0, Sql.Length - 5);
            }
        }

        protected override List<string> ProcessSql(string sql)
        {
            List<string> sqlList = new List<string> { sql };
            return sqlList;
        }

        protected override void ProcessResultSet(MySqlDataReader reader)
        {
            try
            {
                while (reader.Read())
                {
                    _dt.Rows.Add(reader.IsDBNull(0) ? "": reader.GetString(0),
                                 reader.IsDBNull(1) ? "": reader.GetString(1),
                                 reader.IsDBNull(2) ? "":reader.GetString(2),
                                 reader.IsDBNull(3) ? "" : reader.GetString(3),
                                 reader.IsDBNull(4) ? "" : reader.GetString(4),
                                 reader.IsDBNull(5) ? "" : reader.GetString(5),
                                 reader.IsDBNull(6) ? "" : reader.GetString(6),
                                 reader.IsDBNull(7) ? "" : reader.GetString(7),
                                 reader.IsDBNull(8) ? "" : reader.GetString(8)
                                 );
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader?.Close();
            }
        }

        public DataTable GetResult()
        {
            return _dt;
        }
    }
}
