using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
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
                    Description = p.Description,
                    FileName = p.FileName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    UploadedAt = p.UploadedAt,
                    Full = p.FullPath,
                    Thumbnail = p.ThumbnailPath,
                    Tags = p.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(portraits);
        }

        // GET: api/Portraits/{id}/thumb
        [HttpGet("{id}/thumb")]
        public async Task<IActionResult> GetThumbnail(int id)
        {
            var portrait = await _context.Portraits.FindAsync(id);
            if (portrait == null || string.IsNullOrEmpty(portrait.ThumbnailPath))
                return NotFound();

            // Map the ThumbnailPath (like /images/portraits/thumbs/IMG_1494.jpeg) to the filesystem
            var filePath = Path.Combine(_imagesRoot, "thumbs", Path.GetFileName(portrait.ThumbnailPath));
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            // Detect MIME type based on file extension
            var contentType = Path.GetExtension(filePath).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
        }

        // GET: api/Portraits/{id}/full
        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetFullImage(int id)
        {
            var portrait = await _context.Portraits.FindAsync(id);
            if (portrait == null || string.IsNullOrEmpty(portrait.FullPath))
                return NotFound();

            // Map the FullPath (like /images/portraits/full/IMG_1494.jpeg) to the filesystem
            var filePath = Path.Combine(_imagesRoot, "full", Path.GetFileName(portrait.FullPath));
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            // Detect MIME type based on file extension
            var contentType = Path.GetExtension(filePath).ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return PhysicalFile(filePath, contentType);
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
                    Description = p.Description,
                    FileName = p.FileName,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    UploadedAt = p.UploadedAt,
                    Full = p.FullPath,
                    Thumbnail = p.ThumbnailPath,
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
            [FromForm] string? description,
            [FromForm] string? tagNames)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var fullFileName = Path.Combine(_imagesRoot, "full", file.FileName);
            var thumbFileName = Path.Combine(_imagesRoot, "thumbs", file.FileName);

            using (var fs = new FileStream(fullFileName, FileMode.Create))
                await file.CopyToAsync(fs);

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
                Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description,
                CategoryId = categoryId,
                FileName = file.FileName,
                UploadedAt = DateTime.UtcNow,
                FullPath = $"/images/portraits/full/{file.FileName}",
                ThumbnailPath = $"/images/portraits/thumbs/{file.FileName}"
            };

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

            var dto = new PortraitResponseDto
            {
                Id = portrait.Id,
                Title = portrait.Title,
                Description = portrait.Description,
                FileName = portrait.FileName,
                CategoryId = portrait.CategoryId,
                CategoryName = portrait.Category?.Name ?? "Uncategorized",
                UploadedAt = portrait.UploadedAt,
                Full = portrait.FullPath,
                Thumbnail = portrait.ThumbnailPath,
                Tags = portrait.Tags.Select(t => t.Name).ToList()
            };

            return CreatedAtAction(nameof(GetPortraitById), new { id = portrait.Id }, dto);
        }
    }
}