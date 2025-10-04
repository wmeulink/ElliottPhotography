namespace ElliottPhotography.Models
{
    public class Landscape
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Description { get; set; }

        // Category relationship
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
