using Client.Models;
using Client.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Client.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Сервіс для обробки повідомлень чату.
        private readonly ChatService _chatService;

        // Сервіс для роботи із сервером.
        private readonly ServerService _serverService;

        // Повідомлення, що представляє користувача.
        private readonly ChatMessage _userMessage;

        // Колекція повідомлень для відображення в UI.
        public ObservableCollection<ChatMessage> Messages => _chatService.Messages;

        // Конструктор вікна, що ініціалізує сервіси та налаштовує початковий стан.
        public MainWindow(ChatMessage userMessage)
        {
            InitializeComponent();

            _serverService = new ServerService();
            _userMessage = userMessage;
            _chatService = new ChatService("http://localhost:5111/chatHub");

            DataContext = this; // Прив’язка даних до вікна.
            this.Title = $"Чат - {_userMessage.User}"; // Встановлення заголовка вікна.

            ConnectToChat(); // Підключення до чату.
        }

        // Асинхронне підключення до чату.
        private async void ConnectToChat()
        {
            await _chatService.ConnectAsync(_userMessage.User, _userMessage.ProfilePicture.UriSource.ToString());
        }

        // Обробник натискання кнопки для відправки текстового повідомлення.
        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            await _chatService.SendMessageAsync(_userMessage.User, messageBox.Text, _userMessage.ProfilePicture.UriSource.ToString());
            messageBox.Clear(); // Очищення текстового поля після відправки.
        }

        // Обробник натискання кнопки для відправки зображення.
        private async void SendImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageBox.Show("Виберіть файл перед завантаженням!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Завантаження зображення на сервер.
                var uploadResult = await _serverService.UploadImageAsync(_selectedImagePath);

                if (!uploadResult.IsSuccess)
                {
                    MessageBox.Show($"Помилка під час завантаження: {uploadResult.ErrorMessage}");
                    return;
                }

                using HttpClient client = new HttpClient();
                byte[] imageBytes = await client.GetByteArrayAsync(uploadResult.ImageUrl);

                // Зміна розміру та стиснення зображення.
                byte[] resizedImageBytes = ResizeAndCompressImage(imageBytes, 300, 300, 75);
                string base64Image = Convert.ToBase64String(resizedImageBytes);

                // Відправка зображення до чату.
                await _chatService.SendImageAsync(_userMessage.User, base64Image, _userMessage.ProfilePicture.UriSource.ToString());
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    User = "Система",
                    Content = $"Сталася помилка: {ex.Message}",
                    IsTextVisible = Visibility.Visible
                });
            }
        }

        // Змінна для збереження шляху до вибраного зображення.
        private string _selectedImagePath;

        // Обробник натискання кнопки для вибору зображення.
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName; // Збереження шляху до вибраного файлу.
            }
        }

        // Метод для зміни розміру та стиснення зображення.
        private byte[] ResizeAndCompressImage(byte[] byteArray, int maxWidth, int maxHeight, int quality)
        {
            using var memoryStream = new MemoryStream(byteArray);
            using var image = Image.Load(memoryStream);

            var aspectRatio = (double)image.Width / image.Height;
            int newWidth = maxWidth;
            int newHeight = maxHeight;

            if (image.Width > maxWidth || image.Height > maxHeight)
            {
                if (aspectRatio > 1)
                    newHeight = (int)(maxWidth / aspectRatio);
                else
                    newWidth = (int)(maxHeight * aspectRatio);
            }

            image.Mutate(x => x.Resize(newWidth, newHeight)); // Зміна розміру зображення.

            using var outputMemoryStream = new MemoryStream();
            var encoder = new JpegEncoder { Quality = quality }; // Встановлення якості стиснення.
            image.Save(outputMemoryStream, encoder);

            return outputMemoryStream.ToArray(); // Повернення зображення у вигляді байтового масиву.
        }

        // Обробник натискання кнопки для переходу до вікна входу.
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            StartWindow startWindow = new StartWindow();
            startWindow.Show();

            this.Close(); // Закриття поточного вікна.
        }
    }
}
