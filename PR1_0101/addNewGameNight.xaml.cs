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
using System.Windows.Shapes;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для addNewGameNight.xaml
    /// </summary>
    public partial class addNewGameNight : Window
    {
        Entities db = new Entities();
        public addNewGameNight()
        {
            InitializeComponent();
            CBresponsible.ItemsSource = db.Users.Where(x => x.Role1.ID == 3).ToList();
            LBGames.ItemsSource = db.BoardGames.ToList();
        }

        private void BTNaddNewGameNight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBGames.SelectedItems == null)
                {
                    MessageBox.Show("Укажите игры");
                    return;
                }
                if (DPStartTime.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала");
                    return;
                }
                if(CBresponsible.SelectedItem == null)
                {
                    MessageBox.Show("Укажите ответственного");
                    return;
                }
                if(TBMaxParticipants.Text == "" || TBMinParticipants.Text == "")
                {
                    MessageBox.Show("Заполните количество минимальных и максимальных участников");
                    return;
                }
                if(Convert.ToInt32(TBMinParticipants.Text) > Convert.ToInt32(TBMaxParticipants.Text))
                {
                    MessageBox.Show("Минимум игроков не может быть больше максимума");
                    return;
                }
                var date = DPStartTime.SelectedDate;
                var timeString = TBhour.Text + ":" + TBmin.Text;
                if (date.HasValue && TimeSpan.TryParse(timeString, out TimeSpan time))
                {
                    DateTime fullDateTime = date.Value.Date + time;
                    var selectedGames = LBGames.SelectedItems.Cast<BoardGames>().ToList();

                    foreach (var game in selectedGames)
                    {
                        int gameId = game.ID;
                        string gameName = game.NameGame;
                    }

                    var newGameNight = new GameNights
                    {
                        StartTime = fullDateTime,
                        Responsible = (CBresponsible.SelectedItem as Users).ID,
                        MinimumNumberOfParticipants = Convert.ToInt32(TBMinParticipants.Text),
                        MaximumNumberOfParticipants = Convert.ToInt32(TBMaxParticipants.Text)
                    };
                    db.GameNights.Add(newGameNight);
                    db.SaveChanges();

                    foreach (var game in selectedGames)
                    {
                        db.ListOfGames.Add(new ListOfGames
                        {
                            GameNights = newGameNight.ID,
                            BoardGames = game.ID
                        });
                    }
                    db.SaveChanges();
                    MessageBox.Show("Вы успешно добавили игротеку");
                    DialogResult = true;
                }
            }
            catch 
            {
                MessageBox.Show("Ошибка!!!");
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

        private void TBMinParticipants_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumbersOnly(TBMinParticipants);
        }

        private void TBMaxParticipants_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumbersOnly(TBMaxParticipants);

        }
    }
}
