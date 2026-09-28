using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace super_hot
{
    public partial class UC_Setting : UserControl
    {
        public UC_Setting()
        {
            InitializeComponent();
            LoadConfig();
            this.btnSave.Click += BtnSave_Click;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            super_hot.Settings.Default.PingInterval = (int)nudPingInterval.Value;
            super_hot.Settings.Default.PingTimeout = (int)nudTimeout.Value;
            super_hot.Settings.Default.CpuWarning = (int)nudCpuWarning.Value;

            Properties.Settings.Default.Save();

            MessageBox.Show(
                "Đã lưu cấu hình thành công!",
                "Cài đặt",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void LoadConfig()
        {
            nudPingInterval.Value = super_hot.Settings.Default.PingInterval;
            nudTimeout.Value = super_hot.Settings.Default.PingTimeout;
            nudCpuWarning.Value = super_hot.Settings.Default.CpuWarning;
        }
    }
}
