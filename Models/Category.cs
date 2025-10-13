using System.ComponentModel.DataAnnotations;

namespace ElliottPhotography.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Portrait> Portraits { get; set; }
        public ICollection<Landscape> Landscapes { get; set; }
    }

}
