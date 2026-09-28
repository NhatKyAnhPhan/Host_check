using System;
using System.Data;
using System.Data.SqlClient;
using DAL;

namespace BLL
{
    public class HostStats
    {
        private readonly Connect_DB db = new Connect_DB();

        public DataTable GetLatestCPUByHost()
        {
            DataTable dt = new DataTable();

            string sql = @"
                SELECT h.HostName,
                       hs.CPU
                FROM Hosts h
                INNER JOIN Host_Stats hs ON h.HostID = hs.HostID
                WHERE hs.RecordedAt = (
                    SELECT MAX(RecordedAt)
                    FROM Host_Stats
                    WHERE HostID = h.HostID
                )
                AND h.IsActive = 1
                ORDER BY hs.CPU DESC
            ";

            try
            {
                using (SqlConnection conn = db.get_connect())
                using (SqlDataAdapter da = new SqlDataAdapter(sql, conn))
                {
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi lấy CPU Host: " + ex.Message);
            }

            return dt;
        }
    }
}
