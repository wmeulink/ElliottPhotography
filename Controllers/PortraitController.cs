using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using ElliottPhotography.DTOs;
using ElliottPhotography.Models;
using Microsoft.EntityFrameworkCore;
using ElliottPhotography.Data;
using System.IO;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortraitsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _imagesRoot;

        public PortraitsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            // Root path for saving images in wwwroot/images/portraits
            _imagesRoot = Path.Combine(env.WebRootPath, "images", "portraits");
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "full"));
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "thumbs"));
        }

        // GET: api/Portraits
        [HttpGet]
        public async Task<IActionResult> GetAllPortraits()
        {
            var portraits = await _context.Portraits
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Select(p => new PortraitResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    Description = p.Description,
                    FileName = p.FileName,
                    UploadedAt = p.UploadedAt,
                    Thumbnail = p.ThumbnailPath,
                    Full = p.FullPath,
                    Tags = p.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(portraits);
        }

        // GET: api/Portraits/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("Category name is required.");

            var portraits = await _context.Portraits
                .Include(p => p.Category)
                .Include(p => p.Tags)
                .Where(p => p.Category != null &&
                            p.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(p => new PortraitResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    FileName = p.FileName,
                    UploadedAt = p.UploadedAt,
                    Thumbnail = p.ThumbnailPath,
                    Full = p.FullPath,
                    Tags = p.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(portraits);
        }

        // POST: api/Portraits/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortrait(
            [FromForm] IFormFile file,
            [FromForm] int categoryId,
            [FromForm] string title,
            [FromForm] string description,
            [FromForm] string tagNames) // comma-separated tags
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            // File paths
            var fullFileName = Path.Combine(_imagesRoot, "full", file.FileName);
            var thumbFileName = Path.Combine(_imagesRoot, "thumbs", file.FileName);

            // Save full-size file
            using (var fs = new FileStream(fullFileName, FileMode.Create))
                await file.CopyToAsync(fs);

            // Save thumbnail
            using (var image = Image.Load(file.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 0)
                }));
                await image.SaveAsJpegAsync(thumbFileName, new JpegEncoder { Quality = 70 });
            }

            var portrait = new Portrait
            {
                Title = title,
                UploadedAt = DateTime.UtcNow,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description,
                CategoryId = categoryId,
                FileName = file.FileName,
                FullPath = $"/images/portraits/full/{file.FileName}",
                ThumbnailPath = $"/images/portraits/thumbs/{file.FileName}"
            };

            // Handle tags
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

            return CreatedAtAction(nameof(GetAllPortraits), new { id = portrait.Id }, new
            {
                portrait.Id,
                portrait.Title
            });
        }
    }
}