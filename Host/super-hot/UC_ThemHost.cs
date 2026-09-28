using System;
using System.Windows.Forms;
using BLL;

namespace super_hot
{
    public partial class UC_ThemHost : UserControl
    {
        private readonly HostMonitor bll = new HostMonitor();

        public UC_ThemHost()
        {
            InitializeComponent();
            this.btnRandom.Click += BtnRandom_Click;
        }

        private void BtnRandom_Click(object sender, EventArgs e)
        {
            txtHostIP.Text = RandomIP();
        }

        private string RandomIP()
        {
            Random rd = new Random();

            // Random IP nội bộ cho dễ test
            int a = 192;
            int b = 168;
            int c = rd.Next(1, 254);
            int d = rd.Next(1, 254);

            return $"{a}.{b}.{c}.{d}";
        }

        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            // Kiểm tra tên ô nhập txtHostIP
            if (txtHostIP == null)
            {
                MessageBox.Show("Lỗi Design: Tên ô nhập chưa phải là txtHostIP", "Lỗi");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHostIP.Text))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Lấy IP từ ô nhập
                string ip = txtHostIP.Text.Trim();
                // Tự tạo tên Host
                string name = "Host " + ip;

                bool ketQua = bll.AddHost(ip, name);

                if (ketQua)
                {
                    MessageBox.Show("Thêm thành công!", "Thông báo");
                    txtHostIP.Text = "";
                }
                else
                {
                    MessageBox.Show("Thêm thất bại (Có thể IP đã tồn tại).", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }
    }
}