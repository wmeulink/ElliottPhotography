using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ElliottPhotography.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        // Navigation property
        public ICollection<Portrait> Portraits { get; set; } = new List<Portrait>();
        public ICollection<Landscape> Landscapes { get; set; } = new List<Landscape>();
    }
}
