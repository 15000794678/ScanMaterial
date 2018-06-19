using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace Phicomm_WMS.DB
{
    public abstract class BaseDbStoredProcedureCaller:BaseDbConnection
    {
        private static string _strConn;
        protected readonly Dictionary<string, MySqlParameter> DicParameters = new Dictionary<string, MySqlParameter>();

        protected BaseDbStoredProcedureCaller(string dbName ,string funcName, params MySqlParameter[] parameters)
        {
            Sql = funcName;
            foreach (var parameter in parameters)
            {
                DicParameters.Add(parameter.ParameterName, parameter);
            }
            _strConn = string.Format(BaseConStr, dbName);
        }

        protected abstract void ProcessResult(MySqlDataReader reader);

        public bool ExecuteQuery()
        {
            using (MySqlConnection conn = new MySqlConnection(_strConn))
            {
                try
                {
                    MySqlCommand mySqlCmd = new MySqlCommand();
                    if (conn.State != ConnectionState.Open)
                        conn.Open();
                    mySqlCmd.Connection = conn;
                    mySqlCmd.CommandText = Sql;
                    mySqlCmd.CommandType = CommandType.StoredProcedure;
                    mySqlCmd.CommandTimeout = 84100;
                    mySqlCmd.Parameters.AddRange(DicParameters.Values.ToArray());
                    mySqlCmd.ExecuteNonQuery();
                    using (MySqlDataReader reader = mySqlCmd.ExecuteReader())
                    {
                        ProcessResult(reader);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
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
