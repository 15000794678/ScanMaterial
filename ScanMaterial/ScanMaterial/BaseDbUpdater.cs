using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace Phicomm_WMS.DB
{
    public abstract class BaseDbUpdater:BaseDbConnection
    {
        private static string _strConn;

        protected BaseDbUpdater(string sql, string dbName)
        {
            _strConn = string.Format(BaseConStr, dbName);
            
                Sql = sql.Trim();
                if (!Sql.ToLower().StartsWith("insert ") && !Sql.ToLower().StartsWith("update "))
                    throw new Exception("only support insert, update statement. Current sql:" + Sql);
        }

        protected abstract List<string> ProcessSql(string sql);

        public void ExecuteUpdate()
        {
            using (MySqlConnection conn = new MySqlConnection(_strConn))
            {
                try
                {
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    List<string> sqlList = ProcessSql(Sql);
                    foreach (string sql in sqlList)
                    {
                        MySqlCommand mySqlCmd = new MySqlCommand(sql, conn) {CommandType = CommandType.Text};
                        if (mySqlCmd.ExecuteNonQuery() < 0)
                            throw new Exception("execute sql error:" + sql);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if(conn.State != ConnectionState.Closed)
                        conn.Close();
                }
            }
        }
    }
}
