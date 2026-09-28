using DAL;

namespace BLL
{
    public class HostServiceDB
    {
        HostDAL dal = new HostDAL();

        public string GetSingleHostIP()
        {
            return dal.GetActiveHostIP();
        }
    }
}
