using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElliottPhotography.Models
{
    public class Portrait
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Title { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
        // New binary data
        public byte[] ImageData { get; set; }  // Full size
        public byte[] ThumbnailData { get; set; }  // Smaller version
    }

}