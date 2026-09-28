using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class CheckTcpPort
    {
        public bool CheckPort(string ip, int port, int timeout = 1500)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(ip, port, null, null);
                    return result.AsyncWaitHandle.WaitOne(timeout);
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
