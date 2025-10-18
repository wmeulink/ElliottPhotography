namespace ElliottPhotography.Models
{
    public class Photo
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; } = "No description provided.";

        public string? OriginalFileName { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public byte[] FullImage { get; set; } = Array.Empty<byte>();

        public byte[] ThumbnailImage { get; set; } = Array.Empty<byte>();
    }
}
