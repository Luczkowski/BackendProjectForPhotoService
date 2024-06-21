namespace PTBackend2024.Models
{
    public class PhotoDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile File { get; set; }
    }
}
