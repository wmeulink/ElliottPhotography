using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Linq;

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

        // POST: api/Landscapes
        [HttpPost]
        public IActionResult AddLandscape([FromBody] LandscapeUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var uploadsFull = Path.Combine(_env.WebRootPath, "images/full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "images/thumbnails");

            Directory.CreateDirectory(uploadsFull);
            Directory.CreateDirectory(uploadsThumb);

            var fullPath = Path.Combine(uploadsFull, dto.FileName);
            var thumbPath = Path.Combine(uploadsThumb, dto.FileName);

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "Full image file not found on server." });

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

            var landscape = new Landscape
            {
                Title = dto.Title,
                FileName = dto.FileName,
                UploadedAt = DateTime.UtcNow,
                Description = dto.Description ?? "No description provided.",
                CategoryId = dto.CategoryId
            };

            _context.Landscapes.Add(landscape);
            _context.SaveChanges();

            var categoryName = _context.Categories.FirstOrDefault(c => c.Id == landscape.CategoryId)?.Name;

            return CreatedAtAction(nameof(GetAllLandscapes), new { id = landscape.Id }, new LandscapeResponseDto
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
