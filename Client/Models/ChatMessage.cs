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
        public string User { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public BitmapImage? ProfilePicture { get; set; }
        public BitmapImage? ImageContent { get; set; }
        public Visibility IsTextVisible { get; set; } = Visibility.Collapsed;
        public Visibility IsImageVisible { get; set; } = Visibility.Collapsed;
    }
}
