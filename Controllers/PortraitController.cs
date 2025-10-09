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

        // ✅ POST: api/Portraits
        [HttpPost]
        public IActionResult AddPortrait([FromBody] PortraitUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 👉 Correct folder structure (NOT under /images)
            var uploadsFull = Path.Combine(_env.WebRootPath, "portraits", "full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "portraits", "thumbs");

            Directory.CreateDirectory(uploadsFull);
            Directory.CreateDirectory(uploadsThumb);

            var fullPath = Path.Combine(uploadsFull, dto.FileName);
            var thumbPath = Path.Combine(uploadsThumb, dto.FileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "Full image file not found on server." });

            // Create thumbnail if missing
            if (!System.IO.File.Exists(thumbPath))
            {
                using var image = Image.Load(fullPath);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(400, 0)
                }));
                image.Save(thumbPath, new JpegEncoder { Quality = 70 });
            }

            var portrait = new Portrait
            {
                Title = dto.Title,
                FileName = dto.FileName,
                UploadedAt = DateTime.UtcNow,
                Description = dto.Description ?? "No description provided.",
                CategoryId = dto.CategoryId
            };

            _context.Portraits.Add(portrait);
            _context.SaveChanges();

            var categoryName = _context.Categories
                .FirstOrDefault(c => c.Id == portrait.CategoryId)?.Name ?? "Uncategorized";

            return CreatedAtAction(nameof(AddPortrait), new PortraitResponseDto
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
