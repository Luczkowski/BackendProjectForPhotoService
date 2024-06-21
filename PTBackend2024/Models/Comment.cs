using System.Text.Json.Serialization;

namespace PTBackend2024.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public DateTime CreadesDate { get; set; }
        public int UserId { get; set; }
        public int PhotoId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Photo? Photo { get; set; }
    }
}
