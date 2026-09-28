using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class HostInfo
    {
        public int HostID { get; set; }
        public string Name { get; set; }
        public string IP { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; }          // true = Online, false = Offline
        public DateTime LastCheck { get; set; }

        // Tài nguyên hiện tại
        public float CPU { get; set; }
        public float RAM { get; set; }
        public float DISK { get; set; }

        // Dùng khi ping
        public long ResponseTime { get; set; }

        public HostInfo() { }

        public HostInfo(int hostID, string name, string ip, string description, bool status,
                        DateTime lastCheck, float cpu, float ram, float disk, long responseTime)
        {
            HostID = hostID;
            Name = name;
            IP = ip;
            Description = description;
            Status = status;
            LastCheck = lastCheck;
            CPU = cpu;
            RAM = ram;
            DISK = disk;
            ResponseTime = responseTime;
        }
    }

}

