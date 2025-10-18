namespace ElliottPhotography.DTOs
{
    public class PhotoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Thumbnail { get; set; }
        public string Full { get; set; }
    }

}
