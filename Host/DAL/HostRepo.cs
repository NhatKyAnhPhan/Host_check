using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class HostRepo
    {
        private readonly Connect_DB db = new Connect_DB();

        // ===============================
        // 1️⃣ LẤY DANH SÁCH HOST
        // ===============================
        public List<HostInfo> GetAllHosts()
        {
            List<HostInfo> list = new List<HostInfo>();

            string query = @"
                SELECT h.HostID, h.HostName, h.IPAddress, h.Description, h.IsActive, h.LastSeen,
                       ISNULL(s.CPU, 0) AS CPU,
                       ISNULL(s.RAM, 0) AS RAM,
                       ISNULL(s.Disk, 0) AS Disk
                FROM Hosts h
                OUTER APPLY (
                    SELECT TOP 1 *
                    FROM Host_Stats
                    WHERE HostID = h.HostID
                    ORDER BY RecordedAt DESC
                ) s";

            using (SqlConnection conn = db.get_connect())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            list.Add(new HostInfo
                            {
                                HostID = Convert.ToInt32(rd["HostID"]),
                                Name = rd["HostName"].ToString(),
                                IP = rd["IPAddress"].ToString(),
                                Description = rd["Description"] != DBNull.Value
                                    ? rd["Description"].ToString()
                                    : "",
                                Status = rd["IsActive"] != DBNull.Value && (bool)rd["IsActive"],
                                LastCheck = rd["LastSeen"] != DBNull.Value
                                    ? (DateTime)rd["LastSeen"]
                                    : DateTime.MinValue,
                                CPU = Convert.ToSingle(rd["CPU"]),
                                RAM = Convert.ToSingle(rd["RAM"]),
                                DISK = Convert.ToSingle(rd["Disk"]),
                                ResponseTime = 0
                            });
                        }
                    }
                }
            }
            return list;
        }

        // ===============================
        // 2️⃣ THÊM HOST (CHỐNG TRÙNG IP)
        // ===============================
        public bool AddHost(string ip, string name = "New Host")
        {
            string query = @"
                IF NOT EXISTS (SELECT 1 FROM Hosts WHERE IPAddress = @IP)
                BEGIN
                    INSERT INTO Hosts (HostName, IPAddress, LastSeen, IsActive)
                    VALUES (@Name, @IP, GETDATE(), 1)
                END";

            using (SqlConnection conn = db.get_connect())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@IP", ip.Trim());

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ===============================
        // 3️⃣ XÓA HOST (XÓA LIÊN QUAN)
        // ===============================
        public bool RemoveHost(string ip)
        {
            string query = @"
                DECLARE @ID INT;
                SELECT @ID = HostID FROM Hosts WHERE IPAddress = @IP;

                IF @ID IS NOT NULL
                BEGIN
                    DELETE FROM Alerts WHERE HostID = @ID;
                    DELETE FROM Host_Stats WHERE HostID = @ID;
                    DELETE FROM Event_Logs WHERE HostID = @ID;
                    DELETE FROM Hosts WHERE HostID = @ID;
                END";

            using (SqlConnection conn = db.get_connect())
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@IP", ip.Trim());

                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}
