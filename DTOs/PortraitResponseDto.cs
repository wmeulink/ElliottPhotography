namespace ElliottPhotography.DTOs
{
    public class PortraitResponseDto
    {
        public int Id { get; set; }                       // Portrait ID
        public string Title { get; set; }                 // Portrait title
        public int CategoryId { get; set; }              // FK for category
        public string CategoryName { get; set; }         // Category name
        public string? Description { get; set; }         // Description of the portrait
        public string? FileName { get; set; }            // Original file name
        public byte[]? Thumbnail { get; set; }           // Thumbnail image bytes
        public byte[]? Full { get; set; }                // Full image bytes
        public List<string>? Tags { get; set; } = new(); // List of tag names
        public DateTime UploadedAt { get; set; }         // Date/time the portrait was uploaded
    }
}
