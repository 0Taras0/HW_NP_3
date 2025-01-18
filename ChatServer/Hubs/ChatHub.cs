using Microsoft.AspNetCore.SignalR;

namespace ChatServer.Hubs
{
    public class ChatHub : Hub
    {
        // Відправка текстового повідомлення
        public async Task SendMessage(string user, string message, string profilePictureUrl)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, profilePictureUrl);
        }

        // Відправка зображення
        public async Task SendImage(string user, string base64Image, string profilePictureUrl)
        {
            await Clients.All.SendAsync("ReceiveImage", user, base64Image, profilePictureUrl);
        }
    }
}
