using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using ElliottPhotography.DTOs;
using ElliottPhotography.Models;
using Microsoft.EntityFrameworkCore;
using ElliottPhotography.Data;

namespace YourAppNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortraitsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PortraitsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Portraits
        [HttpGet]
        public IActionResult GetAllPortraits()
        {
            var portraits = _context.Portraits
                .Include(p => p.Category)
                .Select(p => new PortraitResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/portraits/thumbs/{p.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/portraits/full/{p.FileName}",
                    FileName = p.FileName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    Description = p.Description
                })
                .ToList();

            return Ok(portraits);
        }

        // GET: api/Portraits/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        // ✅ POST: api/Portraits
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortrait(
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
            var uploadsFull = Path.Combine(_env.WebRootPath, "portraits", "full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "portraits", "thumbs");
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

            // Create Portrait
            var portrait = new Portrait
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
                    if (!portrait.Tags.Any(t => t.Name.ToLower() == existingTag.Name.ToLower()))
                        portrait.Tags.Add(existingTag);
                }
            }

            _context.Portraits.Add(portrait);
            await _context.SaveChangesAsync();

            var categoryName = _context.Categories
                .FirstOrDefault(c => c.Id == portrait.CategoryId)?.Name ?? "Uncategorized";

            // Return the full response DTO
            return CreatedAtAction(nameof(UploadPortrait), new PortraitResponseDto
            {
                Id = portrait.Id,
                Title = portrait.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/portraits/thumbs/{portrait.FileName}",
                Full = $"{Request.Scheme}://{Request.Host}/portraits/full/{portrait.FileName}",
                FileName = portrait.FileName,
                CategoryId = portrait.CategoryId,
                CategoryName = categoryName,
                Description = portrait.Description
            });
        }


        [HttpGet("category/{category}")]
        public IActionResult GetByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("Category name is required.");

            var portraits = _context.Portraits
                .Include(p => p.Category)
                .AsEnumerable() // ← forces query to run in memory, allowing string comparison safely
                .Where(p => p.Category != null &&
                            p.Category.Name != null &&
                            p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(p => new PortraitResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/portraits/thumbs/{p.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/portraits/full/{p.FileName}",
                    FileName = p.FileName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Description = p.Description
                })
                .ToList();

            return Ok(portraits);
        }

    }

}
