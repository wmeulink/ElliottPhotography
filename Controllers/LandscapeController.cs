using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandscapesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LandscapesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Landscapes
        [HttpGet]
        public IActionResult GetAllLandscapes()
        {
            var landscapes = _context.Landscapes
                .Include(l => l.Category)
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{l.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{l.FileName}",
                    FileName = l.FileName,
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category.Name,
                    Description = l.Description
                })
                .ToList();

            return Ok(landscapes);
        }

        // GET: api/Landscapes/categories
        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = _context.Categories
                .Select(c => new { c.Id, c.Name })
                .ToList();

            return Ok(categories);
        }

        // GET: api/Landscapes/by-category/{categoryName}
        [HttpGet("category/{categoryName}")]
        public IActionResult GetLandscapesByCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return BadRequest(new { error = "Category name cannot be empty." });

            var landscapes = _context.Landscapes
                .Include(l => l.Category)
                .Where(l => l.Category.Name.ToLower() == categoryName.ToLower())
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{l.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{l.FileName}",
                    FileName = l.FileName,
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category.Name,
                    Description = l.Description
                })
                .ToList();

            // Always return 200 with an array, even if empty
            return Ok(landscapes);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadLandscape(
            [FromForm] IFormFile file,
            [FromForm] int categoryId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string tagNames // comma-separated tags
        )
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // Prepare folders
            var uploadsFull = Path.Combine(_env.WebRootPath, "images", "full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "images", "thumbnails");
            Directory.CreateDirectory(uploadsFull);
            Directory.CreateDirectory(uploadsThumb);

            // Normalize extension to .jpg
            var fileName = Path.GetFileNameWithoutExtension(file.FileName) + ".jpg";
            var fullPath = Path.Combine(uploadsFull, fileName);
            var thumbPath = Path.Combine(uploadsThumb, fileName);

            // Save full image as JPG
            using (var image = Image.Load(file.OpenReadStream()))
            {
                image.Save(fullPath, new JpegEncoder { Quality = 90 });
            }

            // Create thumbnail if missing
            if (!System.IO.File.Exists(thumbPath))
            {
                using var thumbImage = Image.Load(fullPath);
                thumbImage.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 0)
                }));
                thumbImage.Save(thumbPath, new JpegEncoder { Quality = 70 });
            }

            // Create Landscape
            var landscape = new Landscape
            {
                Title = title,
                FileName = fileName,
                UploadedAt = DateTime.UtcNow,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description,
                CategoryId = categoryId
            };

            // Handle multiple tags
            if (!string.IsNullOrWhiteSpace(tagNames))
            {
                var tagsArray = tagNames.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                foreach (var tagName in tagsArray)
                {
                    var existingTag = await _context.Tags.FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower());
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Name = tagName };
                        _context.Tags.Add(existingTag);
                        await _context.SaveChangesAsync();
                    }
                    if (!landscape.Tags.Any(t => t.Name.ToLower() == existingTag.Name.ToLower()))
                        landscape.Tags.Add(existingTag);
                }
            }

            _context.Landscapes.Add(landscape);
            await _context.SaveChangesAsync();

            var categoryName = _context.Categories
                .FirstOrDefault(c => c.Id == landscape.CategoryId)?.Name ?? "Uncategorized";

            // Return the full response DTO
            return CreatedAtAction(nameof(UploadLandscape), new LandscapeResponseDto
            {
                Id = landscape.Id,
                Title = landscape.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{landscape.FileName}",
                Full = $"{Request.Scheme}://{Request.Host}/images/full/{landscape.FileName}",
                FileName = landscape.FileName,
                CategoryId = landscape.CategoryId,
                CategoryName = categoryName,
                Description = landscape.Description
            });
        }

    }
}
