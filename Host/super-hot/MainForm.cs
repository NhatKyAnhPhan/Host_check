using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using BLL;
using System.Windows.Forms.DataVisualization.Charting;
using static BLL.PingService;
using DTO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace super_hot
{
   
    public partial class MainForm : Form
    {
        private UserDTO _currentUser;
        private string _currentHostIP;   // 🔴 BẮT BUỘC PHẢI CÓ
        private FormDangNhap _loginForm;
        private string _currentUsername;
       

        public MainForm()
        {
            this.BackColor = Color.Black;
            InitializeComponent();
            this.Load += MainForm_Load;
            timerCPU.Tick += Timer1_Tick;
            btnCheck.Click += BtnCheck_Click;
            this.thêmHostToolStripMenuItem.Click += ThêmHostToolStripMenuItem_Click;
            this.càiĐặtToolStripMenuItem.Click += CàiĐặtToolStripMenuItem_Click;
            this.quảnLýToolStripMenuItem.Click += QuảnLýToolStripMenuItem_Click;
            this.MaintoolStripMenuItem.Click += MaintoolStripMenuItem_Click;
        }
        public MainForm(UserDTO user) : this()
        {

            _currentUser = user;

            // ví dụ: hiển thị user đăng nhập
            this.Text = $"Super Hot - {user.FullName} ({user.RoleName})";

            // (Tùy chọn) hiển thị user – KHÔNG ẢNH HƯỞNG UI
            this.Text = $"HOST MANAGER - {user.Username}";
            this.logoutToolStripMenuItem.Click += LogoutToolStripMenuItem_Click;
            this.FormClosing += MainForm_FormClosing;
        }

        private void MaintoolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelshow.Controls.Clear();
            panelshow.Visible = false;

            tableLayoutPanel1.Visible = true;
            RefreshDashboard();

        }
        private void RefreshDashboard()
        {
            LoadTotalHost();
            LoadCPUChartFromDB();
            CheckCurrentHost();
        }

        private void CheckCurrentHost()
        {
            if (string.IsNullOrEmpty(_currentHostIP)) return;

            HostService service = new HostService();
            HostCheckResult result = service.CheckHost(_currentHostIP);

            // Hiển thị lên các Label trực tiếp trên MainForm
            lblStatus.Text = result.State.ToString();
            SetHostStatusLabel(result.State);
            ShowHostResult(result);
        }
        private void LogoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rs = MessageBox.Show(
                "Bạn có chắc muốn đăng xuất?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (rs == DialogResult.Yes)
            {
                _loginForm?.Show();
                this.Close();
            }
        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_loginForm != null && !_loginForm.Visible)
            {
                _loginForm.Show();
            }
        }

        private void QuảnLýToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_QuanLy());
        }

        private void CàiĐặtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Setting());
        }

        private void ThêmHostToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_ThemHost());
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            timerCPU.Stop();
            HostService service = new HostService();
            HostCheckResult result = service.CheckHost(lblIP.Text);

            ShowHostResult(result);
            SetHostStatusLabel(result.State);
        }
        private void ShowHostResult(HostCheckResult result)
        {
            lblStatus.Text = result.State.ToString();

            switch (result.State)
            {
                case HostState.Online:
                    lblStatus.ForeColor = Color.LimeGreen;
                    break;

                case HostState.PingBlocked:
                    lblStatus.ForeColor = Color.Orange;
                    ShowNotify("Ping Blocked", result.StatusText, NotifyType.Warning);
                    break;

                case HostState.InvalidIP:
                    lblStatus.ForeColor = Color.Gray;
                    ShowNotify("Invalid IP", result.StatusText, NotifyType.Error);
                    break;

                case HostState.Offline:
                    lblStatus.ForeColor = Color.Red;
                    ShowNotify("Offline", result.StatusText, NotifyType.Critical);
                    break;
            }
        }
        
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (tableLayoutPanel1.Visible)
                LoadCPUChartFromDB();
            if (panelshow.Controls.Count == 0)
            {
                LoadCPUChartFromDB();
            }
        }
      

        private void MainForm_Load(object sender, EventArgs e)
        {

            SetupCPUChart();

            tableLayoutPanel1.Visible = true;
            panelshow.Visible = false;


            HostServiceDB hostDB = new HostServiceDB();
            _currentHostIP = hostDB.GetSingleHostIP();

            if (!string.IsNullOrEmpty(_currentHostIP))
            {
                lblIP.Text = _currentHostIP;
                RefreshDashboard();
            }

            timerCPU.Start();
        }

        private void SetupCPUChart()
        {
            chartCPU.Series.Clear();
            chartCPU.ChartAreas.Clear();
            chartCPU.Legends.Clear();

            // ===== Chart Area =====
            ChartArea area = new ChartArea("MainArea");
            area.BackColor = System.Drawing.Color.FromArgb(18, 18, 18);

            // Grid
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.LineColor = System.Drawing.Color.FromArgb(50, 50, 50);

            // Axis
            area.AxisX.LabelStyle.ForeColor = System.Drawing.Color.Gainsboro;
            area.AxisY.LabelStyle.ForeColor = System.Drawing.Color.Gainsboro;
            area.AxisX.LineColor = System.Drawing.Color.Gray;
            area.AxisY.LineColor = System.Drawing.Color.Gray;

            area.AxisY.Minimum = 0;
            area.AxisY.Maximum = 100;
            area.AxisY.Interval = 20;

            chartCPU.ChartAreas.Add(area);

            // ===== Legend =====
            Legend legend = new Legend();
            legend.ForeColor = System.Drawing.Color.White;   // chữ trắng
            legend.BackColor = System.Drawing.Color.Black;   // nền đen
            legend.Docking = Docking.Top;
            legend.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            chartCPU.Legends.Add(legend);

            // ===== Series =====
            Series series = new Series("CPU Usage");
            series.ChartType = SeriesChartType.Column;
            series.IsValueShownAsLabel = true;
            series.LabelForeColor = System.Drawing.Color.White;
            series.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            series.BorderWidth = 0;

            // Gradient
            series.BackGradientStyle = GradientStyle.VerticalCenter;
            series.BackSecondaryColor = System.Drawing.Color.FromArgb(0, 160, 255);

            chartCPU.Series.Add(series);

            // Chart nền
            chartCPU.BackColor = System.Drawing.Color.FromArgb(25, 25, 25);
        }
        private void LoadCPUChartFromDB()
        {
            HostStats bll = new HostStats();
            DataTable dt = bll.GetLatestCPUByHost();

            chartCPU.Series["CPU Usage"].Points.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string hostName = row["HostName"].ToString();
                double cpu = Convert.ToDouble(row["CPU"]);

                DataPoint p = new DataPoint();
                p.AxisLabel = hostName;
                p.YValues = new double[] { cpu };
                p.ToolTip = $"{hostName}\nCPU: {cpu}%";
                if (cpu >= 85)
                {
                    p.Color = System.Drawing.Color.FromArgb(255, 80, 80);     // đỏ cảnh báo
                    p.BackSecondaryColor = System.Drawing.Color.FromArgb(180, 30, 30);
                }
                else if (cpu >= 65)
                {
                    p.Color = System.Drawing.Color.FromArgb(255, 170, 0);     // cam
                    p.BackSecondaryColor = System.Drawing.Color.FromArgb(180, 120, 0);
                }
                else
                {
                    p.Color = System.Drawing.Color.FromArgb(0, 200, 120);     // xanh
                    p.BackSecondaryColor = System.Drawing.Color.FromArgb(0, 120, 80);
                }

                p.BackGradientStyle = GradientStyle.VerticalCenter;

                chartCPU.Series["CPU Usage"].Points.Add(p);
            }
        }
       
        public enum NotifyType
        {
            Success,
            Info,
            Warning,
            Error,
            Critical
        }
        private void SetHostStatusLabel(HostState state)
        {
            // 1. Định dạng chung cho lblIP
            lblIP.AutoSize = false;
            lblIP.TextAlign = ContentAlignment.MiddleCenter;
            lblIP.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblIP.Padding = new Padding(6);
            lblIP.BorderStyle = BorderStyle.FixedSingle;

            // 2. Định dạng theo từng trạng thái (HostState)
            switch (state)
            {
                case HostState.Online:
                    lblIP.Text = "ONLINE";
                    lblIP.ForeColor = Color.FromArgb(40, 189, 99);
                    break;

                case HostState.PingBlocked:
                    lblIP.Text = "PING BLOCKED";                   
                    lblIP.ForeColor = Color.FromArgb(243, 156, 18); ;
                    break;

                case HostState.Offline:
                    lblIP.Text = "OFFLINE";
                    lblIP.ForeColor = Color.FromArgb(231, 76, 60);
                    break;

                case HostState.InvalidIP:
                    lblIP.Text = "INVALID IP";
                    lblIP.ForeColor = Color.FromArgb(149, 165, 166);
                    break;

                case HostState.Unknown:
                default:
                    lblIP.Text = "UNKNOWN";
                    lblIP.ForeColor = Color.FromArgb(52, 152, 219);
                    break;
            }
        }
        private void ShowNotify(string title, string message, NotifyType type)
        {
            MessageBoxIcon icon;

            switch (type)
            {
                case NotifyType.Success:
                    icon = MessageBoxIcon.Information;
                    break;
                case NotifyType.Warning:
                    icon = MessageBoxIcon.Warning;
                    break;
                case NotifyType.Critical:
                    icon = MessageBoxIcon.Error;
                    break;
                default:
                    icon = MessageBoxIcon.Information;
                    break;
            }

            MessageBox.Show(message, title, MessageBoxButtons.OK, icon);
        }

       
        private void LoadTotalHost()
        {
            TongHost bll = new TongHost();
            lbTongHost.Text = bll.GetTotalHost().ToString();
        }
        private void LoadUserControl(UserControl uc)
        {
            tableLayoutPanel1.Visible = false;
            panelshow.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelshow.Controls.Add(uc); 
            panelshow.Visible = true;
        }
    }
}
