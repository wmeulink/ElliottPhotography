using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.EntityFrameworkCore;

namespace ElliottPhotography.Services
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.Migrate();

            // --- Categories ---
            var categoryNames = new List<string> { "Portrait", "Landscape", "Nature", "Urban" };
            foreach (var name in categoryNames)
            {
                if (!context.Categories.Any(c => c.Name == name))
                {
                    context.Categories.Add(new Category { Name = name });
                }
            }
            context.SaveChanges();

            // --- Tags ---
            var tagNames = new List<string>
            {
                "Sunset", "Dog", "Coast", "Sunrise", "Car",
                "Flowers", "Foggy", "Cat", "River", "Pickup",
                "Barn", "Snow", "House", "Autumn", "Trees",
                "Lake", "Waterfall", "Golden Hour", "Mountains"
            };

            foreach (var name in tagNames)
            {
                if (!context.Tags.Any(t => t.Name == name))
                {
                    context.Tags.Add(new Tag { Name = name });
                }
            }
            context.SaveChanges();

            // --- Portraits Data ---
            var portraitsData = new List<(string Title, string FileName, string Description, DateTime UploadedAt, string CategoryName, List<string> Tags)>
            {
                ("Waffles at the river", "IMG_4024.JPEG", "Waffles at Cottonwood Beach", new DateTime(2025,10,09,03,47,29), "Portrait", new List<string> { "Dog" }),
                ("Scout", "IMG_1586.JPG", "Scout the blue gray cat", new DateTime(2025,10,06,03,43,43), "Portrait", new List<string> { "Cat" })
                // Add more Portraits here...
            };

            foreach (var (Title, FileName, Description, UploadedAt, CategoryName, Tags) in portraitsData)
            {
                SeedOrUpdatePortrait(context, Title, FileName, Description, UploadedAt, CategoryName, Tags);
            }

            // --- Landscapes Data ---
            var landscapesData = new List<(string Title, string FileName, string Description, DateTime UploadedAt, string CategoryName, List<string> Tags)>
            {
                ("Snowy Silverfalls", "IMG_3293.JPEG", "Waterfall at Silver Falls, Silverton Oregon", new DateTime(2025,10,09,03,54,06), "Landscape", new List<string> { "Snow", "Waterfall" }),
                ("Seaside Oregon", "IMG_2222.jpg", "Seaside Oregon, sunset and swingset", new DateTime(2025,10,08,18,42,20), "Landscape", new List<string> { "Sunset", "Coast" })
                // Add more Landscapes here...
            };

            foreach (var (Title, FileName, Description, UploadedAt, CategoryName, Tags) in landscapesData)
            {
                SeedOrUpdateLandscape(context, Title, FileName, Description, UploadedAt, CategoryName, Tags);
            }
        }

        // --- Portrait seeding/updating ---
        private static void SeedOrUpdatePortrait(AppDbContext context, string title, string fileName, string description, DateTime uploadedAt, string categoryName, List<string> desiredTags)
        {
            var portrait = context.Portraits
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.Title == title);

            var category = context.Categories.First(c => c.Name == categoryName);

            if (portrait == null)
            {
                portrait = new Portrait
                {
                    Title = title,
                    FileName = fileName,
                    Description = description,
                    UploadedAt = uploadedAt,
                    CategoryId = category.Id
                };
                context.Portraits.Add(portrait);
            }
            else
            {
                portrait.FileName = fileName;
                portrait.Description = description;
                portrait.UploadedAt = uploadedAt;
                portrait.CategoryId = category.Id;
            }

            SyncTags(context, portrait.Tags, desiredTags);

            context.SaveChanges();
        }

        // --- Landscape seeding/updating ---
        private static void SeedOrUpdateLandscape(AppDbContext context, string title, string fileName, string description, DateTime uploadedAt, string categoryName, List<string> desiredTags)
        {
            var landscape = context.Landscapes
                .Include(l => l.Tags)
                .FirstOrDefault(l => l.Title == title);

            var category = context.Categories.First(c => c.Name == categoryName);

            if (landscape == null)
            {
                landscape = new Landscape
                {
                    Title = title,
                    FileName = fileName,
                    Description = description,
                    UploadedAt = uploadedAt,
                    CategoryId = category.Id
                };
                context.Landscapes.Add(landscape);
            }
            else
            {
                landscape.FileName = fileName;
                landscape.Description = description;
                landscape.UploadedAt = uploadedAt;
                landscape.CategoryId = category.Id;
            }

            SyncTags(context, landscape.Tags, desiredTags);

            context.SaveChanges();
        }

        // --- Sync tags helper ---
        private static void SyncTags(AppDbContext context, ICollection<Tag> currentTags, List<string> desiredTagNames)
        {
            // Remove tags no longer desired
            var tagsToRemove = currentTags.Where(t => !desiredTagNames.Contains(t.Name)).ToList();
            foreach (var t in tagsToRemove)
                currentTags.Remove(t);

            // Add tags that are missing
            foreach (var tagName in desiredTagNames)
            {
                if (!currentTags.Any(t => t.Name == tagName))
                {
                    var tag = context.Tags.FirstOrDefault(t => t.Name == tagName) ?? new Tag { Name = tagName };
                    currentTags.Add(tag);
                }
            }
        }
    }
}
