using System.Text.Json.Serialization;

namespace PTBackend2024.Models
{
    public class Like
    {
        [JsonIgnore]
        public int LikeId { get; set; }
        public int UserId { get; set; }
        public int PhotoId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Photo? Photo { get; set; }
    }
}
