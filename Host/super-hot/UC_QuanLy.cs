using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BLL;
using DTO;

namespace super_hot
{
    public partial class UC_QuanLy : UserControl
    {
        private readonly HostMonitor bll = new HostMonitor();

        // Tạo Timer cập nhật tự động
        private Timer timerUpdate;
        private bool isUpdating = false; // Cờ để tránh xung đột

        public UC_QuanLy()
        {
            InitializeComponent();
            SetupTimer(); // Cài đặt Timer ngay khi khởi tạo
        }

        // --- CÀI ĐẶT TIMER ---
        private void SetupTimer()
        {
            timerUpdate = new Timer();
            timerUpdate.Interval = 3000; // 3000ms = 3 giây
            timerUpdate.Tick += TimerUpdate_Tick;
        }

        // --- SỰ KIỆN KHI VÀO MÀN HÌNH ---
        private void UC_QuanLy_Load(object sender, EventArgs e)
        {
            LoadDanhSach();     // Load dữ liệu lần đầu
            timerUpdate.Start(); // Bắt đầu đếm giờ
        }

        // --- HÀM LOAD LẦN ĐẦU ---
        public void LoadDanhSach()
        {
            try
            {
                dgvDanhSach.AutoGenerateColumns = false;
                // Lấy danh sách từ DB
                List<HostInfo> listHosts = bll.GetHosts();
                dgvDanhSach.DataSource = listHosts;

                // Chạy cập nhật ngay lập tức 1 lần
                RunUpdateAsync(listHosts);
            }
            catch { }
        }

        // --- SỰ KIỆN TIMER (CHẠY MỖI 3 GIÂY) ---
        private void TimerUpdate_Tick(object sender, EventArgs e)
        {
            // Nếu danh sách đang trống hoặc đang cập nhật dở thì bỏ qua 
            if (dgvDanhSach.DataSource == null || isUpdating) return;

            // Lấy danh sách đang hiện trên bảng
            List<HostInfo> currentList = dgvDanhSach.DataSource as List<HostInfo>;
            if (currentList != null)
            {
                RunUpdateAsync(currentList);
            }
        }

        // --- HÀM CHẠY PING NGẦM ---
        private async void RunUpdateAsync(List<HostInfo> list)
        {
            isUpdating = true; // Đánh dấu đang bận
            try
            {
                await Task.Run(() =>
                {
                    // Dùng Parallel.ForEach để Ping nhiều máy cùng lúc cho nhanh
                    Parallel.ForEach(list, host =>
                    {
                        HostStatus result = bll.PingHost(host.IP);
                        if (result != null)
                        {
                            host.Status = result.Status;
                            host.ResponseTime = result.RoundTripTime;
                        }
                    });
                });

                // Cập nhật giao diện sau khi Ping xong hết
                if (this.IsHandleCreated)
                {
                    this.Invoke(new Action(() =>
                    {
                        dgvDanhSach.Refresh(); // Vẽ lại bảng
                    }));
                }
            }
            catch { }
            finally
            {
                isUpdating = false; // Đánh dấu đã xong
            }
        }

        // --- FORMAT GIAO DIỆN (MÀU SẮC, CHỮ UP/DOWN) ---
        private void dgvDanhSach_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Cột Status
            if (dgvDanhSach.Columns[e.ColumnIndex].DataPropertyName == "Status" && e.Value != null)
            {
                bool status = false;
                if (e.Value is bool) status = (bool)e.Value;
                else if (e.Value.ToString() == "Up") status = true;

                e.Value = status ? "Up" : "Down";
                e.CellStyle.ForeColor = status ? Color.Green : Color.Red;
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Bold);
                e.FormattingApplied = true;
            }

            // Cột ResponseTime
            if (dgvDanhSach.Columns[e.ColumnIndex].DataPropertyName == "ResponseTime" && e.Value != null)
            {
                e.Value = e.Value.ToString() + " ms";
                e.FormattingApplied = true;
            }
        }

        // --- NÚT XÓA ---
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvDanhSach.CurrentRow == null) return;
            try
            {
                HostInfo item = dgvDanhSach.CurrentRow.DataBoundItem as HostInfo;
                if (item != null && MessageBox.Show($"Xóa IP: {item.IP}?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (bll.RemoveHost(item.IP)) LoadDanhSach();
                }
            }
            catch { }
        }

        // --- NÚT REFRESH ---
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadDanhSach();
        }

        // --- KHI THOÁT KHỎI TAB NÀY THÌ DỪNG TIMER ---
        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (timerUpdate != null) timerUpdate.Stop();
            base.OnHandleDestroyed(e);
        }
    }
}