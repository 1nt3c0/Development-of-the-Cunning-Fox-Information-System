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
using System.Data.Entity;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : ConfirmCloseWindow
    {   Users User;
        Entities db = new Entities();
        public AdminWindow(Users user)
        {
            InitializeComponent();
            LBallGame.ItemsSource = db.BoardGames.ToList();
            CBgenre.ItemsSource = db.Genre.ToList();
            CBcategory.ItemsSource = db.Category.ToList();
            CBthematics.ItemsSource = db.Thematics.ToList();
            CBageLimit.ItemsSource = db.BoardGames.Select(x => x.AgeLimit).Distinct().ToList();
            User = user;
            TBnameMaster.Text = "Здравствуйте, " + User.FirstName + " " + User.LastName;
            TBroleMaster.Text = "Роль: " + User.Role1;
            LBtournaments.ItemsSource = db.Tournaments.ToList();
            RefreshLBgameNight();

            LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();
            CBplayerName.ItemsSource = db.Users.Where(x => db.GameNightsApplications.Any(h => h.User == x.ID)).ToList();

            LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();
            CBtournamentUsers.ItemsSource = db.Users.Where(x => db.TournamentsApplications.Any(h => h.User == x.ID)).ToList();

        }
        public void RefreshLBgameNight()
        {
            var list = db.ListOfGames
                .Include(x => x.GameNights1)
                .Include(x => x.BoardGames1)
                .ToList();

            var grouped = list
                .GroupBy(x => x.GameNights1.ID)
                .Select(g => new GameNights
                {
                    ID = g.Key,
                    StartTime = g.First().GameNights1.StartTime,
                    ListOfGames = g.ToList(),
                    Responsible = g.FirstOrDefault().GameNights1.Responsible,
                    MaximumNumberOfParticipants = g.First().GameNights1.MaximumNumberOfParticipants,
                    MinimumNumberOfParticipants = g.First().GameNights1.MinimumNumberOfParticipants
                })
                .OrderBy(x => x.ID)
                .ToList();

            LBgameNight.ItemsSource = grouped;
        }

        private void BTNcancleFilter_Click(object sender, RoutedEventArgs e)
        {
            CBageLimit.SelectedIndex = -1;
            CBcategory.SelectedIndex = -1;
            CBgenre.SelectedIndex = -1;
            CBthematics.SelectedIndex = -1;
        }
        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            int category = CBcategory.SelectedItem != null ? (CBcategory.SelectedItem as Category).ID : 0;
            int genre = CBgenre.SelectedItem != null ? (CBgenre.SelectedItem as Genre).ID : 0;
            int theme = CBthematics.SelectedItem != null ? (CBthematics.SelectedItem as Thematics).ID : 0;
            int? ageLimit = CBageLimit.SelectedItem as int?;

            var filtered = db.BoardGames.Where(g =>
                (category == 0 || g.Category == category) &&
                (genre == 0 || g.Genre == genre) &&
                (theme == 0 || g.Thematics == theme) &&
                (!ageLimit.HasValue || g.AgeLimit == ageLimit.Value)
            ).ToList();
            LBallGame.ItemsSource = filtered;
        }

        private void LBallGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var listBox = sender as ListBox;
                var selectedGame = listBox.SelectedItem as BoardGames;
                if (selectedGame != null)
                {
                    TBdescription.Text = selectedGame.Description;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNaddNewGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addNewGameWindow addNewGameWindow = new addNewGameWindow();
                bool? result = addNewGameWindow.ShowDialog();
                if (result == true)
                {
                    LBallGame.ItemsSource = db.BoardGames.ToList();
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNaddNewTournaments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addNewTournament addNewTournament = new addNewTournament();
                if (addNewTournament.ShowDialog() == true)
                {
                    LBtournaments.ItemsSource = db.Tournaments.ToList();
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNdelTournaments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBtournaments.SelectedItem != null)
                {
                    var selectedItem = LBtournaments.SelectedItem as Tournaments;

                    MessageBoxResult result = MessageBox.Show(
                         $"Вы уверены, что хотите удалить турнир \"{selectedItem.BoardGames1}\"?",
                         "Подтверждение удаления",
                         MessageBoxButton.YesNo,
                         MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        int tournamentId = selectedItem.ID;
                        var applicationsToDelete = db.TournamentsApplications.Where(x => x.Tournaments == tournamentId).ToList(); //заявки на удаление
                        db.TournamentsApplications.RemoveRange(applicationsToDelete);

                        var participantsToDelete = db.TournamentsParticipiants.Where(x => x.Tournaments == tournamentId).ToList(); //записи об участниках турнира
                        db.TournamentsParticipiants.RemoveRange(participantsToDelete);

                        db.Tournaments.Remove(selectedItem);

                        db.SaveChanges();
                        LBtournaments.ItemsSource = null;
                        LBtournaments.ItemsSource = db.Tournaments.ToList();

                        LBtournamentsOrder.ItemsSource = null;
                        LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();
                        MessageBox.Show("Турнир и все связанные данные успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите турнир");
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNeditTournaments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBtournaments.SelectedItem != null)
                {
                    var selectedTournament = LBtournaments.SelectedItem as Tournaments;
                    editTournament editTournament = new editTournament(db, selectedTournament);
                    if (editTournament.ShowDialog() == true)
                    {
                        LBtournaments.ItemsSource = db.Tournaments.ToList();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите турнир");
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNaddNewGameNight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                addNewGameNight addNewGameNight = new addNewGameNight();
                if (addNewGameNight.ShowDialog() == true)
                {
                    RefreshLBgameNight();
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNeditGameNight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBgameNight.SelectedItem != null)
                {
                    var selectedGameNight = LBgameNight.SelectedItem as GameNights;
                    editGameNight edit = new editGameNight(db, selectedGameNight);
                    if (edit.ShowDialog() == true)
                    {
                        RefreshLBgameNight();
                    }
                }
                else
                {
                    MessageBox.Show("Выберите игротеку");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNconfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBgameNightOrder.SelectedItem != null)
                {
                    var selectedGameNightOrder = LBgameNightOrder.SelectedItem as GameNightsApplications;
                    var nowGameNightOrder = db.GameNightsApplications.FirstOrDefault(x => x.ID == selectedGameNightOrder.ID);

                    if (nowGameNightOrder.Status == 2)
                    {
                        MessageBox.Show("Заявка уже подтверждена");
                        return;
                    }

                    if (nowGameNightOrder.Status == 1)
                    {
                        nowGameNightOrder.Status = 2;
                        db.SaveChanges();
                        LBgameNightOrder.ItemsSource = null;
                        LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();

                        var newGameNightsParticipiant = new GameNightsParticipiants
                        {
                            GameNights = nowGameNightOrder.GameNights,
                            Participiants = nowGameNightOrder.Users.ID,
                        };
                        db.GameNightsParticipiants.Add(newGameNightsParticipiant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка подтверждена");
                        return;
                    }
                    if (nowGameNightOrder.Status == 3)
                    {
                        var res = MessageBox.Show(
                            "Вы уверены что хотите подтвертить, отклоненную заявку?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (res != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        nowGameNightOrder.Status = 2;
                        db.SaveChanges();
                        LBgameNightOrder.ItemsSource = null;
                        LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();
                        var newGameNightsParticipiant = new GameNightsParticipiants
                        {
                            GameNights = nowGameNightOrder.GameNights,
                            Participiants = nowGameNightOrder.Users.ID,
                        };
                        db.GameNightsParticipiants.Add(newGameNightsParticipiant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка подтверждена");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заявку", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNdenyOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBgameNightOrder.SelectedItem != null)
                {
                    var selectedGameNightOrder = LBgameNightOrder.SelectedItem as GameNightsApplications;
                    var nowGameNightOrder = db.GameNightsApplications.FirstOrDefault(x => x.ID == selectedGameNightOrder.ID);

                    if (nowGameNightOrder.Status == 3)
                    {
                        MessageBox.Show("Заявка уже отклонена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (nowGameNightOrder.Status == 2)
                    {
                        var res = MessageBox.Show(
                            "Вы умерены что хотите отклонить уже принятую заявку?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);
                        if (res != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        nowGameNightOrder.Status = 3;
                        db.SaveChanges();
                        MessageBox.Show("Заявка отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LBgameNightOrder.ItemsSource = null;
                        LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();

                        var delGameNightsParticipiant = db.GameNightsParticipiants.FirstOrDefault(x => x.GameNights == nowGameNightOrder.GameNights && x.Participiants == nowGameNightOrder.Users.ID);
                        db.GameNightsParticipiants.Remove(delGameNightsParticipiant);
                        db.SaveChanges();
                        return;
                    }

                    if (nowGameNightOrder.Status == 1)
                    {
                        nowGameNightOrder.Status = 3;
                        db.SaveChanges();
                        LBgameNightOrder.ItemsSource = null;
                        LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();
                        MessageBox.Show("Заявка отклонена");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заявку", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CBplayerName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = CBplayerName.SelectedItem as Users;
            if (CBplayerName.SelectedIndex == -1)
            {
                LBgameNightOrder.ItemsSource = null;
                LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();
                return;
            }
            LBgameNightOrder.ItemsSource = db.GameNightsApplications.Where(x => x.User == selectedUser.ID).ToList();
            
        }

        private void BTNreset_Click(object sender, RoutedEventArgs e)
        {
            CBplayerName.SelectedIndex = -1;
        }

        private void BTNconfirmTournamentOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBtournamentsOrder.SelectedItem != null)
                {
                    var selectedTournamentOrder = LBtournamentsOrder.SelectedItem as TournamentsApplications;
                    var nowTournamentOrder = db.TournamentsApplications.FirstOrDefault(x => x.ID == selectedTournamentOrder.ID);

                    if (nowTournamentOrder.Status == 2)
                    {
                        MessageBox.Show("Заявка уже подтверждена");
                        return;
                    }

                    if (nowTournamentOrder.Status == 1)
                    {
                        nowTournamentOrder.Status = 2;
                        db.SaveChanges();
                        LBtournamentsOrder.ItemsSource = null;
                        LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();

                        var newTournamentParticipiant = new TournamentsParticipiants
                        {
                            Tournaments = nowTournamentOrder.Tournaments,
                            Participiants = nowTournamentOrder.Users.ID,
                        };
                        db.TournamentsParticipiants.Add(newTournamentParticipiant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка подтверждена");
                        return;
                    }
                    if (nowTournamentOrder.Status == 3)
                    {
                        var res = MessageBox.Show(
                            "Вы уверены что хотите подтвертить, отклоненную заявку?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);

                        if (res != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        nowTournamentOrder.Status = 2;
                        db.SaveChanges();
                        LBtournamentsOrder.ItemsSource = null;
                        LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();
                        var newTournamentParticipiant = new TournamentsParticipiants
                        {
                            Tournaments = nowTournamentOrder.Tournaments,
                            Participiants = nowTournamentOrder.Users.ID,
                        };
                        db.TournamentsParticipiants.Add(newTournamentParticipiant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка подтверждена");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заявку", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void BTNdenyTournamentOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBtournamentsOrder.SelectedItem != null)
                {
                    var selectedTournamentOrder = LBtournamentsOrder.SelectedItem as TournamentsApplications;
                    var nowTournamentOrder = db.TournamentsApplications.FirstOrDefault(x => x.ID == selectedTournamentOrder.ID);

                    if (nowTournamentOrder.Status == 3)
                    {
                        MessageBox.Show("Заявка уже отклонена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (nowTournamentOrder.Status == 2)
                    {
                        var res = MessageBox.Show(
                            "Вы умерены что хотите отклонить уже принятую заявку?",
                            "Подтверждение",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning);
                        if (res != MessageBoxResult.Yes)
                        {
                            return;
                        }
                        nowTournamentOrder.Status = 3;
                        db.SaveChanges();
                        LBtournamentsOrder.ItemsSource = null;
                        LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();

                        var delTournamentsParticipiant = db.TournamentsParticipiants.FirstOrDefault(x => x.Tournaments == nowTournamentOrder.Tournaments && x.Participiants == nowTournamentOrder.Users.ID);
                        db.TournamentsParticipiants.Remove(delTournamentsParticipiant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка отклонена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    if (nowTournamentOrder.Status == 1)
                    {
                        nowTournamentOrder.Status = 3;
                        db.SaveChanges();
                        LBtournamentsOrder.ItemsSource = null;
                        LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();
                        MessageBox.Show("Заявка отклонена");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Выберите заявку", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Ошибка","Глобальная",MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        private void BTNresetTournaments_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CBtournamentUsers.SelectedIndex = -1;
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CBtournamentUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedUser = CBtournamentUsers.SelectedItem as Users;
            if (CBtournamentUsers.SelectedIndex == -1)
            {
                LBtournamentsOrder.ItemsSource = null;
                LBtournamentsOrder.ItemsSource = db.TournamentsApplications.ToList();
                return;
            }
            LBtournamentsOrder.ItemsSource = db.TournamentsApplications.Where(x => x.User == selectedUser.ID).ToList();
        }

        private void BTNdelGameNight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LBgameNight.SelectedItem != null)
                {
                    var selectedItem = LBgameNight.SelectedItem as GameNights;

                    MessageBoxResult result = MessageBox.Show(
                         $"Вы уверены, что хотите удалить игротеку?",
                         "Подтверждение удаления",
                         MessageBoxButton.YesNo,
                         MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var gameNightToDelete = db.GameNights.FirstOrDefault(g => g.ID == selectedItem.ID);


                        int gameNightId = gameNightToDelete.ID;
                        var applicationsToDelete = db.GameNightsApplications.Where(x => x.GameNights == gameNightId).ToList();
                        db.GameNightsApplications.RemoveRange(applicationsToDelete);

                        var participantsToDelete = db.GameNightsParticipiants.Where(x => x.GameNights == gameNightId).ToList();
                        db.GameNightsParticipiants.RemoveRange(participantsToDelete);

                        var gamesInGameNightToDelete = db.ListOfGames.Where(x => x.GameNights == gameNightId).ToList();
                        db.ListOfGames.RemoveRange(gamesInGameNightToDelete);

                        db.GameNights.Remove(gameNightToDelete);

                        db.SaveChanges();
                        LBgameNight.ItemsSource = null;
                        LBgameNight.ItemsSource = db.GameNights.ToList();

                        LBgameNightOrder.ItemsSource = null;
                        LBgameNightOrder.ItemsSource = db.GameNightsApplications.ToList();
                    MessageBox.Show("Игротека и все связанные данные успешно удалены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Выберите турнир");
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
