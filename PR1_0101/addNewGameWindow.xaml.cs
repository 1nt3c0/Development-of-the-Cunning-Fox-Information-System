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
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;

namespace PR1_0101
{
    /// <summary>
    /// Логика взаимодействия для addNewGameWindow.xaml
    /// </summary>
    public partial class addNewGameWindow : Window
    {
        Entities db = new Entities();
        string FileName;
        public addNewGameWindow()
        {
            InitializeComponent();
            CBgenre.ItemsSource = db.Genre.ToList();
            CBcategory.ItemsSource = db.Category.ToList();
            CBthematics.ItemsSource = db.Thematics.ToList();
            CBageLimit.ItemsSource = db.BoardGames.Select(x => x.AgeLimit).Distinct().ToList();
        }
       
        private void Image_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Обработка клика
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.gif;*.bmp)|*.png;*.jpeg;*.jpg;*.gif;*.bmp|All files (*.*)|*.*";
                openFileDialog.Title = "Выберите изображение";
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
                        FileName = fileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при копировании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Устанавливаем изображение как Source для элемента Image
                    Iphoto.Source = new BitmapImage(new Uri(destinationPath));
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
                if(TBnameGame.Text == "")
                {
                    MessageBox.Show("Напишите название игры");
                    return;
                }
                if(CBcategory.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите категорию игры");
                    return;
                }
                if(CBgenre.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите жанр игры");
                    return;
                }
                if (CBthematics.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите тематику игры");
                    return;
                }
                if (CBageLimit.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите возрастное ограничение игры");
                    return;
                }
                if (TBdescription.Text == "")
                {
                    MessageBox.Show("Напишите описание игры");
                    return;
                }
                if (Iphoto.Source == null)
                {
                    MessageBox.Show("Укажите фото игры");
                    return;
                }
                if (TBcost.Text == "")
                {
                    MessageBox.Show("Укажите цену у игры");
                    return;
                }


                if (db.BoardGames.Any(x => x.NameGame == TBnameGame.Text))
                {
                    MessageBox.Show("Такая игра уже есть!!!");
                    return;
                }
                var ex = 0;
                if (CBexclusivity.IsChecked == true)
                {
                    ex = 1;
                }
                var newGame = new BoardGames
                {
                    NameGame = TBnameGame.Text,
                    Category = (CBcategory.SelectedItem as Category).ID,
                    Genre = (CBgenre.SelectedItem as Genre).ID,
                    Thematics = (CBthematics.SelectedItem as Thematics).ID,
                    AgeLimit = Convert.ToInt32(CBageLimit.SelectedItem.ToString()),
                    Description = TBdescription.Text,
                    Price = Convert.ToInt32(TBcost.Text),
                    Exclusivity = Convert.ToBoolean(ex),
                    Photo = "/images/" + FileName
                };
                db.BoardGames.Add(newGame);
                db.SaveChanges();
                MessageBox.Show("Игра добавлена");
                DialogResult = true;
            }
            catch
            {
                MessageBox.Show("Ошибка", "Глобальная", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
