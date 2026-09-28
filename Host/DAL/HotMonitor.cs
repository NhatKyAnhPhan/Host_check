using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace DAL
{
    public class HotMonitor
    {
        SqlConnection conn = new SqlConnection("your_connection_string");

        public List<HostInfo> GetAllHosts()
        {
            List<HostInfo> list = new List<HostInfo>();

            string sql = "SELECT * FROM HOST";
            SqlCommand cmd = new SqlCommand(sql, conn);

            conn.Open();
            SqlDataReader rd = cmd.ExecuteReader();
            while (rd.Read())
            {
                list.Add(new HostInfo
                {
                    HostID = (int)rd["HostID"],
                    Name = rd["Name"].ToString(),
                    IP = rd["IP"].ToString(),
                    Description = rd["Description"].ToString(),
                    Status = (bool)rd["Status"],
                    LastCheck = (DateTime)rd["LastCheck"]
                });
            }
            conn.Close();
            return list;
        }
    }
}
