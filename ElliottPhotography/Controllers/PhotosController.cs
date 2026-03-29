using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.IO;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _imagesRoot;

        public PhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _imagesRoot = Path.Combine(env.WebRootPath, "images", "photos");
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "full"));
            Directory.CreateDirectory(Path.Combine(_imagesRoot, "thumbs"));
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
                    Description = photo.Description ?? "No description provided.",
                    OriginalFileName = photo.OriginalFileName,
                    UploadedAt = photo.UploadedAt,
                    Full = photo.FullPath,
                    Thumbnail = photo.ThumbnailPath
                })
                .ToList();

            return Ok(photos);
        }

        // GET: api/photos/{id}/full
        [HttpGet("{id}/full")]
        public IActionResult GetFullImage(int id)
        {
            var photo = _context.Photos.Find(id);
            if (photo == null || string.IsNullOrWhiteSpace(photo.FullPath))
                return NotFound();

            var fullFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.FullPath.TrimStart('/'));
            if (!System.IO.File.Exists(fullFile))
                return NotFound();

            return PhysicalFile(fullFile, "image/jpeg");
        }

        // GET: api/photos/{id}/thumb
        [HttpGet("{id}/thumb")]
        public IActionResult GetThumbnail(int id)
        {
            var photo = _context.Photos.Find(id);
            if (photo == null || string.IsNullOrWhiteSpace(photo.ThumbnailPath))
                return NotFound();

            var thumbFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.ThumbnailPath.TrimStart('/'));
            if (!System.IO.File.Exists(thumbFile))
                return NotFound();

            return PhysicalFile(thumbFile, "image/jpeg");
        }

        // POST: api/photos/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, [FromForm] string? title)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var fullFileName = Path.Combine(_imagesRoot, "full", file.FileName);
            var thumbFileName = Path.Combine(_imagesRoot, "thumbs", file.FileName);

            // Save full-size image
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

            var photo = new Photo
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                OriginalFileName = file.FileName,
                Description = "No description provided.",
                UploadedAt = DateTime.UtcNow,
                FullPath = $"/images/photos/full/{file.FileName}",
                ThumbnailPath = $"/images/photos/thumbs/{file.FileName}"
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, new PhotoResponseDto
            {
                Id = photo.Id,
                Title = photo.Title,
                Description = photo.Description,
                OriginalFileName = photo.OriginalFileName,
                UploadedAt = photo.UploadedAt,
                Full = photo.FullPath,
                Thumbnail = photo.ThumbnailPath
            });
        }
    }
}