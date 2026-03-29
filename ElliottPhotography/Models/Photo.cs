namespace ElliottPhotography.Models
{
    public class Photo
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = "No description provided.";

        public string? OriginalFileName { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public string FullPath { get; set; } = string.Empty;

        public string ThumbnailPath { get; set; } = string.Empty;
    }
}
