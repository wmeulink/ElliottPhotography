using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.DTOs;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhotosController(AppDbContext context)
        {
            _context = context;
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
                    Thumbnail = $"{Request.Scheme}://{Request.Host}/photos/{photo.Id}/thumb",
                    Full = $"{Request.Scheme}://{Request.Host}/photos/{photo.Id}/full"
                })
                .ToList();

            return Ok(photos);
        }

        // GET: api/photos/{id}/full
        [HttpGet("{id}/full")]
        public IActionResult GetFullImage(int id)
        {
            var photo = _context.Photos.Find(id);
            if (photo == null || photo.FullImage.Length == 0)
                return NotFound();

            return File(photo.FullImage, "image/jpeg");
        }

        // GET: api/photos/{id}/thumb
        [HttpGet("{id}/thumb")]
        public IActionResult GetThumbnail(int id)
        {
            var photo = _context.Photos.Find(id);
            if (photo == null || photo.ThumbnailImage.Length == 0)
                return NotFound();

            return File(photo.ThumbnailImage, "image/jpeg");
        }

        // POST: api/photos/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file, [FromForm] string title)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            byte[] fullBytes;
            byte[] thumbBytes;

            using (var image = Image.Load(file.OpenReadStream()))
            {
                // Full-size image
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

            var photo = new Photo
            {
                Title = string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                OriginalFileName = file.FileName,
                Description = "No description provided.",
                UploadedAt = DateTime.UtcNow,
                FullImage = fullBytes,
                ThumbnailImage = thumbBytes
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, new PhotoResponseDto
            {
                Id = photo.Id,
                Title = photo.Title,
                Thumbnail = $"{Request.Scheme}://{Request.Host}/photos/{photo.Id}/thumb",
                Full = $"{Request.Scheme}://{Request.Host}/photos/{photo.Id}/full"
            });
        }
    }
}
