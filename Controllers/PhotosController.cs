using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using ElliottPhotography.DTOs;

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

        [HttpGet]
        public IActionResult GetPhotos()
        {
            var photos = _context.Photos
                .Select(photo => new PhotoResponseDto
                {
                    Id = photo.Id,
                    Title = photo.Title,
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbs/{photo.FileName}",
                    Full = $"{Request.Scheme}://{Request.Host}/images/full/{photo.FileName}"
                })
                .ToList();

            return Ok(photos);
        }

        [HttpPost]
        public IActionResult AddPhoto([FromBody] PhotoUploadDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.FileName) || string.IsNullOrEmpty(dto.Title))
                return BadRequest(new { error = "FileName and Title are required." });

            var uploadsFull = Path.Combine(_env.WebRootPath, "images/full");
            var uploadsThumb = Path.Combine(_env.WebRootPath, "images/thumbs");

            if (!Directory.Exists(uploadsFull)) Directory.CreateDirectory(uploadsFull);
            if (!Directory.Exists(uploadsThumb)) Directory.CreateDirectory(uploadsThumb);

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

            var photo = new Photo
            {
                Title = dto.Title,
                FileName = dto.FileName
            };

            _context.Photos.Add(photo);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, new PhotoResponseDto
            {
                Id = photo.Id,
                Title = photo.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/images/thumbs/{photo.FileName}",
                Full = $"{Request.Scheme}://{Request.Host}/images/full/{photo.FileName}"
            });
        }
    }
}