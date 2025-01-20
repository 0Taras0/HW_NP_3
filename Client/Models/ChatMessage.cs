using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Client.Models
{
    public class ChatMessage
    {
        // Ім'я користувача, який надіслав повідомлення.
        public string User { get; set; } = String.Empty;

        // Текст повідомлення.
        public string Content { get; set; } = String.Empty;

        // Зображення профілю користувача.
        public BitmapImage? ProfilePicture { get; set; }

        // Додане зображення в повідомленні.
        public BitmapImage? ImageContent { get; set; }

        // Видимість тексту (за замовчуванням приховано).
        public Visibility IsTextVisible { get; set; } = Visibility.Collapsed;

        // Видимість зображення (за замовчуванням приховано).
        public Visibility IsImageVisible { get; set; } = Visibility.Collapsed;
    }
}
