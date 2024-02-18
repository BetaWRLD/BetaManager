using System.Security;
using System.Windows.Media.Imaging;

namespace BetaManager.Models
{
    public class UserModel
    {
        public string ID { get; set; }
        public string Rank { get; set; }
        public string Username { get; set; }
        public SecureString Password { get; set; }
        public string HWID { get; set; }
        public int Tokens { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public int Downloads { get; set; }
        public int Ratings { get; set; }
        public BitmapSource ProfilePicture { get; set; }
        public string ProfilePicturePath { get; set; }
        public string Date { get; set; }
        public UserJSON JSON { get; set; }
        public bool Verified { get; set; }
        public bool Guest { get; set; }
    }
}
