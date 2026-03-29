namespace ElliottPhotography.DTOs
{
    public class PhotoUploadDto
    {
        public string Title { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string? Description { get; set; } = "No description provided."; // optional
    }
}