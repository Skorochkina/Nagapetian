using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace HR_Reference_System
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            if (System.IO.File.Exists("logo.ico"))
                this.Icon = new Icon("logo.ico");

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            string lastName = txtLastNameFilter.Text.Trim();
            string position = txtPositionFilter.Text.Trim();

            try
            {
                using (SQLiteDataReader reader = DBMaster.GetFilteredEmployees(lastName, position))
                {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    dgvEmployees.DataSource = dt;

                    if (dgvEmployees.Columns.Count > 0)
                    {
                        if (dgvEmployees.Columns["Id"] != null)
                            dgvEmployees.Columns["Id"].Visible = false;
                        if (dgvEmployees.Columns["LastName"] != null)
                            dgvEmployees.Columns["LastName"].HeaderText = "Фамилия";
                        if (dgvEmployees.Columns["FirstName"] != null)
                            dgvEmployees.Columns["FirstName"].HeaderText = "Имя";
                        if (dgvEmployees.Columns["Patronymic"] != null)
                            dgvEmployees.Columns["Patronymic"].HeaderText = "Отчество";
                        if (dgvEmployees.Columns["Position"] != null)
                            dgvEmployees.Columns["Position"].HeaderText = "Должность";
                        if (dgvEmployees.Columns["Department"] != null)
                            dgvEmployees.Columns["Department"].HeaderText = "Отдел";
                        if (dgvEmployees.Columns["Phone"] != null)
                            dgvEmployees.Columns["Phone"].HeaderText = "Телефон";
                        if (dgvEmployees.Columns["Email"] != null)
                            dgvEmployees.Columns["Email"].HeaderText = "Email";
                        if (dgvEmployees.Columns["HireDate"] != null)
                            dgvEmployees.Columns["HireDate"].HeaderText = "Дата приема";
                    }

                    lblRecordCount.Text = $"Найдено записей: {dt.Rows.Count}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Добавление сотрудника
        private void btnAdd_Click(object sender, EventArgs e)
        {
            EmployeeCardForm cardForm = new EmployeeCardForm();
            if (cardForm.ShowDialog() == DialogResult.OK)
            {
                LoadEmployees(); // Обновляем список
                MessageBox.Show("Сотрудник успешно добавлен!", "Успех",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // Редактирование сотрудника
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int employeeId = Convert.ToInt32(dgvEmployees.CurrentRow.Cells["Id"].Value);

            // Получаем данные сотрудника из БД
            DataTable employeeData = DBMaster.GetEmployeeById(employeeId);
            if (employeeData.Rows.Count > 0)
            {
                DataRow row = employeeData.Rows[0];
                EmployeeCardForm cardForm = new EmployeeCardForm(employeeId, row);
                if (cardForm.ShowDialog() == DialogResult.OK)
                {
                    LoadEmployees(); // Обновляем список
                    MessageBox.Show("Данные сотрудника успешно обновлены!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Удаление сотрудника
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.CurrentRow == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления!", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int employeeId = Convert.ToInt32(dgvEmployees.CurrentRow.Cells["Id"].Value);
            string fullName = $"{dgvEmployees.CurrentRow.Cells["LastName"].Value} " +
                             $"{dgvEmployees.CurrentRow.Cells["FirstName"].Value}";

            DialogResult result = MessageBox.Show($"Вы действительно хотите удалить сотрудника:\n{fullName}?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (DBMaster.DeleteEmployee(employeeId))
                {
                    LoadEmployees(); // Обновляем список
                    MessageBox.Show("Сотрудник успешно удален!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении сотрудника!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Обновление данных
        private void btnRefreshData_Click(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            LoadEmployees();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtLastNameFilter.Clear();
            txtPositionFilter.Clear();
            LoadEmployees();
        }
    }
}