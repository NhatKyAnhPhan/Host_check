using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class UserDAL
    {
        private Connect_DB db = new Connect_DB();

        // ================= LOGIN =================
        public UserDTO Login(string username, string password)
        {
            using (SqlCommand cmd = new SqlCommand("SP_Login", db.get_connect()))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                db.get_connect().Open();
                SqlDataReader rd = cmd.ExecuteReader();

                if (rd.Read())
                {
                    UserDTO user = new UserDTO
                    {
                        UserID = (int)rd["UserID"],
                        Username = rd["Username"].ToString(),
                        FullName = rd["FullName"].ToString(),
                        RoleName = rd["RoleName"].ToString()
                    };

                    db.get_connect().Close();
                    return user;
                }

                db.get_connect().Close();
                return null;
            }
        }

        // ================= REGISTER =================
        public bool Register(string username, string password, string fullname, string email)
        {
            using (SqlCommand cmd = new SqlCommand("SP_Register", db.get_connect()))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", username);

                // ⚠ HASH SHA256 TRONG SQL → gửi plaintext
                cmd.Parameters.AddWithValue("@Password",
                    password);

                cmd.Parameters.AddWithValue("@FullName", fullname);
                cmd.Parameters.AddWithValue("@Email", email);

                db.get_connect().Open();
                cmd.ExecuteNonQuery();
                db.get_connect().Close();

                return true;
            }
        }
    }
}
