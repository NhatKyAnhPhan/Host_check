using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using BLL;
using System.Runtime.CompilerServices;

namespace super_hot
{
    public partial class Form1 : Form
    {
        private List<HostInfo> hosts = new List<HostInfo>();
        private readonly string jsonFile = "hosts.json";
        private Timer timer = new Timer();  // Forms.Timer an toàn nhất
        private bool isMonitoring = false;
       
        public class HostInfo
        {
            public string HostAddress { get; set; }
            public string Status { get; set; } = "Unknown";
            public long? RTT { get; set; }
            public DateTime? LastCheck { get; set; }
        }

        public Form1()
        {
            InitializeComponent();
            CenterToScreen();
            this.btnStart.Click += BtnStart_Click;
            this.btnStop.Click += BtnStop_Click;
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            isMonitoring = false;
            timer.Stop();
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (hosts.Count == 0) { MessageBox.Show("Thêm host trước đã!"); return; }

            isMonitoring = true;
            timer.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            PingAllHosts();  // Ping ngay lập tức
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadHosts();
            UpdateListView();
            timer.Interval = 5000;
            timer.Tick += (s, ev) => PingAllHosts();
           
        }

        private void PingAllHosts()
        {
            if (!isMonitoring) return;

            Task.Run(async () =>
            {
                var tasks = hosts.Select(PingHostAsync).ToArray();
                await Task.WhenAll(tasks);

                Invoke((MethodInvoker)UpdateListView);
            });
        }

        private async Task PingHostAsync(HostInfo host)
        {
            await Task.Run(() =>
            {
                try
                {
                    var process = new System.Diagnostics.Process();
                    process.StartInfo.FileName = "ping.exe";
                    process.StartInfo.Arguments = $"-n 1 -w 2000 \"{host.HostAddress}\""; // ping 1 lần, timeout 2 giây
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    host.LastCheck = DateTime.Now;

                    // Phân tích kết quả ping.exe
                    if (process.ExitCode == 0 && output.Contains("Reply from"))
                    {
                        // Lấy RTT (thời gian trung bình)
                        var timeIndex = output.IndexOf("time=");
                        if (timeIndex > 0)
                        {
                            var msText = output.Substring(timeIndex + 5);
                            msText = new string(msText.TakeWhile(char.IsDigit).ToArray());
                            if (long.TryParse(msText, out long rtt))
                                host.RTT = rtt;
                            else
                                host.RTT = null;
                        }

                        host.Status = "Up";
                    }
                    else
                    {
                        host.Status = "Down";
                        host.RTT = null;
                    }
                }
                catch
                {
                    host.Status = "Error";
                    host.RTT = null;
                    host.LastCheck = DateTime.Now;
                }
            });
        }

        private void UpdateListView()
        {
            lvHosts.Items.Clear();
            foreach (var h in hosts)
            {
                var item = new ListViewItem(h.HostAddress);
                item.SubItems.Add(h.Status == "Up" ? "✔ Up" : h.Status == "Down" ? "✘ Down" : "⚠ Error");
                item.SubItems.Add(h.RTT?.ToString() ?? "N/A");
                item.SubItems.Add(h.LastCheck?.ToString("HH:mm:ss") ?? "-");

                item.ForeColor = h.Status == "Up" ? Color.Green : Color.Red;

                lvHosts.Items.Add(item);
            }
            SaveHosts();
        }

        private void LoadHosts()
        {
            if (File.Exists(jsonFile))
            {
                try
                {
                    string json = File.ReadAllText(jsonFile);
                    hosts = JsonConvert.DeserializeObject<List<HostInfo>>(json) ?? new List<HostInfo>();
                }
                catch { hosts = new List<HostInfo>(); }
            }
        }

        private void SaveHosts()
        {
            try
            {
                File.WriteAllText(jsonFile, JsonConvert.SerializeObject(hosts, Formatting.Indented));
            }
            catch { }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string addr = txtHost.Text.Trim();
            if (string.IsNullOrEmpty(addr)) { MessageBox.Show("Nhập host đi!"); return; }
            if (hosts.Any(h => h.HostAddress.Equals(addr, StringComparison.OrdinalIgnoreCase))) { MessageBox.Show("Đã có rồi!"); return; }

            hosts.Add(new HostInfo { HostAddress = addr });
            SaveHosts();
            UpdateListView();
            txtHost.Clear();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lvHosts.SelectedItems.Count > 0)
            {
                string addr = lvHosts.SelectedItems[0].Text;
                hosts.RemoveAll(h => h.HostAddress.Equals(addr, StringComparison.OrdinalIgnoreCase));
                SaveHosts();
                UpdateListView();
            }
        }




    }
}