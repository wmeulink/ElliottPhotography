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
    public class LandscapesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _imagesRoot;

        public LandscapesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _imagesRoot = Path.Combine(env.WebRootPath, "images", "landscapes");
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "full"));
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "thumbs"));
        }

        // GET: api/Landscapes
        [HttpGet]
        public async Task<IActionResult> GetAllLandscapes()
        {
            var landscapes = await _context.Landscapes
                .Include(l => l.Category)
                .Include(l => l.Tags)
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    FileName = l.FileName,
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category != null ? l.Category.Name : "Uncategorized",
                    UploadedAt = l.UploadedAt,
                    Full = l.FullPath,
                    Thumbnail = l.ThumbnailPath,
                    Tags = l.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(landscapes);
        }

        // GET: api/Landscapes/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLandscapeById(int id)
        {
            var landscape = await _context.Landscapes
                .Include(l => l.Category)
                .Include(l => l.Tags)
                .Where(l => l.Id == id)
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    FileName = l.FileName,
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category != null ? l.Category.Name : "Uncategorized",
                    UploadedAt = l.UploadedAt,
                    Full = l.FullPath,
                    Thumbnail = l.ThumbnailPath,
                    Tags = l.Tags.Select(t => t.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (landscape == null)
                return NotFound($"No landscape found with ID {id}");

            return Ok(landscape);
        }

        // GET: api/Landscapes/category/{category}
        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("Category name is required.");

            var landscapes = await _context.Landscapes
                .Include(l => l.Category)
                .Include(l => l.Tags)
                .Where(l => l.Category != null &&
                            l.Category.Name.Equals(category, StringComparison.OrdinalIgnoreCase))
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    FileName = l.FileName,
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category.Name,
                    UploadedAt = l.UploadedAt,
                    Full = l.FullPath,
                    Thumbnail = l.ThumbnailPath,
                    Tags = l.Tags.Select(t => t.Name).ToList()
                })
                .ToListAsync();

            return Ok(landscapes);
        }

        // POST: api/Landscapes/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadLandscape(
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

            var landscape = new Landscape
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                Description = string.IsNullOrWhiteSpace(description) ? "No description provided." : description,
                CategoryId = categoryId,
                FileName = file.FileName,
                UploadedAt = DateTime.UtcNow,
                FullPath = $"/images/landscapes/full/{file.FileName}",
                ThumbnailPath = $"/images/landscapes/thumbs/{file.FileName}"
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
                    if (!landscape.Tags.Any(t => t.Name.ToLower() == existingTag.Name.ToLower()))
                        landscape.Tags.Add(existingTag);
                }
            }

            _context.Landscapes.Add(landscape);
            await _context.SaveChangesAsync();

            var dto = new LandscapeResponseDto
            {
                Id = landscape.Id,
                Title = landscape.Title,
                Description = landscape.Description,
                FileName = landscape.FileName,
                CategoryId = landscape.CategoryId,
                CategoryName = landscape.Category?.Name ?? "Uncategorized",
                UploadedAt = landscape.UploadedAt,
                Full = landscape.FullPath,
                Thumbnail = landscape.ThumbnailPath,
                Tags = landscape.Tags.Select(t => t.Name).ToList()
            };

            return CreatedAtAction(nameof(GetLandscapeById), new { id = landscape.Id }, dto);
        }
    }
}