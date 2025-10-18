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

        public PortraitsController(AppDbContext context)
        {
            _context = context;
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
                    Thumbnail = p.ThumbnailData,
                    Full = p.ImageData,
                    Tags = p.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(portraits);
        }

        // GET: api/Portraits/{id}/full
        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetFullImage(int id)
        {
            var portrait = await _context.Portraits.FindAsync(id);
            if (portrait == null || portrait.ImageData == null)
                return NotFound();

            return File(portrait.ImageData, "image/jpeg");
        }

        // GET: api/Portraits/{id}/thumb
        [HttpGet("{id}/thumb")]
        public async Task<IActionResult> GetThumbnail(int id)
        {
            var portrait = await _context.Portraits.FindAsync(id);
            if (portrait == null || portrait.ThumbnailData == null)
                return NotFound();

            return File(portrait.ThumbnailData, "image/jpeg");
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

            byte[] fullBytes;
            byte[] thumbBytes;

            using (var image = Image.Load(file.OpenReadStream()))
            {
                // Full image
                using var msFull = new MemoryStream();
                image.Save(msFull, new JpegEncoder { Quality = 90 });
                fullBytes = msFull.ToArray();

                // Thumbnail
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 0)
                }));
                using var msThumb = new MemoryStream();
                image.Save(msThumb, new JpegEncoder { Quality = 70 });
                thumbBytes = msThumb.ToArray();
            }

            var portrait = new Portrait
            {
                Title = title,
                UploadedAt = DateTime.UtcNow,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description,
                CategoryId = categoryId,
                FileName = file.FileName,
                ImageData = fullBytes,
                ThumbnailData = thumbBytes
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
                            p.Category.Name != null &&
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
                    Thumbnail = p.ThumbnailData,
                    Full = p.ImageData,
                    Tags = p.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(portraits);
        }
    }
}