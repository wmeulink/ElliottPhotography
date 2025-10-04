using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetLandscapes()
        {
            var landscapes = _context.Landscapes
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{l.FileName}", // <-- FIXED
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{l.FileName}",
                    FileName = l.FileName
                })
                .ToList();

            return Ok(landscapes);
        }

        // GET: api/Landscapes/category/{category}
        [HttpGet("category/{category}")]
        public IActionResult GetLandscapesByCategory(string category)
        {
            var landscapes = _context.Landscapes
                .Where(l => l.Category != null && l.Category.ToLower() == category.ToLower())
                .Select(l => new LandscapeResponseDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{l.FileName}", // <-- FIXED
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{l.FileName}",
                    FileName = l.FileName
                })
                .ToList();

            return Ok(landscapes);
        }

        // POST: api/Landscapes
        [HttpPost]
        public IActionResult AddLandscape([FromBody] LandscapeUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var uploadsFull = Path.Combine(_env.WebRootPath, "images/full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "images/thumbnails"); // <-- MATCHED

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
                Description = "",
                Category = ""
            };

            _context.Landscapes.Add(landscape);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetLandscapes), new { id = landscape.Id }, new LandscapeResponseDto
            {
                Id = landscape.Id,
                Title = landscape.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbnails/{landscape.FileName}", // <-- FIXED
                Full = $"{Request.Scheme}://{Request.Host}/images/full/{landscape.FileName}",
                FileName = landscape.FileName
            });
        }
    }
}
