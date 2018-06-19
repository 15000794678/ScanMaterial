using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace Phicomm_WMS.DB
{
    public abstract class BaseDbQuery : BaseDbConnection
    {
        private static string _strConn;

        protected BaseDbQuery(string sql,string dbName)
        {
            _strConn = string.Format(BaseConStr, dbName);
            Sql = sql.Trim();
            if (!Sql.ToLower().StartsWith("select "))
                throw new Exception("Only support select statement. Current sql "+Sql);
        }

        protected abstract void ProcessResultSet(MySqlDataReader reader);
        protected abstract List<string> ProcessSql(string sql);

        public void ExecuteQuery()
        {
            using (MySqlConnection conn = new MySqlConnection(_strConn))
            {
                try
                {
                    MySqlCommand mySqlCmd = new MySqlCommand();
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    mySqlCmd.Connection = conn;
                    List<string> sqlList = ProcessSql(Sql);
                    foreach (string sql in sqlList)
                    {
                        mySqlCmd.CommandText = sql;
                        mySqlCmd.CommandType = CommandType.Text;
                        MySqlDataReader reader = mySqlCmd.ExecuteReader();
                        ProcessResultSet(reader);
                    }
                }
                catch (Exception ex)
                {
                    throw  ex;
                }
                finally
                {
                    if (conn.State != ConnectionState.Closed)
                        conn.Close();
                }

            }
        }
    }
}
