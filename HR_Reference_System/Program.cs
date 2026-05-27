using System;
using System.Windows.Forms;

namespace HR_Reference_System
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализация базы данных
            DBMaster.InitializeDatabase();

            // Запуск формы авторизации
            Application.Run(new FormAuth());
        }
    }
}