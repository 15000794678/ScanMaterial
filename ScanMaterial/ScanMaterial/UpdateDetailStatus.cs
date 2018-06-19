using System.Collections.Generic;
using System.Data;

namespace Phicomm_WMS.DB
{
    public class UpdateRInventoryDetailStatus : BaseDbUpdater
    {
        private List<string> _sqlList = new List<string>();

        public UpdateRInventoryDetailStatus(string trsn, string oldstatus, string newstatus)
            : base("Update r_inventory_detail Set status='" + newstatus + "' Where trsn='" + trsn + "' And status='" + oldstatus + "'", DbName)
        {
            _sqlList.Add(Sql);
        }

        protected override List<string> ProcessSql(string sql)
        {
            return _sqlList;
        }
    }
}