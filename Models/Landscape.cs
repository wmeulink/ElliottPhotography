using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElliottPhotography.Models
{
    public class Landscape
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // rename to match Portrait
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public byte[] ThumbnailData { get; set; } = Array.Empty<byte>();

        public string? FileName { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
