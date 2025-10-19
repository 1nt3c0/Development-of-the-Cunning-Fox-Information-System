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
    /// Логика взаимодействия для addNewTournament.xaml
    /// </summary>
    public partial class addNewTournament : Window
    {
        Entities db = new Entities();
        public addNewTournament()
        {
            InitializeComponent();
            CBnameGame.ItemsSource = db.BoardGames.ToList();
            CBresponsible.ItemsSource = db.Users.Where(x => x.Role == 3).ToList();
        }

        private void BTNaddNewGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DPdate.SelectedDate < DateTime.Now)
                {
                    MessageBox.Show("Нельзя ввести прошлые числа");
                    return;
                }
                if(CBnameGame.SelectedItem == null)
                {
                    MessageBox.Show("Выберите игру");
                    return;
                }
                if(TBhour.Text == null || TBmin.Text == null)
                {
                    MessageBox.Show("Укажите конекретное время");
                    return;
                }
                if(Convert.ToInt32(TBhour.Text) < 0 || Convert.ToInt32(TBhour.Text) > 23)
                {
                    MessageBox.Show("Часовов не может быть меньше 0 или больше 23");
                    return;
                }
                if (Convert.ToInt32(TBmin.Text) < 0 || Convert.ToInt32(TBmin.Text) > 59)
                {
                    MessageBox.Show("Минут не может быть меньше 0 или больше 59");
                    return;
                }
                if(CBageLimit.SelectedItem == null)
                {
                    MessageBox.Show("Укажите возраст");
                    return;
                }
                if(TBcost.Text == null)
                {
                    MessageBox.Show("Укажите цену");
                    return;
                }
                if(CBresponsible.SelectedItem == null)
                {
                    MessageBox.Show("Укажите ответственного");
                    return;
                }

                var selectedItem = CBageLimit.SelectedItem as ComboBoxItem;
                string content = selectedItem.Content.ToString();
                int age = Convert.ToInt32(content);
                var date = DPdate.SelectedDate;
                var timeString = TBhour.Text + ":" + TBmin.Text;
                if (date.HasValue && TimeSpan.TryParse(timeString, out TimeSpan time))
                {
                    DateTime fullDateTime = date.Value.Date + time;
                    var newTournament = new Tournaments
                    {
                        TimeDate = fullDateTime,
                        AgeLimit = age,
                        BoardGames = (CBnameGame.SelectedItem as BoardGames).ID,
                        Price = Convert.ToInt32(TBcost.Text),
                        Responsible = (CBresponsible.SelectedItem as Users).ID
                    };
                    db.Tournaments.Add(newTournament);
                    db.SaveChanges();
                    MessageBox.Show("Турнир добавлен");
                    DialogResult = true;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void NumbersOnly(TextBox tb)
        {
            tb.Text = new string(tb.Text.Where(char.IsDigit).ToArray());
            tb.CaretIndex = tb.Text.Length;
        }

        private void TBhour_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumbersOnly(TBhour);
        }

        private void TBmin_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumbersOnly(TBmin);
        }

        private void TBcost_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumbersOnly(TBcost);
        }
    }
}
