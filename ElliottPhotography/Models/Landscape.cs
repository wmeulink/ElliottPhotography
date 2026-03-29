using ElliottPhotography.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Landscape
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // file info
    public string? FileName { get; set; }

    public string FullPath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}