using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BLL;

namespace super_hot
{
    public partial class FormDangKy : Form
    {
        private UserBLL bll = new UserBLL();
        public FormDangKy()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            // ==== GIỮ NGUYÊN CHECK DỮ LIỆU ====
            if (txtUser.Text.Trim() == "" ||
                txtPass.Text.Trim() == "" ||
                txtConfirm.Text.Trim() == "" ||
                txtFullName.Text.Trim() == ""||
                txtEmail.Text.Trim() == "")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin");
                return;
            }

            if (txtPass.Text != txtConfirm.Text)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp");
                return;
            }

            // ==== CHỈ ĐỔI PHẦN SQL → BLL ====
            try
            {
                bool result = bll.Register(
                    txtUser.Text.Trim(),
                    txtPass.Text.Trim(),
                    txtFullName.Text.Trim(),
                    txtEmail.Text.Trim()
                );

                if (result)
                {
                    MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi đăng ký: " + ex.Message);
            }
        }

        private void FormDangKy_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.OpenForms["FormDangNhap"]?.Show();
        }

        private void linkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Application.OpenForms["FormDangNhap"]?.Show();
            this.Close();
        }

    }
}
