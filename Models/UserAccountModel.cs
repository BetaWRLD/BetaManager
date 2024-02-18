using System.Drawing;

namespace BetaManager.Models
{
    public class UserAccountModel
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public Bitmap ProfilePicture { get; set; }
    }
}
