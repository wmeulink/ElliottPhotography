namespace ElliottPhotography.DTOs
{
    public class PhotoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; } = "No description provided.";
        public string? OriginalFileName { get; set; }  // matches PhotosController
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public string Thumbnail { get; set; } = string.Empty;
        public string Full { get; set; } = string.Empty;
    }
}