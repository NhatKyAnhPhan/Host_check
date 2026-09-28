using DAL;
using DTO;

namespace BLL
{
    public class UserBLL
    {
        private UserDAL dal = new UserDAL();

        public UserDTO Login(string username, string password)
        {
            return dal.Login(username, password);
        }

        public bool Register(string username, string password, string fullname, string email)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            return dal.Register(username, password, fullname, email);
        }
    }
}
