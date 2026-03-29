using System;
using System.IO;
using System.Linq;
using ElliottPhotography.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ElliottPhotography.Data
{
    public static class DbSeeder
    {
        public static void MigrateOldImagesToPaths(AppDbContext context, string contentRootPath)
        {
            Console.WriteLine("Migrating old images to path-based storage...");

            // Directories for new images
            var fullDir = Path.Combine(contentRootPath, "wwwroot", "images", "full");
            var thumbDir = Path.Combine(contentRootPath, "wwwroot", "images", "thumbs");
            Directory.CreateDirectory(fullDir);
            Directory.CreateDirectory(thumbDir);

            void ProcessFolder(string oldFolder, bool isPortrait, int categoryId)
            {
                if (!Directory.Exists(oldFolder))
                    return;

                var files = Directory.GetFiles(oldFolder);
                Console.WriteLine($"Found {files.Length} files in {oldFolder}");

                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileName(filePath);

                    // Skip if already in DB
                    bool exists = isPortrait
                        ? context.Portraits.Any(p => p.FileName == fileName)
                        : context.Landscapes.Any(l => l.FileName == fileName);
                    if (exists) continue;

                    try
                    {
                        // Copy full image
                        var fullDest = Path.Combine(fullDir, fileName);
                        File.Copy(filePath, fullDest, overwrite: true);

                        // Generate thumbnail
                        var thumbDest = Path.Combine(thumbDir, fileName);
                        using var image = Image.Load<Rgba32>(filePath); // strong typing
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(400, 0)
                        }));
                        image.SaveAsJpeg(thumbDest, new JpegEncoder { Quality = 70 });

                        if (isPortrait)
                        {
                            var portrait = new Portrait
                            {
                                Title = Path.GetFileNameWithoutExtension(fileName),
                                FileName = fileName,
                                UploadedAt = DateTime.UtcNow,
                                CategoryId = categoryId,
                                FullPath = $"/images/full/{fileName}",
                                ThumbnailPath = $"/images/thumbs/{fileName}"
                            };
                            context.Portraits.Add(portrait);
                        }
                        else
                        {
                            var landscape = new Landscape
                            {
                                Title = Path.GetFileNameWithoutExtension(fileName),
                                FileName = fileName,
                                UploadedAt = DateTime.UtcNow,
                                CategoryId = categoryId,
                                FullPath = $"/images/full/{fileName}",
                                ThumbnailPath = $"/images/thumbs/{fileName}"
                            };
                            context.Landscapes.Add(landscape);
                        }

                        context.SaveChanges();
                        Console.WriteLine($"Added {(isPortrait ? "portrait" : "landscape")}: {fileName}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process {fileName}: {ex.Message}");
                    }
                }
            }

            // Get category IDs
            var landscapesCategory = context.Categories.FirstOrDefault(c => c.Name == "Landscape")?.Id ?? 0;
            var portraitsCategory = context.Categories.FirstOrDefault(c => c.Name == "Portrait")?.Id ?? 0;

            // Migrate folders
            ProcessFolder(Path.Combine(contentRootPath, "OldImages", "Landscapes"), false, landscapesCategory);
            ProcessFolder(Path.Combine(contentRootPath, "OldImages", "Portraits"), true, portraitsCategory);

            Console.WriteLine("Migration complete!");
        }
    }
}