using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using ZXing.Common;
using ZXing;

namespace Entity
{
    public partial class BarCodeScan : Form
    {
        private Account _loggedInAccount;
        public BarCodeScan(Account loggedInAccount)
        {
            InitializeComponent();
            _loggedInAccount = loggedInAccount;
            //lblUser.Text = $"Xin chào, {_loggedInAccount.Username}";
            lbLoadHistory.Visible = false;
            progressBarExport.Visible = false;
            lblProgressPercentage.Visible = false;
            LoadRoles();
            LoadData();
            LoadProducts();
            LoadHistoryScan();
            AddBindingProducts();
            SetupListView();

        }
        // Quản lý tài khoản
        private void LoadRoles()
        {
            // Thêm các mục với Key là giá trị hiển thị và Value là giá trị lưu
            cbRole.Items.Add(new KeyValuePair<string, string>("quản trị", "quản trị"));
            cbRole.Items.Add(new KeyValuePair<string, string>("nhân viên", "nhân viên"));

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
            if (_loggedInAccount.Role != "admin")
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
                        MessageBox.Show("Tên tài khoản đã tồn tại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        private void loadAccount_Click_1(object sender, EventArgs e)
        {
            LoadData();
        }
        private void DeleteAccount()
        {
            if (listAccount.SelectedRows.Count > 0)
            {
                string username = listAccount.SelectedRows[0].Cells[0].Value.ToString();
                string role = _loggedInAccount.Role;
                if (role != "admin")
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

        private void btnDeleteAccount_Click_1(object sender, EventArgs e)
        {
            DeleteAccount();
        }
        //Quản lý sản phẩm 
        void AddBindingProducts()
        {
            txtIdProduct.DataBindings.Clear();
            txtProductName.DataBindings.Clear();
            txtProductModel.DataBindings.Clear();
            txtIdProduct.DataBindings.Add(new Binding("Text", dataGridViewProducts.DataSource, "Id", true, DataSourceUpdateMode.Never));
            txtProductName.DataBindings.Add(new Binding("Text", dataGridViewProducts.DataSource, "Name", true, DataSourceUpdateMode.Never));
            txtProductModel.DataBindings.Add(new Binding("Text", dataGridViewProducts.DataSource, "Model", true, DataSourceUpdateMode.Never));
        }
        private void LoadProducts()
        {
            using (var context = new Model1())
            {
                var products = context.Products.ToList();
                cmbProductModel.DataSource = products;  // Gán danh sách sản phẩm vào ComboBox
                cmbProductModel.DisplayMember = "Model";  // Hiển thị tên model của sản phẩm
                cmbProductModel.ValueMember = "Id";
                dataGridViewProducts.Columns.Clear();
                dataGridViewProducts.AutoGenerateColumns = false;
                // Thêm cột Id vào DataGridView
                dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Id", // Thuộc tính từ Product entity
                    HeaderText = "Product ID", // Tên hiển thị trong DataGridView
                    Name = "Id",
                    Visible = true // Đảm bảo cột này hiển thị
                });
                dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Name",
                    HeaderText = "Tên sản phẩm"
                });

                dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "Model",
                    HeaderText = "Model"
                });
                dataGridViewProducts.Columns.Add(new DataGridViewTextBoxColumn
                {
                    DataPropertyName = "CreatedDate",
                    HeaderText = "Ngày tạo"
                });
                dataGridViewProducts.DataSource = products;
                AddBindingProducts();
            }

        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            using (var context = new Model1())
            {
                string productName = txtProductName.Text.Trim();
                string productModel = txtProductModel.Text.Trim();

                // Kiểm tra tên sản phẩm đã tồn tại
                bool isProductNameExists = context.Products.Any(p => p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

                // Kiểm tra model sản phẩm đã tồn tại
                bool isProductModelExists = context.Products.Any(p => p.Model.Equals(productModel, StringComparison.OrdinalIgnoreCase));

                if (isProductNameExists)
                {
                    MessageBox.Show("Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                    return;
                }

                if (isProductModelExists)
                {
                    MessageBox.Show("Model sản phẩm đã tồn tại. Vui lòng chọn model khác.");
                    return;
                }

                // Nếu tên và model sản phẩm không tồn tại, thêm sản phẩm mới
                var newProduct = new Product
                {
                    Name = productName,
                    Model = productModel,
                    CreatedDate = DateTime.Now
                };

                context.Products.Add(newProduct);
                context.SaveChanges();
                LoadProducts();  // Refresh lại danh sách sản phẩm

                MessageBox.Show("Sản phẩm đã được thêm.");
            }
        }
        private void btnEditProduct_Click(object sender, EventArgs e)
        {
            using (var context = new Model1())
            {
                if (dataGridViewProducts.SelectedRows.Count > 0)
                {
                    int id = Convert.ToInt32(dataGridViewProducts.SelectedCells[0].OwningRow.Cells["Id"].Value.ToString());
                    var product = context.Products.Find(id);

                    if (product != null)
                    {
                        string newName = txtProductName.Text.Trim();
                        string newModel = txtProductModel.Text.Trim();

                        // Kiểm tra tên sản phẩm đã tồn tại (ngoại trừ sản phẩm hiện tại)
                        bool isProductNameExists = context.Products
                            .Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase) && p.Id != id);

                        // Kiểm tra model sản phẩm đã tồn tại (ngoại trừ sản phẩm hiện tại)
                        bool isProductModelExists = context.Products
                            .Any(p => p.Model.Equals(newModel, StringComparison.OrdinalIgnoreCase) && p.Id != id);

                        if (isProductNameExists)
                        {
                            MessageBox.Show("Tên sản phẩm đã tồn tại. Vui lòng chọn tên khác.");
                            return;
                        }

                        if (isProductModelExists)
                        {
                            MessageBox.Show("Model sản phẩm đã tồn tại. Vui lòng chọn model khác.");
                            return;
                        }

                        // Cập nhật sản phẩm
                        product.Name = newName;
                        product.Model = newModel;

                        context.SaveChanges();
                        LoadProducts();  // Refresh lại danh sách sản phẩm

                        MessageBox.Show("Sản phẩm đã được cập nhật.");
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm để sửa.");
                }
            }
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            using (var context = new Model1())
            {
                if (dataGridViewProducts.SelectedRows.Count > 0)
                {
                    int productId = (int)dataGridViewProducts.SelectedRows[0].Cells["Id"].Value;
                    var product = context.Products.Find(productId);
                    if (product != null)
                    {
                        context.Products.Remove(product);
                        context.SaveChanges();
                        LoadProducts();  // Refresh lại danh sách sản phẩm
                        MessageBox.Show("Sản phẩm đã được xóa.");
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn sản phẩm để xóa.");
                }
            }
        }

        private void btnDow_Click(object sender, EventArgs e)
        {
            LoadProducts();
        }
        private string GetBarcodeFromScanner()
        {
            return "23243-312339852";  // Mã vạch giả lập
        }
        private void SetupListView()
        {
            listViewBarcodes.View = View.Details;
            listViewBarcodes.FullRowSelect = true;

            // Thêm các cột vào ListView
            listViewBarcodes.Columns.Add("Barcode", 250); // Cột hiển thị mã barcode
            listViewBarcodes.Columns.Add("Status", 100);  // Cột hiển thị trạng thái OK/NG
        }

        private void AddBarcodeToListView(string barcode, string status)
        {
            // Tạo một item mới cho ListView
            var listViewItem = new ListViewItem(barcode);
            listViewItem.SubItems.Add(status); // Thêm trạng thái vào cột thứ 2

            // Thêm item vào vị trí đầu tiên của ListView
            listViewBarcodes.Items.Insert(0, listViewItem);
        }
        private int totalOK = 0;
        private int totalNG = 0;

        private void btnScan_Click(object sender, EventArgs e)
        {
            using (var context = new Model1())
            {
                string scannedCode = GetBarcodeFromScanner();  // Lấy mã từ thiết bị đọc
                btnViewBarCode.Text = scannedCode;

                // Lấy sản phẩm được chọn từ ComboBox
                var selectedProduct = cmbProductModel.SelectedItem as Product;

                if (selectedProduct != null)
                {
                    // Kiểm tra mã vạch đã tồn tại với sản phẩm hiện tại hay chưa
                    bool isDuplicate = context.BarcodeScans.Any(b => b.Barcode == scannedCode && b.ProductId == selectedProduct.Id);

                    if (isDuplicate)
                    {
                        // Nếu mã vạch đã tồn tại, hiển thị trạng thái NG và không lưu vào DB
                        GenerateBarcodeImage(scannedCode, "NG");  // Tạo và hiển thị ảnh mã vạch với trạng thái NG
                        AddBarcodeToListView(scannedCode, "NG");  // Hiển thị mã vạch vào ListView với trạng thái NG

                        // Cập nhật tổng số lượng NG
                        totalNG++;
                        UpdateStatusLabels();
                    }
                    else
                    {
                        // Nếu mã vạch chưa tồn tại, lưu mã vạch mới vào cơ sở dữ liệu
                        BarcodeScan barcodeScan = new BarcodeScan
                        {
                            Barcode = scannedCode,
                            ScanTime = DateTime.Now,
                            AccountId = _loggedInAccount.Id,
                            ProductId = selectedProduct.Id  // Liên kết với sản phẩm đã chọn
                        };

                        context.BarcodeScans.Add(barcodeScan);
                        context.SaveChanges();

                        // Hiển thị mã vạch và trạng thái OK vào ListView
                        AddBarcodeToListView(scannedCode, "OK");
                        GenerateBarcodeImage(scannedCode, "OK");

                        // Cập nhật tổng số lượng OK
                        totalOK++;
                        UpdateStatusLabels();
                    }
                }
            }
        }

        private void UpdateStatusLabels()
        {
            // Giả sử bạn có các label lbTotalOK và lbTotalNG để hiển thị số lượng
            btnOK.Text = $"{totalOK}";
            btnNG.Text = $"{totalNG}";
            btnTotall.Text = $"{totalOK + totalNG}";
        }


        private void GenerateBarcodeImage(string barcodeText, string status)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128, // Loại mã vạch
            };

            // Tạo mã vạch dưới dạng hình ảnh
            using (var bitmap = barcodeWriter.Write(barcodeText))
            {
                int padding = 20; // Khoảng cách padding cho tất cả các phía
                int textHeight = 10; // Chiều cao cho phần văn bản

                // Tạo một bản sao của hình ảnh với thêm không gian padding
                using (var bitmapWithPadding = new Bitmap(bitmap.Width + padding * 2, bitmap.Height + textHeight + padding * 2))
                {
                    using (Graphics g = Graphics.FromImage(bitmapWithPadding))
                    {
                        g.Clear(Color.Green); // Màu nền cho padding (có thể tùy chỉnh)

                        // Vẽ mã vạch vào vị trí giữa, có khoảng cách padding xung quanh
                        g.DrawImage(bitmap, new Rectangle(padding, padding, bitmap.Width, bitmap.Height));

                        // Thiết lập font và màu cho trạng thái
                        Font font = new Font("Times New Roman", 12, FontStyle.Bold);
                        Brush brush = status == "OK" ? Brushes.White : Brushes.Red;

                        //// Tính toán kích thước của văn bản để căn giữa
                        //SizeF textSize = g.MeasureString($"Status: {status}", font);
                        //float textX = (bitmapWithPadding.Width - textSize.Width) / 2; // Vị trí X để căn giữa
                        lbStatus.Text = status;
                        // Vẽ trạng thái (OK/NG) vào vị trí dưới cùng, căn giữa theo chiều ngang
                        //g.DrawString($"Status: {status}", font, brush, new PointF(textX, bitmap.Height + padding + 5));
                    }

                    // Hiển thị hình ảnh có thêm trạng thái và padding trực tiếp trong PictureBox
                    pictureBoxBarcode.Image?.Dispose(); // Giải phóng hình ảnh cũ
                    pictureBoxBarcode.Image = new Bitmap(bitmapWithPadding);
                }
            }
        }

        //Phân trang lịch sử 

        private int _currentPage = 1;  // Trang hiện tại
        private int _recordsPerPage = 100;  // Số bản ghi trên mỗi trang
        private int _totalPages = 1;  // Tổng số trang

        //Lịch sử quét
        private void LoadHistoryScan()
        {
            // Show loading label
            lbLoadHistory.Visible = true;

            // Disable the DataGridView to prevent user interaction while loading
            dtgvHistoryScan.Enabled = false;

            Task.Run(() =>
            {
                using (var context = new Model1())
                {
                    // Lấy khoảng thời gian từ DateTimePickers
                    DateTime fromDate = dtpFromDate.Value.Date;
                    DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddTicks(-1); // Đặt toDate là cuối ngày

                    // Lọc dữ liệu theo khoảng thời gian và tài khoản hiện tại
                    var scansQuery = context.BarcodeScans
                                            .Where(scan => scan.ScanTime >= fromDate && scan.ScanTime <= toDate);

                    // Nếu tài khoản không phải là admin, chỉ lấy các mã quét của tài khoản hiện tại
                    if (_loggedInAccount.Role != "admin")
                    {
                        scansQuery = scansQuery.Where(scan => scan.AccountId == _loggedInAccount.Id);
                    }

                    // Tính tổng số trang
                    int totalRecords = scansQuery.Count();
                    _totalPages = (int)Math.Ceiling((double)totalRecords / _recordsPerPage);

                    // Lấy dữ liệu cho trang hiện tại và bao gồm thông tin tài khoản và sản phẩm
                    var scans = scansQuery
                                .OrderBy(scan => scan.ScanTime)
                                .Skip((_currentPage - 1) * _recordsPerPage)
                                .Take(_recordsPerPage)
                                .Select(scan => new
                                {
                                    scan.Id,
                                    scan.Barcode,
                                    scan.ScanTime,
                                    AccountUsername = context.Accounts
                                        .Where(a => a.Id == scan.AccountId)
                                        .Select(a => a.Username)
                                        .FirstOrDefault(),
                                    ProductName = context.Products
                                        .Where(p => p.Id == scan.ProductId)
                                        .Select(p => p.Name)
                                        .FirstOrDefault()
                                })
                                .ToList();

                    // Ngăn DataGridView tự động tạo cột
                    dtgvHistoryScan.Invoke(new Action(() =>
                    {
                        dtgvHistoryScan.AutoGenerateColumns = false;

                        // Xóa các cột cũ trước khi thêm cột mới
                        dtgvHistoryScan.Columns.Clear();

                        // Thêm cột mã vạch
                        dtgvHistoryScan.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            DataPropertyName = "Barcode",
                            HeaderText = "Mã vạch"
                        });

                        // Thêm cột thời gian quét
                        dtgvHistoryScan.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            DataPropertyName = "ScanTime",
                            HeaderText = "Thời gian quét"
                        });

                        // Kiểm tra nếu người dùng hiện tại là admin thì hiển thị cột "Người quét"
                        if (_loggedInAccount.Role == "admin")
                        {
                            dtgvHistoryScan.Columns.Add(new DataGridViewTextBoxColumn
                            {
                                DataPropertyName = "AccountUsername",
                                HeaderText = "Người quét"
                            });
                        }

                        // Thêm cột tên sản phẩm
                        dtgvHistoryScan.Columns.Add(new DataGridViewTextBoxColumn
                        {
                            DataPropertyName = "ProductName",
                            HeaderText = "Tên sản phẩm"
                        });

                        // Thêm dữ liệu đã lọc vào DataGridView
                        dtgvHistoryScan.DataSource = scans;

                        // Cập nhật thông tin phân trang
                        lblPageInfo.Text = $"Trang {_currentPage}/{_totalPages}";

                        // Enable DataGridView after loading is complete
                        dtgvHistoryScan.Enabled = true;
                    }));
                }

                // Hide loading label after data is loaded
                lbLoadHistory.Invoke(new Action(() => lbLoadHistory.Visible = false));
            });
        }

        private void btnViewHistory_Click(object sender, EventArgs e)
        {
            // Đảm bảo rằng ngày bắt đầu không lớn hơn ngày kết thúc (cho phép cùng một ngày)
            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // Không cho phép xuất nếu thời gian không hợp lệ
            }

            LoadHistoryScan();
        }
        private void btnPreviousPage_Click(object sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadHistoryScan();  // Tải lại dữ liệu với trang hiện tại
            }
        }

        private void btnNextPage_Click(object sender, EventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadHistoryScan();  // Tải lại dữ liệu với trang hiện tại
            }
        }


        //Đăng xuất
        private void Close_Click(object sender, EventArgs e)
        {
            // Hiển thị hộp thoại xác nhận
            var result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất không?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Đóng form hiện tại và trở về màn hình đăng nhập
                this.Hide();  // Ẩn form chính
                Login loginForm = new Login();  // Giả sử bạn có một form đăng nhập tên là LoginForm
                loginForm.Show();  // Hiển thị lại form đăng nhập
            }
        }
        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            // Đảm bảo rằng ngày bắt đầu không lớn hơn ngày kết thúc
            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show("Ngày bắt đầu không được lớn hơn ngày kết thúc.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.Title = "Save Excel File";
                saveFileDialog.FileName = $"HistoryScan_{dtpFromDate.Value:yyyy-MM-dd}_{dtpToDate.Value:yyyy-MM-dd}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var fileInfo = new System.IO.FileInfo(saveFileDialog.FileName);

                    // Hiển thị ProgressBar và thiết lập giá trị ban đầu
                    progressBarExport.Visible = true;
                    progressBarExport.Value = 0;
                    lblProgressPercentage.Visible = true;
                    lblProgressPercentage.Text = "0%";

                    await Task.Run(() =>
                    {
                        using (var package = new OfficeOpenXml.ExcelPackage(fileInfo))
                        {
                            var worksheet = package.Workbook.Worksheets.Add("History Scan");

                            worksheet.Cells[1, 1].Value = "Mã vạch";
                            worksheet.Cells[1, 2].Value = "Thời gian quét";
                            worksheet.Cells[1, 3].Value = "Tên sản phẩm";

                            if (_loggedInAccount.Role == "admin")
                            {
                                worksheet.Cells[1, 4].Value = "Người quét";
                            }

                            using (var range = worksheet.Cells[1, 1, 1, _loggedInAccount.Role == "admin" ? 4 : 3])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Font.Size = 16;
                                range.Style.Font.Name = "Times New Roman";
                                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }

                            int batchSize = 1000;
                            int row = 2;

                            using (var context = new Model1())
                            {
                                DateTime fromDate = dtpFromDate.Value.Date;
                                DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddTicks(-1);

                                var query = context.BarcodeScans
                                                    .Include(scan => scan.Account)
                                                    .Include(scan => scan.Product)
                                                    .Where(scan => scan.ScanTime >= fromDate && scan.ScanTime <= toDate);

                                if (_loggedInAccount.Role != "admin")
                                {
                                    query = query.Where(scan => scan.AccountId == _loggedInAccount.Id);
                                }

                                int totalRecords = query.Count();
                                int totalBatches = (int)Math.Ceiling((double)totalRecords / batchSize);
                                int currentBatch = 0;

                                do
                                {
                                    var scans = query.OrderBy(scan => scan.ScanTime)
                                                     .Skip(currentBatch * batchSize)
                                                     .Take(batchSize)
                                                     .ToList();

                                    if (scans.Count == 0)
                                        break;

                                    foreach (var scan in scans)
                                    {
                                        worksheet.Cells[row, 1].Value = scan.Barcode;
                                        worksheet.Cells[row, 2].Value = scan.ScanTime.ToString("yyyy-MM-dd HH:mm:ss");
                                        worksheet.Cells[row, 3].Value = scan.Product?.Name;

                                        if (_loggedInAccount.Role == "admin")
                                        {
                                            worksheet.Cells[row, 4].Value = scan.Account?.Username;
                                        }
                                        row++;
                                    }

                                    currentBatch++;

                                    // Cập nhật ProgressBar và % hoàn thành
                                    int progressPercentage = (int)((currentBatch / (double)totalBatches) * 100);

                                    // Thực hiện cập nhật trên UI thread
                                    this.Invoke(new Action(() =>
                                    {
                                        progressBarExport.Value = progressPercentage;
                                        lblProgressPercentage.Text = $"{progressPercentage}%"; // Hiển thị phần trăm hoàn thành
                                    }));

                                } while (currentBatch < totalBatches);
                            }

                            using (var range = worksheet.Cells[2, 1, row - 1, _loggedInAccount.Role == "admin" ? 4 : 3])
                            {
                                range.Style.Font.Size = 14;
                                range.Style.Font.Name = "Times New Roman";
                                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Left;
                                range.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                            }

                            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                            package.Save();
                        }
                    });

                    // Ẩn ProgressBar và Label khi hoàn tất
                    progressBarExport.Visible = false;
                    lblProgressPercentage.Visible = false;

                    // Hỏi người dùng có muốn mở file sau khi xuất không
                    var result = MessageBox.Show("Bạn có muốn mở file đã lưu không?", "Mở file", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(fileInfo.FullName);
                    }
                }
            }
        }

       
    }
}
