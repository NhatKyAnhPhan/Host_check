using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace BLL
{
    public class PingService
    {
        public class HostCheckResult
        {
            public bool IsSuccess { get; set; }
            public string StatusText { get; set; }
            public HostState State { get; set; }
        }
        public enum HostState
        {
            Online,
            PingBlocked,
            Offline,
            InvalidIP,
            Unknown
        }

        public class HostService
        {
            public HostCheckResult CheckHost(string ip)
            {
                try
                {
                    Ping ping = new Ping();
                    PingReply reply = ping.Send(ip, 3000); // tăng timeout

                    switch (reply.Status)
                    {
                        case IPStatus.Success:
                            return new HostCheckResult
                            {
                                IsSuccess = true,
                                State = HostState.Online,
                                StatusText = "Host đang hoạt động"
                            };

                        case IPStatus.TimedOut:
                            return new HostCheckResult
                            {
                                IsSuccess = false,
                                State = HostState.PingBlocked,
                                StatusText = "Ping timeout (có thể firewall chặn)"
                            };

                        case IPStatus.DestinationHostUnreachable:
                            return new HostCheckResult
                            {
                                IsSuccess = false,
                                State = HostState.Offline,
                                StatusText = "Host không tồn tại hoặc đã tắt"
                            };

                        default:
                            return new HostCheckResult
                            {
                                IsSuccess = false,
                                State = HostState.Unknown,
                                StatusText = "Không xác định trạng thái"
                            };
                    }
                }
                catch (PingException)
                {
                    return new HostCheckResult
                    {
                        IsSuccess = false,
                        State = HostState.PingBlocked,
                        StatusText = "Ping bị chặn"
                    };
                }
                catch
                {
                    return new HostCheckResult
                    {
                        IsSuccess = false,
                        State = HostState.InvalidIP,
                        StatusText = "IP không hợp lệ"
                    };
                }
            }

        }

    }
}
