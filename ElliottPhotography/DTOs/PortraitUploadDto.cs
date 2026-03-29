using System.ComponentModel.DataAnnotations;

namespace ElliottPhotography.DTOs
{
    public class PortraitUploadDto
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string FileName { get; set; }

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}
