using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace PTBackend2024.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public string FileName { get; set; }
        public string Uri { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public ICollection<Like>? Likes { get; set; }
        [JsonIgnore]
        public ICollection<Comment>? Comments { get; set; }

        public Photo()
        {
            Likes = new List<Like>();
            Comments = new List<Comment>();
        }
    }
}
