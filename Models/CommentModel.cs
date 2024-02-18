using System.Collections.Generic;

namespace BetaManager.Models
{
    public class CommentModel
    {
        public string ID { get; set; } = Functions.GenerateRandomString(12);
        public string UserID { get; set; } = Saved.User.ID;
        public string Comment { get; set; }
        public long Date { get; set; }
        public List<LikesModel> Likes { get; set; }
    }

    public class LikesModel
    {
        public string UserID { get; set; }
        public long Date { get; set; }
    }
}
