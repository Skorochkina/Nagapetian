using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace HR_Reference_System
{
    public class DBMaster
    {
        public static string dbPath = "hr_database.db";
        public static string connectionString = $"Data Source={dbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
                CreateTables();
                InsertTestData();

                MessageBox.Show("База данных успешно создана с тестовыми данными!\n\n" +
                    "Тестовый вход:\nЛогин: admin\nПароль: 123",
                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static void CreateTables()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                string sqlUsers = @"CREATE TABLE Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login TEXT NOT NULL UNIQUE,
                    Password TEXT NOT NULL,
                    FullName TEXT,
                    Role TEXT
                );";

                string sqlEmployees = @"CREATE TABLE Employees (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    LastName TEXT,
                    FirstName TEXT,
                    Patronymic TEXT,
                    Position TEXT,
                    Department TEXT,
                    Phone TEXT,
                    Email TEXT,
                    HireDate TEXT
                );";

                using (SQLiteCommand cmd = new SQLiteCommand(sqlUsers, conn)) cmd.ExecuteNonQuery();
                using (SQLiteCommand cmd = new SQLiteCommand(sqlEmployees, conn)) cmd.ExecuteNonQuery();

                conn.Close();
            }
        }

        private static void InsertTestData()
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // Администратор
                string sqlInsertUser = @"INSERT INTO Users (Login, Password, FullName, Role) 
                    VALUES ('admin', '123', 'Администратор Системы', 'Администратор');";
                using (SQLiteCommand cmd = new SQLiteCommand(sqlInsertUser, conn)) cmd.ExecuteNonQuery();

                // Расширенные тестовые данные сотрудников
                string[] employees = {
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Сидоров', 'Петр', 'Алексеевич', 'Ведущий разработчик', 'IT-отдел', '+7 (999) 111-22-33', 'petr.sidorov@profi.ru', '2023-01-15');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Кузнецова', 'Анна', 'Сергеевна', 'HR-директор', 'Отдел персонала', '+7 (999) 222-33-44', 'anna.kuznetsova@profi.ru', '2022-11-01');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Васильев', 'Дмитрий', 'Игоревич', 'Главный бухгалтер', 'Бухгалтерия', '+7 (999) 333-44-55', 'dmitry.vasiliev@profi.ru', '2024-02-20');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Морозова', 'Екатерина', 'Андреевна', 'Менеджер по подбору персонала', 'Отдел персонала', '+7 (999) 444-55-66', 'ekaterina.morozova@profi.ru', '2023-06-10');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Соколов', 'Алексей', 'Владимирович', 'Системный администратор', 'IT-отдел', '+7 (999) 555-66-77', 'alexey.sokolov@profi.ru', '2023-09-05');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Новикова', 'Ольга', 'Павловна', 'Офис-менеджер', 'Административный отдел', '+7 (999) 666-77-88', 'olga.novikova@profi.ru', '2024-01-15');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Козлов', 'Максим', 'Денисович', 'Специалист по подбору', 'Отдел персонала', '+7 (999) 777-88-99', 'maxim.kozlov@profi.ru', '2023-03-20');",
                    "INSERT INTO Employees (LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate) VALUES ('Лебедева', 'Ирина', 'Витальевна', 'Финансовый аналитик', 'Финансовый отдел', '+7 (999) 888-99-00', 'irina.lebedev@profi.ru', '2023-11-12');"
                };

                foreach (string sql in employees)
                {
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn)) cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
        }

        public static bool AuthenticateUser(string login, string password)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Users WHERE Login = @login AND Password = @password";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public static SQLiteDataReader GetFilteredEmployees(string lastNameFilter = "", string positionFilter = "")
        {
            SQLiteConnection conn = new SQLiteConnection(connectionString);
            conn.Open();

            string sql = @"SELECT Id, LastName, FirstName, Patronymic, Position, Department, Phone, Email, HireDate 
                          FROM Employees WHERE 1=1";

            if (!string.IsNullOrEmpty(lastNameFilter))
                sql += " AND LastName LIKE @lastName";

            if (!string.IsNullOrEmpty(positionFilter))
                sql += " AND Position LIKE @position";

            sql += " ORDER BY LastName";

            SQLiteCommand cmd = new SQLiteCommand(sql, conn);

            if (!string.IsNullOrEmpty(lastNameFilter))
                cmd.Parameters.AddWithValue("@lastName", "%" + lastNameFilter + "%");

            if (!string.IsNullOrEmpty(positionFilter))
                cmd.Parameters.AddWithValue("@position", "%" + positionFilter + "%");

            return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }
        // Получение сотрудника по ID
        public static DataTable GetEmployeeById(int id)
        {
            DataTable dt = new DataTable();
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT * FROM Employees WHERE Id = @id";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        // Добавление нового сотрудника
        public static bool AddEmployee(string lastName, string firstName, string patronymic,
                                       string position, string department, string phone,
                                       string email, string hireDate)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO Employees (LastName, FirstName, Patronymic, Position, 
                          Department, Phone, Email, HireDate) 
                          VALUES (@lastName, @firstName, @patronymic, @position, 
                          @department, @phone, @email, @hireDate)";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@patronymic", patronymic);
                        cmd.Parameters.AddWithValue("@position", position);
                        cmd.Parameters.AddWithValue("@department", department);
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hireDate", hireDate);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка добавления: " + ex.Message);
                return false;
            }
        }

        // Обновление данных сотрудника
        public static bool UpdateEmployee(int id, string lastName, string firstName, string patronymic,
                                          string position, string department, string phone,
                                          string email, string hireDate)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE Employees SET 
                          LastName = @lastName,
                          FirstName = @firstName,
                          Patronymic = @patronymic,
                          Position = @position,
                          Department = @department,
                          Phone = @phone,
                          Email = @email,
                          HireDate = @hireDate
                          WHERE Id = @id";

                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@lastName", lastName);
                        cmd.Parameters.AddWithValue("@firstName", firstName);
                        cmd.Parameters.AddWithValue("@patronymic", patronymic);
                        cmd.Parameters.AddWithValue("@position", position);
                        cmd.Parameters.AddWithValue("@department", department);
                        cmd.Parameters.AddWithValue("@phone", phone);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@hireDate", hireDate);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка обновления: " + ex.Message);
                return false;
            }
        }

        // Удаление сотрудника
        public static bool DeleteEmployee(int id)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Employees WHERE Id = @id";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка удаления: " + ex.Message);
                return false;
            }
        }
    }
}