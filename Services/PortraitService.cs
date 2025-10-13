using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.EntityFrameworkCore;

namespace ElliottPhotography.Services
{
    public class PortraitService
    {
        private readonly AppDbContext _context;

        public PortraitService(AppDbContext context)
        {
            _context = context;
        }

        // Method to add tags to an existing portrait
        public async Task AddTagsToPortraitAsync(string portraitTitle, List<string> tagNames)
        {
            // find the portrait
            var portrait = await _context.Portraits
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Title == portraitTitle);

            if (portrait == null)
                throw new Exception($"Portrait with title '{portraitTitle}' not found.");

            foreach (var tagName in tagNames)
            {
                // check if the tag exists or create it
                var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                if (existingTag == null)
                {
                    existingTag = new Tag { Name = tagName };
                    _context.Tags.Add(existingTag);
                }

                // add the tag if not already linked
                if (!portrait.Tags.Any(t => t.Name == tagName))
                    portrait.Tags.Add(existingTag);
            }

            await _context.SaveChangesAsync();
        }
    }
}
