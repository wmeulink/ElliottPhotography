namespace ElliottPhotography.Models
{
    public class Landscape
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; }  // ✅ Enum instead of string
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
