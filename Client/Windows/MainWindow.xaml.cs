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
        private readonly ChatService _chatService;
        private readonly ServerService _serverService;
        private readonly ChatMessage _userMessage;

        public ObservableCollection<ChatMessage> Messages => _chatService.Messages;

        public MainWindow(ChatMessage userMessage)
        {
            InitializeComponent();

            _serverService = new ServerService();
            _userMessage = userMessage;
            _chatService = new ChatService("http://localhost:5111/chatHub");

            DataContext = this;
            this.Title = $"Чат - {_userMessage.User}";

            ConnectToChat();
        }

        private async void ConnectToChat()
        {
            await _chatService.ConnectAsync(_userMessage.User, _userMessage.ProfilePicture.UriSource.ToString());
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            await _chatService.SendMessageAsync(_userMessage.User, messageBox.Text, _userMessage.ProfilePicture.UriSource.ToString());
            messageBox.Clear();
        }

        private async void SendImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedImagePath))
            {
                MessageBox.Show("Виберіть файл перед завантаженням!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var uploadResult = await _serverService.UploadImageAsync(_selectedImagePath);

                if (!uploadResult.IsSuccess)
                {
                    MessageBox.Show($"Помилка під час завантаження: {uploadResult.ErrorMessage}");
                    return;
                }

                using HttpClient client = new HttpClient();
                byte[] imageBytes = await client.GetByteArrayAsync(uploadResult.ImageUrl);

                byte[] resizedImageBytes = ResizeAndCompressImage(imageBytes, 300, 300, 75);
                string base64Image = Convert.ToBase64String(resizedImageBytes);

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

        private string _selectedImagePath;

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
            }
        }

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

            image.Mutate(x => x.Resize(newWidth, newHeight));

            using var outputMemoryStream = new MemoryStream();
            var encoder = new JpegEncoder { Quality = quality };
            image.Save(outputMemoryStream, encoder);

            return outputMemoryStream.ToArray();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            StartWindow startWindow = new StartWindow();
            startWindow.Show();

            this.Close();
        }
    }
}
