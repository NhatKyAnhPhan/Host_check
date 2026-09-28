using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using DTO;

namespace BLL
{
    public class HostMonitor
    {
        private readonly PingService pingService = new PingService();
        private HostRepo repo = new HostRepo();

        public List<HostInfo> GetHosts()
        {
            return repo.GetAllHosts();
        }

        public bool AddHost(string ip, string name = "New Host")
        {
            return repo.AddHost(ip, name);
        }

        public bool RemoveHost(string ip)
        {
            return repo.RemoveHost(ip);
        }

        // --- HÀM PING DÙNG STOPWATCH (Bất tử, không sợ lỗi thư viện) ---
        public HostStatus PingHost(string ipAddress)
        {
            HostStatus result = new HostStatus();

            // Tạo đồng hồ bấm giờ
            Stopwatch sw = new Stopwatch();

            try
            {
                using (var ping = new global::System.Net.NetworkInformation.Ping())
                {
                    // 1. Bắt đầu bấm giờ
                    sw.Start();

                    // 2. Gửi Ping
                    var reply = ping.Send(ipAddress, 1000);

                    // 3. Dừng bấm giờ ngay lập tức
                    sw.Stop();

                    if (reply != null && reply.Status == global::System.Net.NetworkInformation.IPStatus.Success)
                    {
                        result.Status = true;
                        // Lấy thời gian từ đồng hồ của mình (ElapsedMilliseconds)
                        result.RoundTripTime = sw.ElapsedMilliseconds;
                    }
                    else
                    {
                        result.Status = false;
                        result.RoundTripTime = 0;
                    }
                }
            }
            catch
            {
                sw.Stop(); // Đảm bảo đồng hồ dừng nếu lỗi
                result.Status = false;
                result.RoundTripTime = 0;
            }
            return result;
        }

    }
}
