using System.Data;
using System.Data.SqlClient;

namespace DAL
{
    public class Connect_DB
    {
        private readonly SqlConnection conn;

        // 🔹 KHỞI TẠO CONNECTION CHUẨN
        public Connect_DB()
        {
            string connStr =
                "Data Source=LAPTOP-I965QD1N;" +
                "Initial Catalog=QL_HOST;" +
                "Integrated Security=True;";

            conn = new SqlConnection(connStr);
        }

        // ✅ DÙNG CHO SqlDataAdapter
        public SqlConnection get_connect()
        {
            return conn;
        }

        // 🔐 OPEN SAFE
        private void open()
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();
        }

        // 🔐 CLOSE SAFE
        private void close()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        // ✅ ExecuteScalar CHUẨN
        public object ExecuteScalar(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    open();
                    return cmd.ExecuteScalar();
                }
            }
            finally
            {
                close();
            }
        }

        // ✅ ExecuteNonQuery (NÊN CÓ)
        public int ExecuteNonQuery(string sql)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    open();
                    return cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                close();
            }
        }

        // ✅ ExecuteReader (OPTIONAL)
        public SqlDataReader ExecuteReader(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, conn);
            open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
    }
}