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
using System.ComponentModel;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для MasterWindow.xaml
    /// </summary>
    public partial class MasterWindow : ConfirmCloseWindow
    {
        Entities db = new Entities();
        Users User;
        public MasterWindow(Users user)
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
                    ListOfGames = g.ToList()
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
            var listBox = sender as ListBox;
            var selectedGame = listBox.SelectedItem as BoardGames;
            if (selectedGame != null)
            {
                TBdescription.Text = selectedGame.Description;
            }
        }

        private void BTNaddNewGame_Click(object sender, RoutedEventArgs e)
        {
            addNewGameWindow addNewGameWindow = new addNewGameWindow();
            bool? result = addNewGameWindow.ShowDialog(); 
            if (result == true)
            {
                LBallGame.ItemsSource = db.BoardGames.ToList();
            }
        }

        private void BTNaddNewTournaments_Click(object sender, RoutedEventArgs e)
        {
            addNewTournament addNewTournament = new addNewTournament();
            if(addNewTournament.ShowDialog() == true)
            {
                LBtournaments.ItemsSource = db.Tournaments.ToList();
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
    }
}
