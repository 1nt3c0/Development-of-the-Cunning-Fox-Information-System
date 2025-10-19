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
    /// Логика взаимодействия для editTournament.xaml
    /// </summary>
    public partial class editTournament : Window
    {
        Entities Db;
        Tournaments Tournaments;
        public editTournament(Entities db, Tournaments tournaments)
        {
            InitializeComponent();
            Tournaments = tournaments;
            Db = db;
            var list_BoardGames = db.BoardGames.ToList();
            var list_users = db.Users.Where(x => x.Role == 3).ToList();
            CBnameGame.ItemsSource = list_BoardGames;
            CBresponsible.ItemsSource = db.Users.Where(x => x.Role == 3).ToList();

            CBnameGame.SelectedIndex = list_BoardGames.FindIndex(x => x.ID == Tournaments.BoardGames1.ID);
            CBresponsible.SelectedIndex = list_users.FindIndex(x => x.ID == Tournaments.Responsible);
            TBageLimit.Text = Tournaments.AgeLimit.ToString();
            TBcost.Text = Tournaments.Price.ToString();
            DPdate.SelectedDate = Tournaments.TimeDate?.Date;
            TBhour.Text = Tournaments.TimeDate?.Hour.ToString();
            TBmin.Text = Tournaments.TimeDate?.Minute.ToString();
        }
       

        private void BTNeditTournament_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DPdate.SelectedDate < DateTime.Now)
                {
                    MessageBox.Show("Нельзя ввести прошлые числа");
                    return;
                }
                if (CBnameGame.SelectedItem == null)
                {
                    MessageBox.Show("Выберите игру");
                    return;
                }
                if (TBhour.Text == null || TBmin.Text == null)
                {
                    MessageBox.Show("Укажите конекретное время");
                    return;
                }
                if (Convert.ToInt32(TBhour.Text) < 0 || Convert.ToInt32(TBhour.Text) > 23)
                {
                    MessageBox.Show("Часовов не может быть меньше 0 или больше 23");
                    return;
                }
                if (Convert.ToInt32(TBmin.Text) < 0 || Convert.ToInt32(TBmin.Text) > 59)
                {
                    MessageBox.Show("Минут не может быть меньше 0 или больше 59");
                    return;
                }
                if (TBageLimit.Text == null)
                {
                    MessageBox.Show("Укажите возраст");
                    return;
                }
                if (TBcost.Text == null)
                {
                    MessageBox.Show("Укажите цену");
                    return;
                }
                if (CBresponsible.SelectedItem == null)
                {
                    MessageBox.Show("Укажите ответственного");
                    return;
                }

                var nowTournament = Db.Tournaments.FirstOrDefault(x => x.ID == Tournaments.ID);
                var date = DPdate.SelectedDate;
                var timeString = TBhour.Text + ":" + TBmin.Text;
                if (date.HasValue && TimeSpan.TryParse(timeString, out TimeSpan time))
                {
                    DateTime fullDateTime = date.Value.Date + time;
                    nowTournament.TimeDate = fullDateTime;
                    nowTournament.BoardGames = (CBnameGame.SelectedItem as BoardGames).ID;
                    nowTournament.AgeLimit = Convert.ToInt32(TBageLimit.Text);
                    nowTournament.Price = Convert.ToInt32(TBcost.Text);
                    nowTournament.Responsible = (CBresponsible.SelectedItem as Users).ID;
                    Db.SaveChanges();
                    MessageBox.Show("Турнир обновлен");
                    DialogResult = true;
                    Close();
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
