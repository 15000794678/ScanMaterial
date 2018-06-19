
namespace Phicomm_WMS.DB
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseDbConnection
    {
        protected static string BaseConStr = @"Database = {0}; Data Source = 172.16.173.241; User Id = mesmysql; " +
                                     "Password =dev#store@2018; Max Pool Size = 400;";

        //protected static string BaseConStr = @"Database = {0}; Data Source = 127.0.0.1; User Id = root; " +
        //                 "Password =root; Max Pool Size = 400;";

        protected static string DbName = "autowms";

        protected string Sql;

        public BaseDbConnection()
        {
            
        }
    }
}
