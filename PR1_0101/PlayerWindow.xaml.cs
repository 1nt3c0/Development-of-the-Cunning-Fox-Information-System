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
using System.IO;
using Microsoft.Win32;
using System.Data.Entity;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;


namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для PlayerWindow.xaml
    /// </summary>
    public partial class PlayerWindow : ConfirmCloseWindow
    {
        Entities db = new Entities();
        Users Users1;
        public PlayerWindow(Users users)
        {
            InitializeComponent();

            LBallGame.ItemsSource = db.BoardGames.ToList();
            CBgenre.ItemsSource = db.Genre.ToList();
            CBcategory.ItemsSource = db.Category.ToList();
            CBthematics.ItemsSource = db.Thematics.ToList();
            CBageLimit.ItemsSource = db.BoardGames.Select(x => x.AgeLimit).Distinct().ToList();
            Users1 = users;
            MessageBox.Show("Здравствуйте " + Users1.LastName + " " + Users1.FirstName + "\n" + "Роль: " + Users1.Role1);

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
            LBtournaments.ItemsSource = db.Tournaments.ToList();
            string imagePath =  System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../Images", Users1.Photo);
            Iuser.Source = new BitmapImage(new Uri(imagePath));
            TBuserName.Text = "Пользователь: " + Users1.LastName + " " + Users1.FirstName;
            TBuserRole.Text = "Роль: " + Users1.Role1;
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

        private void LBallGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;
            var selectedGame = listBox.SelectedItem as BoardGames;
            if (selectedGame != null)
            {
                TBdescription.Text = selectedGame.Description;
            }
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

        private void BTNcancleFilter_Click(object sender, RoutedEventArgs e)
        {
            CBageLimit.SelectedIndex = -1;
            CBcategory.SelectedIndex = -1;
            CBgenre.SelectedIndex = -1;
            CBthematics.SelectedIndex = -1;
        }

        private void BTNregGameNight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedGameNight = (LBgameNight.SelectedItem as GameNights).ID;
                if (db.GameNightsApplications.Any(x => x.User == Users1.ID && x.GameNights == selectedGameNight))
                {
                    MessageBox.Show("Заявка уже отправлена");
                    return;
                }
                if (LBgameNight.SelectedItem == null)
                {
                    MessageBox.Show("Выберите игротеку");
                    return;
                }

                RegGameNightPlayer regGameNightPlayer = new RegGameNightPlayer();
                if (regGameNightPlayer.ShowDialog() == true)
                {
                    int Age = regGameNightPlayer.AGE;

                    var newParticipant = new GameNightsApplications
                    {
                        User = Users1.ID,
                        GameNights = selectedGameNight,
                        Age = Age,
                        Status = 1
                    };
                    db.GameNightsApplications.Add(newParticipant);
                    db.SaveChanges();
                    MessageBox.Show("Заявка отправлена");
                };
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNregTournament_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedTournament = (LBtournaments.SelectedItem as Tournaments).ID;

                if (db.TournamentsApplications.Any(x => x.User == Users1.ID && x.Tournaments == selectedTournament))
                {
                    MessageBox.Show("Заявка уже отправлена");
                    return;
                }
                if (LBtournaments.SelectedItem == null)
                {
                    MessageBox.Show("Выберите турнир");
                    return;
                }

                RegGameNightPlayer regGameNightPlayer = new RegGameNightPlayer();
                if (regGameNightPlayer.ShowDialog() == true)
                {
                    int Age = regGameNightPlayer.AGE;

                    if (Age >= (LBtournaments.SelectedItem as Tournaments).AgeLimit)
                    {
                        var newParticipant = new TournamentsApplications
                        {
                            User = Users1.ID,
                            Tournaments = selectedTournament,
                            Age = Age,
                            Status = 1
                        };
                        db.TournamentsApplications.Add(newParticipant);
                        db.SaveChanges();
                        MessageBox.Show("Заявка отправлена");
                    }
                    else
                    {
                        MessageBox.Show("Ваш возраст слишком мал");
                        return;
                    }
                };
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BTNeditPhoto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif;*.bmp)|*.png;*.jpeg;*.jpg;*.gif;*.bmp|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите изображение";

                // Показываем диалог и проверяем, что пользователь выбрал файл
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string fileName = System.IO.Path.GetFileName(selectedFilePath);
                    string projectImagesPath = System.IO.Path.GetFullPath(@"..\..\Images");

                    string destinationPath = System.IO.Path.Combine(projectImagesPath, fileName);

                    // Копируем файл в папку Images
                    try
                    {
                        File.Copy(selectedFilePath, destinationPath, overwrite: true);
                        var nowUser = db.Users.FirstOrDefault(x => x.ID == Users1.ID);
                        nowUser.Photo = fileName;
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Устанавливаем изображение как Source для элемента Image
                    Iuser.Source = new BitmapImage(new Uri(destinationPath));
                }
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
