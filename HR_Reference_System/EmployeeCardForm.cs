using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace HR_Reference_System
{
    public partial class EmployeeCardForm : Form
    {
        private int employeeId = 0;
        private bool isEditMode = false;

        public EmployeeCardForm(int id = 0, DataRow rowData = null)
        {
            InitializeComponent();

            employeeId = id;
            isEditMode = (id > 0 && rowData != null);

            this.Text = isEditMode ? "Редактирование сотрудника" : "Добавление сотрудника";
            lblTitle.Text = isEditMode ? "✏️ Редактирование карточки сотрудника" : "➕ Добавление нового сотрудника";

            if (System.IO.File.Exists("logo.ico"))
                this.Icon = new Icon("logo.ico");

            if (isEditMode && rowData != null)
            {
                LoadEmployeeData(rowData);
            }
        }

        private void LoadEmployeeData(DataRow row)
        {
            txtLastName.Text = row["LastName"].ToString();
            txtFirstName.Text = row["FirstName"].ToString();
            txtPatronymic.Text = row["Patronymic"].ToString();
            txtPosition.Text = row["Position"].ToString();
            txtDepartment.Text = row["Department"].ToString();
            txtPhone.Text = row["Phone"].ToString();
            txtEmail.Text = row["Email"].ToString();

            if (DateTime.TryParse(row["HireDate"].ToString(), out DateTime hireDate))
                dtpHireDate.Value = hireDate;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("Введите фамилию сотрудника!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите имя сотрудника!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPosition.Text))
            {
                MessageBox.Show("Введите должность сотрудника!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPosition.Focus();
                return;
            }

            bool result;
            if (isEditMode)
            {
                result = DBMaster.UpdateEmployee(
                    employeeId,
                    txtLastName.Text.Trim(),
                    txtFirstName.Text.Trim(),
                    txtPatronymic.Text.Trim(),
                    txtPosition.Text.Trim(),
                    txtDepartment.Text.Trim(),
                    txtPhone.Text.Trim(),
                    txtEmail.Text.Trim(),
                    dtpHireDate.Value.ToString("yyyy-MM-dd")
                );
            }
            else
            {
                result = DBMaster.AddEmployee(
                    txtLastName.Text.Trim(),
                    txtFirstName.Text.Trim(),
                    txtPatronymic.Text.Trim(),
                    txtPosition.Text.Trim(),
                    txtDepartment.Text.Trim(),
                    txtPhone.Text.Trim(),
                    txtEmail.Text.Trim(),
                    dtpHireDate.Value.ToString("yyyy-MM-dd")
                );
            }

            if (result)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Ошибка при сохранении данных!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}