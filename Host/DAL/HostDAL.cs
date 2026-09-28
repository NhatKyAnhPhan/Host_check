using System;
using DAL;

namespace DAL
{
    public class HostDAL
    {
        Connect_DB db = new Connect_DB();

        // Lấy 1 IP host đang active
        public string GetActiveHostIP()
        {
            string sql = @"
                SELECT TOP 1 IPAddress
                FROM Hosts
                WHERE IsActive = 1
                ORDER BY HostID
            ";

            object result = db.ExecuteScalar(sql);
            return result?.ToString();
        }
    }
}
