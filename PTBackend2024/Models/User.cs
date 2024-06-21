using System.Text.Json.Serialization;

namespace PTBackend2024.Models
{
    public class User
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        [JsonIgnore]
        public string? ProfilePhotoUri { get; set; }
        [JsonIgnore]
        public ICollection<Photo>? Photos { get; set; }
        [JsonIgnore]
        public ICollection<Like>? Likes { get; set; }
        [JsonIgnore]
        public ICollection<Comment>? Comments { get; set; }

        public User()
        {
            Photos = new List<Photo>();
            Likes = new List<Like>();
            Comments = new List<Comment>();
        }
    }
}
