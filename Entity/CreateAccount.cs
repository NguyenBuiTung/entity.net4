using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Entity
{
    public partial class CreateAccount : Form
    {
        private string currentUserRole;
        public CreateAccount(string currentUserRole)
        {
            InitializeComponent();
            this.currentUserRole = currentUserRole;
            LoadRoles();
            LoadData();
        }
        private void LoadRoles()
        {
            // Thêm các mục với Key là giá trị hiển thị và Value là giá trị lưu
            cbRole.Items.Add(new KeyValuePair<string, string>("admin", "admin"));
            cbRole.Items.Add(new KeyValuePair<string, string>("nhân viên", "staff"));

            cbRole.DisplayMember = "Key";  // Hiển thị Key (chuỗi hiển thị)
            cbRole.ValueMember = "Value";  // Lưu Value (giá trị thực tế)

            cbRole.SelectedIndex = 0; // Chọn mục đầu tiên mặc định
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string role = ((KeyValuePair<string, string>)cbRole.SelectedItem).Value;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Vui lòng nhập tài khoản và mật khẩu");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu chưa khớp");
                return;
            }
            // Chỉ cho phép người dùng có vai trò admin tạo tài khoản
            if (currentUserRole != "admin")
            {
                MessageBox.Show("Chỉ có admin mới có quyền tạo tài khoản.");
                return;
            }
            if (CreateEmployeeAccount(username, password, role))
            {

                MessageBox.Show("Thêm tài khoản thành công");
                LoadData();
                //this.Close(); // Đóng form sau khi hoàn thành
            }
            else
            {
                MessageBox.Show("Tạo tài khoản thất bại");
            }
        }

        private bool CreateEmployeeAccount(string username, string password, string role)
        {
            try
            {
                using (var context = new Model1())
                {
                    // Kiểm tra xem tên người dùng đã tồn tại chưa
                    if (context.Accounts.Any(emp => emp.Username == username))
                    {
                        MessageBox.Show("Username already exists.");
                        return false;
                    }

                    // Hash mật khẩu trước khi lưu
                    string hashedPassword = HashPassword(password);

                    // Tạo đối tượng Account mới
                    var newAccount = new Account
                    {
                        Username = username,
                        Password = hashedPassword, // Lưu mật khẩu đã được mã hóa
                        Role = role,
                        CreatedDate = DateTime.Now // Thêm CreatedDate
                    };

                    // Thêm vào cơ sở dữ liệu và lưu thay đổi
                    context.Accounts.Add(newAccount);
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu xảy ra
                MessageBox.Show($"An error occurred: {ex.Message}");
                return false;
            }
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder result = new StringBuilder();
                foreach (byte b in bytes)
                    result.Append(b.ToString("x2"));
                return result.ToString();
            }
        }
        void LoadData()
        {
            using (var context = new Model1())
            {
                // Trả về danh sách tất cả tài khoản
                var result = context.Accounts.ToList();
                listAccount.Columns.Clear();
                listAccount.AutoGenerateColumns = false;
                listAccount.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Username",
                    HeaderText = "Tài khoản"
                });

                listAccount.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Role",
                    HeaderText = "Vai trò"
                });
                listAccount.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "CreatedDate",
                    HeaderText = "Ngày tạo"
                });
                listAccount.DataSource = result;
            }

        }
        private void loadAccount_Click(object sender, EventArgs e)
        {
            LoadData();

        }
        private void DeleteAccount()
        {
            if (listAccount.SelectedRows.Count > 0)
            {
                string username = listAccount.SelectedRows[0].Cells[0].Value.ToString();

                if (currentUserRole != "admin")
                {
                    MessageBox.Show("Chỉ có admin mới có quyền xóa tài khoản.");
                    return;
                }

                var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản '{username}'?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (var context = new Model1())
                        {
                            var accountToDelete = context.Accounts.FirstOrDefault(a => a.Username == username);
                            if (accountToDelete != null)
                            {
                                context.Accounts.Remove(accountToDelete);
                                context.SaveChanges();
                                MessageBox.Show("Xóa tài khoản thành công.");
                                LoadData(); // Tải lại dữ liệu sau khi xóa
                            }
                            else
                            {
                                MessageBox.Show("Không tìm thấy tài khoản để xóa.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi xóa tài khoản: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn tài khoản để xóa.");
            }
        }
        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            DeleteAccount();
        }
    }
}
