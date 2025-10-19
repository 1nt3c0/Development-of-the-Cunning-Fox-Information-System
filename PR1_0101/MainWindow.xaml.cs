using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Entity;
using System.ComponentModel;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ConfirmCloseWindow
    {
        Entities db = new Entities();
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void BTNinsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TBlogin.Text == null)
                {
                    MessageBox.Show("Заполните Логин");
                    return;
                }
                if (TBpassword.Text == null)
                {
                    MessageBox.Show("Заполните Пароль");
                    return;
                }

                var user = db.Users.Include(x => x.GameNightsApplications).FirstOrDefault(x => x.Password == TBpassword.Text && x.Login == TBlogin.Text);
                if (user != null)
                {
                    if (user.Role == 1) //Админ
                    {
                        AdminWindow adminWindow = new AdminWindow(user);
                        adminWindow.Show();
                        this.Close();
                    }
                    if (user.Role == 2) //Игрок
                    {
                        PlayerWindow playerWindow = new PlayerWindow(user);
                        playerWindow.Show();
                        this.Close();
                    }
                    if (user.Role == 3) //Мастер
                    {
                        MasterWindow masterWindow = new MasterWindow(user);
                        masterWindow.Show();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Данного пользователя не существует.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
