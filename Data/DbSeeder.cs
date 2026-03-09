using System;
using System.IO;
using System.Linq;
using ElliottPhotography.Models;
using Microsoft.EntityFrameworkCore;

namespace ElliottPhotography.Data
{
    public static class DbSeeder
    {
        public static void SeedImages(AppDbContext context, string contentRootPath)
        {
            Console.WriteLine("Starting database seeding...");

            // Ensure database is up-to-date
            context.Database.Migrate();

            // Ensure categories exist
            var landscapeCategory = context.Categories.FirstOrDefault(c => c.Name == "Landscape")
                                    ?? context.Categories.Add(new Category { Name = "Landscape" }).Entity;
            context.SaveChanges();

            var portraitCategory = context.Categories.FirstOrDefault(c => c.Name == "Portrait")
                                   ?? context.Categories.Add(new Category { Name = "Portrait" }).Entity;
            context.SaveChanges();

            // Seed Landscapes
            var landscapeDir = Path.Combine(contentRootPath, "wwwroot", "images", "full");
            if (Directory.Exists(landscapeDir))
            {
                var files = Directory.GetFiles(landscapeDir);
                Console.WriteLine($"Found {files.Length} landscape files.");

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);

                    if (context.Landscapes.Any(l => l.FileName == fileName))
                        continue;

                    try
                    {
                        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        var imageBytes = new byte[fs.Length];
                        fs.Read(imageBytes, 0, imageBytes.Length);

                        var thumbnailBytes = GenerateThumbnail(imageBytes);

                        context.Landscapes.Add(new Landscape
                        {
                            Title = Path.GetFileNameWithoutExtension(fileName),
                            FileName = fileName,
                            UploadedAt = DateTime.UtcNow,
                            CategoryId = landscapeCategory.Id,
                            ImageData = imageBytes,
                            ThumbnailData = thumbnailBytes
                        });

                        // Save each image immediately to reduce memory usage
                        context.SaveChanges();

                        Console.WriteLine($"Added landscape: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to add {fileName}: {ex.Message}");
                    }
                }
            }

            // Seed Portraits
            var portraitDir = Path.Combine(contentRootPath, "wwwroot", "portraits", "full");
            if (Directory.Exists(portraitDir))
            {
                var files = Directory.GetFiles(portraitDir);
                Console.WriteLine($"Found {files.Length} portrait files.");

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);

                    if (context.Portraits.Any(p => p.FileName == fileName))
                        continue;

                    try
                    {
                        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                        var imageBytes = new byte[fs.Length];
                        fs.Read(imageBytes, 0, imageBytes.Length);

                        var thumbnailBytes = GenerateThumbnail(imageBytes);

                        context.Portraits.Add(new Portrait
                        {
                            Title = Path.GetFileNameWithoutExtension(fileName),
                            FileName = fileName,
                            UploadedAt = DateTime.UtcNow,
                            CategoryId = portraitCategory.Id,
                            ImageData = imageBytes,
                            ThumbnailData = thumbnailBytes
                        });

                        context.SaveChanges();

                        Console.WriteLine($"Added portrait: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to add {fileName}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("Database seeding complete!");
        }

        private static byte[] GenerateThumbnail(byte[] imageBytes)
        {
            // For now just returning the original bytes.
            return imageBytes;
        }
    }
}