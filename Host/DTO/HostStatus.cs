using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class HostStatus
    {
        // --- CHÍNH ---
        public bool Status { get; set; }
        public long RoundTripTime { get; set; }

        // --- PHỤ (Alias - Sửa lỗi "does not contain definition") ---
        // Nếu code cũ gọi IsOnline, nó sẽ tự động lấy giá trị của Status
        public bool IsOnline
        {
            get { return Status; }
            set { Status = value; }
        }

        // Nếu code cũ gọi ResponseTime, nó sẽ tự động lấy giá trị của RoundTripTime
        public long ResponseTime
        {
            get { return RoundTripTime; }
            set { RoundTripTime = value; }
        }

        public HostStatus()
        {
            Status = false;
            RoundTripTime = 0;
        }
    }
}
