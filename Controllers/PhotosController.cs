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
    public class PhotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public PhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/photos
        [HttpGet]
        public IActionResult GetPhotos()
        {
            var photos = _context.Photos
                .Select(photo => new PhotoResponseDto
                {
                    Id = photo.Id,
                    Title = photo.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbs/{photo.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{photo.OptimizedFileName ?? photo.FileName}"
                })
                .ToList();

            return Ok(photos);
        }

        // POST: api/photos
        [HttpPost]
        public IActionResult AddPhoto([FromBody] PhotoUploadDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var uploadsFull = Path.Combine(_env.WebRootPath, "images/full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "images/thumbs");

            Directory.CreateDirectory(uploadsFull);
            Directory.CreateDirectory(uploadsThumb);

            var fullPath = Path.Combine(uploadsFull, dto.FileName);
            var thumbPath = Path.Combine(uploadsThumb, dto.FileName);
            var optimizedPath = Path.Combine(uploadsFull, $"optimized-{dto.FileName}");

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "Full image file not found on server." });

            // Create thumbnail
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

            // Create optimized full image
            if (!System.IO.File.Exists(optimizedPath))
            {
                using var fullImage = Image.Load(fullPath);
                fullImage.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(1920, 0)
                }));
                fullImage.Save(optimizedPath, new JpegEncoder { Quality = 85 });
            }

            var photo = new Photo
            {
                Title = dto.Title,
                FileName = dto.FileName,
                OptimizedFileName = $"optimized-{dto.FileName}"
            };

            _context.Photos.Add(photo);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, new PhotoResponseDto
            {
                Id = photo.Id,
                Title = photo.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbs/{photo.FileName}",
                Full = $"{Request.Scheme}://{Request.Host}/images/full/{photo.OptimizedFileName}"
            });
        }
    }
}
