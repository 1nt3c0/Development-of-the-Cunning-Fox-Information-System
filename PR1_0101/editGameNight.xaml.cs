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
using System.Data.Entity;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для editGameNight.xaml
    /// </summary>
    public partial class editGameNight : Window
    {
        Entities Db;
        GameNights GameNights;

        public editGameNight(Entities db,GameNights gameNight)
        {
            InitializeComponent();
            Db = db;
            GameNights = gameNight;
            LoadData();

        }
        private void LoadData()
        {
            var responsibles = Db.Users.Where(x => x.Role == 3).ToList();
            CBresponsible.ItemsSource = responsibles;
            if (GameNights.Responsible.HasValue)
            {
                var selectedUser = responsibles.FirstOrDefault(u => u.ID == GameNights.Responsible.Value);
                CBresponsible.SelectedItem = selectedUser;
            }

            var allGames = Db.BoardGames.ToList();
            LBGames.ItemsSource = allGames;
            if (GameNights.StartTime != null)
            {
                DPStartTime.SelectedDate = GameNights.StartTime.Value.Date;
                TBhour.Text = GameNights.StartTime.Value.Hour.ToString("D2");
                TBmin.Text = GameNights.StartTime.Value.Minute.ToString("D2");
            }

            TBMinParticipants.Text = GameNights.MinimumNumberOfParticipants?.ToString() ?? "";
            TBMaxParticipants.Text = GameNights.MaximumNumberOfParticipants?.ToString() ?? "";

            var currentGameIds = Db.ListOfGames
                .Where(l => l.GameNights == GameNights.ID)
                .Select(l => l.BoardGames)
                .ToList();

            foreach (var item in LBGames.Items)
            {
                var game = item as BoardGames;
                if (game != null && currentGameIds.Contains(game.ID))
                {
                    LBGames.SelectedItems.Add(item);
                }
            }
        }

        private void BTNsave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DPStartTime.SelectedDate == null)
                {
                    MessageBox.Show("Укажите дату начала");
                    return;
                }
                if (CBresponsible.SelectedItem == null)
                {
                    MessageBox.Show("Укажите ответственного");
                    return;
                }
                if (TBMinParticipants.Text == "" || TBMaxParticipants.Text == "")
                {
                    MessageBox.Show("Заполните количество участников");
                    return;
                }
                if (!int.TryParse(TBMinParticipants.Text, out int min) || !int.TryParse(TBMaxParticipants.Text, out int max))
                {
                    MessageBox.Show("Введите корректные числа");
                    return;
                }
                if (min > max)
                {
                    MessageBox.Show("Минимум не может быть больше максимума");
                    return;
                }
                if (LBGames.SelectedItems.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы одну игру");
                    return;
                }

                string timeStr = $"{TBhour.Text}:{TBmin.Text}";
                if (!TimeSpan.TryParse(timeStr, out TimeSpan time))
                {
                    MessageBox.Show("Некорректное время");
                    return;
                }

                var nowGameNight = Db.GameNights.FirstOrDefault(x => x.ID == GameNights.ID);
                nowGameNight.StartTime = DPStartTime.SelectedDate.Value.Date + time;
                nowGameNight.Responsible = (CBresponsible.SelectedItem as Users).ID;
                nowGameNight.MinimumNumberOfParticipants = min;
                nowGameNight.MaximumNumberOfParticipants = max;

                Db.SaveChanges();

                // Удаляем старые связи
                var oldLinks = Db.ListOfGames.Where(l => l.GameNights == GameNights.ID).ToList();
                foreach (var link in oldLinks)
                    Db.ListOfGames.Remove(link);

                // Добавляем новые связи
                var selectedGames = LBGames.SelectedItems.Cast<BoardGames>().ToList();
                foreach (var game in selectedGames)
                {
                    Db.ListOfGames.Add(new ListOfGames
                    {
                        GameNights = GameNights.ID,
                        BoardGames = game.ID
                    });
                }

                Db.SaveChanges();
                MessageBox.Show("Игротека успешно обновлена!");
                DialogResult = true;
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
        private void TBhour_TextChanged(object sender, TextChangedEventArgs e) => NumbersOnly(TBhour);
        private void TBmin_TextChanged(object sender, TextChangedEventArgs e) => NumbersOnly(TBmin);
        private void TBMinParticipants_TextChanged(object sender, TextChangedEventArgs e) => NumbersOnly(TBMinParticipants);
        private void TBMaxParticipants_TextChanged(object sender, TextChangedEventArgs e) => NumbersOnly(TBMaxParticipants);
    }
}
