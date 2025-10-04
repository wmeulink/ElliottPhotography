using System.ComponentModel.DataAnnotations;

namespace ElliottPhotography.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Landscape> Landscapes { get; set; }
    }
}
