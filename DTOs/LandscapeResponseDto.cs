namespace ElliottPhotography.DTOs
{
    public class LandscapeResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public byte[]? Thumbnail { get; set; }
        public byte[]? Full { get; set; }
        public DateTime UploadedAt { get; set; }
        public List<string>? Tags { get; set; } = new List<string>();
    }
}

