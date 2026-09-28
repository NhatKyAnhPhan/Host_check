using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class TongHost
    {
        
        public int GetTotalHosts()
        {
            Connect_DB db = new Connect_DB();
            int total = 0;
            string query = "SELECT COUNT(*) FROM Hosts";

            using (SqlConnection conn = db.get_connect())
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                total = (int)cmd.ExecuteScalar();
            }
            
            return total;
        }
    }
}
