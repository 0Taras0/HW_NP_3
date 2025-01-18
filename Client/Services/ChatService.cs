using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Threading;

namespace Client.Services
{
    public class ChatService
    {
        private readonly HubConnection _hubConnection;

        public ObservableCollection<ChatMessage> Messages { get; } = new ObservableCollection<ChatMessage>();

        public ChatService(string hubUrl)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<string, string, string>("ReceiveMessage", (user, message, profilePictureUrl) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new ChatMessage
                    {
                        User = user,
                        Content = message,
                        ProfilePicture = new BitmapImage(new Uri(profilePictureUrl)),
                        IsTextVisible = Visibility.Visible
                    });
                });
            });

            _hubConnection.On<string, string, string>("ReceiveImage", (user, base64Image, profilePictureUrl) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var image = ConvertBase64ToBitmapImage(base64Image);
                    Messages.Add(new ChatMessage
                    {
                        User = user,
                        ProfilePicture = new BitmapImage(new Uri(profilePictureUrl)),
                        ImageContent = image,
                        IsImageVisible = Visibility.Visible
                    });
                });
            });
        }

        public async Task ConnectAsync(string user, string profilePictureUrl)
        {
            try
            {
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("SendMessage", user, "Підключено до сервера SignalR.", profilePictureUrl);
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    User = "Система",
                    Content = $"Помилка підключення: {ex.Message}",
                    IsTextVisible = Visibility.Visible
                });
            }
        }

        public async Task SendMessageAsync(string user, string message, string profilePictureUrl)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("SendMessage", user, message, profilePictureUrl);
                }
                catch (Exception ex)
                {
                    Messages.Add(new ChatMessage
                    {
                        User = "Система",
                        Content = $"Помилка відправки: {ex.Message}",
                        IsTextVisible = Visibility.Visible
                    });
                }
            }
            else
            {
                Messages.Add(new ChatMessage
                {
                    User = "Система",
                    Content = "Не підключено до сервера.",
                    IsTextVisible = Visibility.Visible
                });
            }
        }

        public async Task SendImageAsync(string user, string base64Image, string profilePictureUrl)
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("SendImage", user, base64Image, profilePictureUrl);
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
            else
            {
                Messages.Add(new ChatMessage
                {
                    User = "Система",
                    Content = "Не підключено до сервера.",
                    IsTextVisible = Visibility.Visible
                });
            }
        }

        private BitmapImage ConvertBase64ToBitmapImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            var bitmap = new BitmapImage();
            using (var stream = new MemoryStream(imageBytes))
            {
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
            }
            return bitmap;
        }
    }
}
