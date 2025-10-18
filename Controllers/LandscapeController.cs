using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandscapesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LandscapesController(AppDbContext context)
        {
            _context = context;
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
                    CategoryId = l.CategoryId,
                    CategoryName = l.Category.Name,
                    Description = l.Description,
                    FileName = l.FileName,
                    UploadedAt = l.UploadedAt,
                    Tags = l.Tags.Select(t => t.Name).ToList(),
                    Thumbnail = l.ThumbnailData,
                    Full = l.ImageData
                })
                .ToListAsync();

            return Ok(landscapes);
        }

        // GET: api/Landscapes/{id}/full
        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetFullImage(int id)
        {
            var landscape = await _context.Landscapes.FindAsync(id);
            if (landscape == null || landscape.ImageData == null)
                return NotFound();

            return File(landscape.ImageData, "image/jpeg");
        }

        // GET: api/Landscapes/{id}/thumb
        [HttpGet("{id}/thumb")]
        public async Task<IActionResult> GetThumbnail(int id)
        {
            var landscape = await _context.Landscapes.FindAsync(id);
            if (landscape == null || landscape.ThumbnailData == null)
                return NotFound();

            return File(landscape.ThumbnailData, "image/jpeg");
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

            byte[] fullBytes;
            byte[] thumbBytes;

            using (var image = Image.Load(file.OpenReadStream()))
            {
                using var msFull = new MemoryStream();
                image.Save(msFull, new JpegEncoder { Quality = 90 });
                fullBytes = msFull.ToArray();

                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 0)
                }));

                using var msThumb = new MemoryStream();
                image.Save(msThumb, new JpegEncoder { Quality = 70 });
                thumbBytes = msThumb.ToArray();
            }

            var landscape = new Landscape
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
                    }
                    landscape.Tags.Add(existingTag);
                }
            }

            _context.Landscapes.Add(landscape);
            await _context.SaveChangesAsync();

            var dto = new LandscapeResponseDto
            {
                Id = landscape.Id,
                Title = landscape.Title,
                CategoryId = landscape.CategoryId,
                Description = landscape.Description,
                FileName = landscape.FileName,
                UploadedAt = landscape.UploadedAt,
                Tags = landscape.Tags.Select(t => t.Name).ToList(),
                Thumbnail = landscape.ThumbnailData,
                Full = landscape.ImageData
            };

            return CreatedAtAction(nameof(GetAllLandscapes), new { id = landscape.Id }, dto);
        }
    }
}
