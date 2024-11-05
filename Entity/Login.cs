using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Entity
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            LoadUserCredentials();
            progressBarLogin.Visible = false;
            lbconnect.Visible = false;
        }

        private void LoadUserCredentials()
        {
            // Kiểm tra nếu tài khoản admin chưa tồn tại, thì tạo tài khoản mặc định
            using (var context = new Model1())
            {
                var adminAccount = context.Accounts.FirstOrDefault(a => a.Username == "admin");
                if (adminAccount == null)
                {
                    var defaultAdmin = new Account
                    {
                        Username = "admin",
                        Password = HashPassword("123456"),
                        Role = "admin",
                        CreatedDate = DateTime.Now // Thêm CreatedDate// Hash mật khẩu trước khi lưu
                    };
                    context.Accounts.Add(defaultAdmin);
                    context.SaveChanges();
                }
            }

            // Kiểm tra nếu tài khoản và mật khẩu đã được lưu, thì hiển thị lại
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Username) &&
                !string.IsNullOrEmpty(Properties.Settings.Default.Password))
            {
                txtUsername.Text = Properties.Settings.Default.Username;
                txtPassword.Text = Properties.Settings.Default.Password;
                chkRememberMe.Checked = true; // Đánh dấu checkbox
            }
        }


        private async void btnLogin_Click_1(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            // Hiển thị ProgressBar trước khi bắt đầu đăng nhập
            progressBarLogin.Visible = true;
            lbconnect.Visible = true;
            progressBarLogin.Style = ProgressBarStyle.Marquee; // Tạo hiệu ứng đang tải
            progressBarLogin.MarqueeAnimationSpeed = 50; // Tốc độ animation

            // Gọi phương thức để xác thực người dùng và lấy đối tượng Account
            var userAccount = await Task.Run(() => AuthenticateUser(username, password));

            // Tắt ProgressBar sau khi hoàn tất
            progressBarLogin.Visible = false;
            lbconnect.Visible = false;

            if (userAccount != null)
            {
                // Nếu người dùng chọn nhớ tài khoản
                if (chkRememberMe.Checked)
                {
                    SaveUserCredentials(username, password);
                }
                else
                {
                    ClearUserCredentials();
                }

                // Mở form chính và truyền đối tượng Account
                BarCodeScan mainForm = new BarCodeScan(userAccount);
                this.Hide();
                mainForm.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Tài khoản hoặc mật khẩu không chính xác");
            }
        }


        private Account AuthenticateUser(string username, string password)
        {
            using (var context = new Model1())
            {
                // Hash mật khẩu trước khi so sánh
                string hashedPassword = HashPassword(password);

                // Kiểm tra xem người dùng có tồn tại và mật khẩu có khớp không
                var user = context.Accounts
                    .FirstOrDefault(emp => emp.Username == username && emp.Password == hashedPassword);

                if (user != null)
                {
                    // Trả về đối tượng Account nếu tìm thấy
                    return user;
                }

                // Trả về null nếu không tìm thấy người dùng
                return null;
            }
        }


        private void SaveUserCredentials(string username, string password)
        {
            // Lưu thông tin tài khoản và mật khẩu vào Settings
            Properties.Settings.Default.Username = username;
            Properties.Settings.Default.Password = password;
            Properties.Settings.Default.Save(); // Lưu thay đổi
        }

        private void ClearUserCredentials()
        {
            // Xóa thông tin tài khoản và mật khẩu
            Properties.Settings.Default.Username = string.Empty;
            Properties.Settings.Default.Password = string.Empty;
            Properties.Settings.Default.Save(); // Lưu thay đổi
        }

        private string HashPassword(string password)
        {
            // Convert the password to a byte array and hash it
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder result = new StringBuilder();
                foreach (byte b in bytes)
                    result.Append(b.ToString("x2"));
                return result.ToString();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
