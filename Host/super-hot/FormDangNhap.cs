using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BLL;
using DTO;

namespace super_hot
{
    public partial class FormDangNhap : Form
    {
        private UserBLL bll = new UserBLL();
        public FormDangNhap()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            UserDTO user = bll.Login(txtUser.Text.Trim(), txtPass.Text.Trim());

            if (user == null)
            {
                MessageBox.Show("Sai tài khoản hoặc mật khẩu");
                return;
            }

            MessageBox.Show(
                $"Xin chào {user.FullName}\nQuyền: {user.RoleName}",
                "Đăng nhập thành công");

            // TODO: mở MainForm
            new MainForm(user).Show();
            this.Hide();
        }

        private void linkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormDangKy frm = new FormDangKy();
            frm.Show();
            this.Hide();
        }
    }
}
