using Client.Models;
using Microsoft.Win32;
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

namespace Client.Windows
{
    /// <summary>
    /// Interaction logic for StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Відкрити діалогове вікно вибору файлу
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Встановити шлях до файлу в TextBox
                avatarPathTB.Text = openFileDialog.FileName;
            }
        }

        // Обробник події для кнопки "Join"
        private void joinBT_Click(object sender, RoutedEventArgs e)
        {
            string userName = userNameTB.Text;
            string avatarPath = avatarPathTB.Text;

            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Будь ласка, введіть ваше ім'я в чаті.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(avatarPath))
            {
                MessageBox.Show("Будь ласка, завантажте аватар.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Створення екземпляра ChatMessage
                ChatMessage chatMessage = new ChatMessage
                {
                    User = userName,
                    Content = string.Empty,
                    ProfilePicture = new BitmapImage(new Uri(avatarPath, UriKind.Absolute)),
                    ImageContent = null,
                    IsTextVisible = Visibility.Collapsed,
                    IsImageVisible = Visibility.Collapsed
                };

                // Відкриття нового вікна MainWindow
                MainWindow mainWindow = new MainWindow(chatMessage);
                mainWindow.Show();

                // Закриття поточного вікна
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
