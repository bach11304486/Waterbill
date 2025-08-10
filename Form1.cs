using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;


namespace SE08204_ElectricBill
{
    public partial class Form1 : Form
    {
        private readonly List<Customer> customerList = new List<Customer>();
        private int selectedIndex = -1;
        private List<Customer> customers = new List<Customer>();

        public Form1()
        {
            InitializeComponent();
            DisplayOnCustomerList();
        }

        private void btelectricitybill_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(tbName.Text) ||
                    string.IsNullOrWhiteSpace(tbId.Text) ||
                    cbbCustomertype.SelectedItem == null ||
                    !int.TryParse(tbNumberlast.Text, out int lastMonthAmount) ||
                    !int.TryParse(tbthismonthnumber.Text, out int thisMonthAmount))
                {
                    MessageBox.Show("Please fill in all fields correctly.", "Input Error");
                    return;
                }

                // Create and add customer
                string name = tbName.Text;
                string id = tbId.Text;
                string typeCustomer = cbbCustomertype.SelectedItem.ToString();

                var customer = new Customer(name, id, typeCustomer, lastMonthAmount, thisMonthAmount);
                customerList.Add(customer);

                tbAmountpaid.Text = customer.AmountPaid.ToString();
                DisplayOnCustomerList();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error");
            }
        }

        private void ClearInputs()
        {
            tbName.Clear();
            tbId.Clear();
            tbNumberlast.Clear();
            tbthismonthnumber.Clear();
            tbAmountpaid.Clear();
            cbbCustomertype.SelectedIndex = -1;
        }

        private void DisplayOnCustomerList()
        {
            listView1.Items.Clear();
            foreach (var customer in customerList)
            {
                ListViewItem row = new ListViewItem(customer.Id);
                row.SubItems.Add(customer.FullName);
                row.SubItems.Add(customer.TypeCustomer);
                row.SubItems.Add(customer.LastMonthAmount.ToString());
                row.SubItems.Add(customer.ThisMonthAmount.ToString());
                row.SubItems.Add(customer.CalculateElectricityBill().ToString());
                row.SubItems.Add(customer.EnvironmentalFee.ToString("F0"));
                row.SubItems.Add(customer.VatFee.ToString("F0"));
                row.SubItems.Add(customer.AmountPaid.ToString());
                listView1.Items.Add(row);
            }
        }

        private void ListView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                selectedIndex = listView1.SelectedIndices[0];
                var customer = customerList[selectedIndex];
                tbName.Text = customer.FullName;
                tbId.Text = customer.Id;
                cbbCustomertype.SelectedItem = customer.TypeCustomer;
                tbNumberlast.Text = customer.LastMonthAmount.ToString();
                tbthismonthnumber.Text = customer.ThisMonthAmount.ToString();
                tbAmountpaid.Text = customer.AmountPaid.ToString();
                cbCustomerType.SelectedItem = customer.TypeCustomer;

            }
        }

        private void btSaveInformation_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog.Title = "Save Customer Information";
                    saveFileDialog.FileName = "CustomerData.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                        {
                            writer.WriteLine("Id,Name,Customer Type,Last Month,This Month,Base Bill,Env Fee,VAT,Total");
                            foreach (var customer in customerList)
                            {
                                writer.WriteLine($"{customer.Id},{customer.FullName},{customer.TypeCustomer},{customer.LastMonthAmount},{customer.ThisMonthAmount},{customer.CalculateElectricityBill()},{customer.EnvironmentalFee:F0},{customer.VatFee:F0},{customer.AmountPaid}");
                            }
                        }
                        MessageBox.Show("Customer information saved successfully!", "Success");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving: {ex.Message}", "Error");
            }
        }

        // Search
        private void btSearch_Click(object sender, EventArgs e)
        {
            // Lấy từ khóa từ TextBox nhập tìm kiếm
            string keyword = btnSearch.Text.Trim().ToLower();

            // Lấy loại khách hàng được chọn trong ComboBox
            string selectedType = cbCustomerType.SelectedItem?.ToString();

            // Bắt đầu lọc từ danh sách gốc
            var results = customerList.AsEnumerable();

            // Nếu không phải "Tất cả" thì lọc theo loại
            if (!string.IsNullOrEmpty(selectedType) && selectedType != "Tất cả")
            {
                results = results.Where(c => c.TypeCustomer.Equals(selectedType, StringComparison.OrdinalIgnoreCase));
            }

            // Nếu có từ khóa thì lọc tiếp theo tên hoặc ID
            if (!string.IsNullOrEmpty(keyword))
            {
                results = results.Where(c =>
                    c.FullName.ToLower().Contains(keyword) ||
                    c.Id.ToLower().Contains(keyword));
            }

            // Hiển thị danh sách kết quả
            DisplayCustomList(results.ToList());
        }

        private void DisplayCustomList(List<Customer> list)
        {
            listView1.Items.Clear();
            foreach (var customer in list)
            {
                ListViewItem row = new ListViewItem(customer.Id);
                row.SubItems.Add(customer.FullName);
                row.SubItems.Add(customer.TypeCustomer);
                row.SubItems.Add(customer.LastMonthAmount.ToString());
                row.SubItems.Add(customer.ThisMonthAmount.ToString());
                row.SubItems.Add(customer.CalculateElectricityBill().ToString());
                row.SubItems.Add(customer.EnvironmentalFee.ToString("F0"));
                row.SubItems.Add(customer.VatFee.ToString("F0"));
                row.SubItems.Add(customer.AmountPaid.ToString());
                listView1.Items.Add(row);
            }
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = btnSearch.Text.ToLower();
            var results = customerList.Where(c =>
                c.FullName.ToLower().Contains(keyword) || // Corrected property name to 'FullName'
                c.Id.Contains(keyword)) // Corrected property name to 'Id'
                .ToList();

            listView1.Items.Clear();
            foreach (var c in results)
            {
                ListViewItem row = new ListViewItem(c.Id);
                row.SubItems.Add(c.FullName);
                row.SubItems.Add(c.TypeCustomer);
                row.SubItems.Add(c.LastMonthAmount.ToString());
                row.SubItems.Add(c.ThisMonthAmount.ToString());
                row.SubItems.Add(c.CalculateElectricityBill().ToString());
                row.SubItems.Add(c.EnvironmentalFee.ToString("F0"));
                row.SubItems.Add(c.VatFee.ToString("F0"));
                row.SubItems.Add(c.AmountPaid.ToString());
                listView1.Items.Add(row);
            }
        }

        private void cbCustomerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            {
                string selectedType = cbCustomerType.Text; // Lấy giá trị được chọn hoặc nhập

                List<Customer> results;

                if (string.IsNullOrWhiteSpace(selectedType) || selectedType == "Tất cả")
                {
                    // Nếu không chọn gì hoặc chọn "Tất cả" → hiển thị toàn bộ
                    results = customerList;
                }
                else
                {
                    // Lọc theo loại khách hàng
                    results = customerList
                                .Where(c => c.TypeCustomer.Equals(selectedType, StringComparison.OrdinalIgnoreCase))
                                .ToList();
                }

                DisplayCustomList(results);
            }
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            var types = customerList.Select(c => c.TypeCustomer).Distinct().ToList();
            cbCustomerType.Items.Add("Tất cả");
            cbCustomerType.Items.Add("household");
            cbCustomerType.Items.Add("Business");
            cbCustomerType.Items.Add("Administration");
            cbCustomerType.Items.Add("Factory");
            cbCustomerType.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cbCustomerType.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbCustomerType.SelectedIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng để xuất hóa đơn.");
                return;
            }

            var id = listView1.SelectedItems[0].SubItems[0].Text;
            var customer = customerList.First(c => c.Id == id);

            string invoice = $"===== HÓA ĐƠN TIỀN ĐIỆN =====\n" +
                             $"Khách hàng: {customer.FullName}\n" +
                             $"Mã KH: {customer.Id}\n" +
                             $"Loại KH: {customer.TypeCustomer}\n" +
                             $"Chỉ số tháng trước: {customer.LastMonthAmount} kWh\n" +
                             $"Chỉ số tháng này: {customer.ThisMonthAmount} kWh\n" +
                             $"Sản lượng: {customer.Consumption} kWh\n" +
                             $"Tiền điện: {customer.CalculateElectricityBill():N0} VND\n" +
                             $"Phí môi trường: {customer.EnvironmentalFee:N0} VND\n" +
                             $"VAT: {customer.VatFee:N0} VND\n" +
                             $"TỔNG CỘNG: {customer.AmountPaid:N0} VND\n" +
                             $"Ngày in: {DateTime.Now:dd/MM/yyyy}\n" +
                             "============================";

            MessageBox.Show(invoice, "Hóa đơn");
            File.WriteAllText("Invoice.txt", invoice);
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string deleteId = txtDelete.Text.Trim();

            if (string.IsNullOrWhiteSpace(deleteId))
            {
                MessageBox.Show("Vui lòng nhập ID khách hàng để xóa.", "Thông báo");
                return;
            }

            // Tìm khách hàng theo ID
            var customer = customerList.FirstOrDefault(c => c.Id.Equals(deleteId, StringComparison.OrdinalIgnoreCase));

            if (customer == null)
            {
                MessageBox.Show("Không tìm thấy khách hàng có ID này.", "Lỗi");
                return;
            }

            // Xác nhận trước khi xóa
            var confirm = MessageBox.Show($"Bạn có chắc muốn xóa khách hàng '{customer.FullName}'?",
                                           "Xác nhận xóa",
                                           MessageBoxButtons.YesNo,
                                           MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                customerList.Remove(customer);
                DisplayCustomList(customerList); // Cập nhật lại ListView
                MessageBox.Show("Xóa khách hàng thành công!", "Thành công");
                txtDelete.Clear();
            }
        }

        private void btnNotifyWaterBill_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy thông tin khách hàng
                string id = txtId.Text.Trim();
                string name = txtName.Text.Trim();
                string customerType = cbCustomerType.Text.Trim(); // ComboBox chọn loại khách hàng

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(customerType))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ ID, Tên và Loại khách hàng.", "Lỗi");
                    return;
                }

                if (!int.TryParse(txtLastMonth.Text, out int lastMonth) ||
                    !int.TryParse(txtThisMonth.Text, out int thisMonth))
                {
                    MessageBox.Show("Vui lòng nhập số hợp lệ cho chỉ số nước.", "Lỗi");
                    return;
                }

                int consumption = thisMonth - lastMonth;
                if (consumption < 0)
                {
                    MessageBox.Show("Chỉ số tháng này phải lớn hơn hoặc bằng tháng trước.", "Lỗi");
                    return;
                }

                // Xác định VAT theo loại khách hàng
                double vatRate = GetVatRate(customerType);

                // Gọi hàm tính tiền
                double pricePerM3 = 10000; // Giá nước/m³
                (double baseBill, double envFee, double vatFee, double totalAmount) =
                    CalculateWaterBill(consumption, pricePerM3, vatRate);

                // Tạo thông báo hóa đơn
                string notification =
                    "===== WATER BILL PAYMENT NOTICE =====\n" +
                    $"Customer : {name} (ID: {id})\n" +
                    $"Type Customer    : {customerType}\n" +
                    $"quantity  : {consumption} m³\n" +
                    $"water bill  : {baseBill,15:N0} VND\n" +
                    $"Environmental fees : {envFee,9:N0} VND\n" +
                    $"VAT ({vatRate * 100}%): {vatFee,12:N0} VND\n" +
                    $"-------------------------------------------\n" +
                    $"Tatal Bill : {totalAmount,15:N0} VND\n" +
                    $"Payment due date: {DateTime.Now.AddDays(7):dd/MM/yyyy}\n" +
                    "Please pay on time to avoid service interruption..\n" +
                    "===========================================";

                // Hiển thị thông báo
                MessageBox.Show(notification, "Water bill notice");

                // Lưu ra file
                File.WriteAllText("WaterBillNotification.txt", notification);
                MessageBox.Show("The notice has been saved at WaterBillNotification.txt", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi");
            }
        }

        /// <summary>
        /// Lấy tỷ lệ VAT theo loại khách hàng
        /// </summary>
        private double GetVatRate(string customerType)
        {
            switch (customerType.ToLower())
            {
                case "Household":
                    return 0.05;
                case "Business":
                    return 0.08;
                case "Administration":
                    return 0.10;
                case "Factory":
                    return 0.12;
                default:
                    return 0.05; // Mặc định là 5% nếu không xác định được loại khách hàng
            }
        }

        /// <summary>
        /// Hàm tính tiền nước + thuế + phí môi trường
        /// </summary>
        private (double baseBill, double envFee, double vatFee, double totalAmount)
            CalculateWaterBill(int consumption, double pricePerM3, double vatRate)
        {
            double baseBill = consumption * pricePerM3;
            double envFee = baseBill * 0.05; 
            double vatFee = (baseBill + envFee) * vatRate;
            double totalAmount = baseBill + envFee + vatFee;
            return (baseBill, envFee, vatFee, totalAmount);
        }

        private void txtName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}



