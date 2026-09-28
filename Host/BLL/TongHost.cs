using DAL;
using System;

namespace BLL
{
    public class TongHost
    {
        Connect_DB db = new Connect_DB();

        public int GetTotalHost()
        {
            string sql = "SELECT COUNT(*) FROM Hosts";
            object result = db.ExecuteScalar(sql);
            return result == null ? 0 : Convert.ToInt32(result);
        }
    }
}
