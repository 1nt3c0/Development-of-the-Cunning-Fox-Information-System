using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для RegGameNightPlayer.xaml
    /// </summary>
    public partial class RegGameNightPlayer : Window
    {
        public int AGE;
        public RegGameNightPlayer()
        {
            InitializeComponent();
          
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DPborn.SelectedDate.ToString() == "")
                {
                    MessageBox.Show("Укажите дату рождения");
                    return;
                }
                DateTime birthDate = DPborn.SelectedDate.Value.Date;
                DateTime now = DateTime.Today;

                int age = now.Year - birthDate.Year;
                // Проверяем, прошёл ли уже день рождения в этом году
                if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                {
                    age--;
                }
                AGE = age;
                DialogResult = true;
                this.Close();
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
