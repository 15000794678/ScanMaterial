using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Data;
using System;

namespace Phicomm_WMS.DB
{
    /// <summary>
    /// 根据入库单筛选出对应的货品清单和信息
    /// </summary>
    public class SearchRInventoryDetail : BaseDbQuery
    {
        private DataTable _dt = null;

        public SearchRInventoryDetail(Dictionary<string, object> dic) :
            base("select tr_sn, material_no, vender_no, date_code, lot_code, qty, status, stock_id, loc_id, fifo_datecode, inventory_date, stock_no, stock_out_no from r_inventory_detail ", DbName)
        {
            _dt = new DataTable();
            _dt.Columns.Add("TR_SN", typeof(string));
            _dt.Columns.Add("KP_NO", typeof(string));
            _dt.Columns.Add("VENDER_ID", typeof(string));
            _dt.Columns.Add("DATE_CODE", typeof(string));
            _dt.Columns.Add("LOT_CODE", typeof(string));
            _dt.Columns.Add("QTY", typeof(int));
            _dt.Columns.Add("STATUS", typeof(string));
            _dt.Columns.Add("KP_DESC", typeof(string));
            _dt.Columns.Add("STOCK_ID", typeof(string));
            _dt.Columns.Add("LOC_ID", typeof(string));
            _dt.Columns.Add("VENDER_NAME", typeof(string));
            _dt.Columns.Add("FIFO_DATECODE", typeof(string));
            _dt.Columns.Add("INVENTORY_DATE", typeof(string));
            _dt.Columns.Add("STOCK_NO", typeof(string));
            _dt.Columns.Add("STOCK_OUT_NO", typeof(string));

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
                    _dt.Rows.Add(
                                reader.IsDBNull(0)? "" : reader.GetString(0),
                                reader.IsDBNull(1) ? "" : reader.GetString(1),
                                reader.IsDBNull(2) ? "" : reader.GetString(2),
                                reader.IsDBNull(3) ? "" : reader.GetString(3),
                                reader.IsDBNull(4) ? "" : reader.GetString(4),
                                reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                reader.IsDBNull(6) ? "" : reader.GetString(6),
                                "",  //KP_DESC
                                reader.IsDBNull(7) ? "" : reader.GetString(7),
                                reader.IsDBNull(8) ? "" : reader.GetString(8),
                                "", //VENDER_NAME
                                reader.IsDBNull(9) ? "" : reader.GetString(9),
                                reader.IsDBNull(10) ? "" : reader.GetString(10),
                                reader.IsDBNull(11) ? "" :reader.GetString(11),
                                reader.IsDBNull(12) ? "" : reader.GetString(12)
                                );
                }
            }
            catch(Exception ex)
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
