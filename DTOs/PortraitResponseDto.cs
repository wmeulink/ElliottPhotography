namespace ElliottPhotography.DTOs
{
    public class PortraitResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string Thumbnail { get; set; }
        public string Full { get; set; }
        public string? Description { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
