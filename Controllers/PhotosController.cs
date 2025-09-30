using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public ActionResult<IEnumerable<object>> GetPhotos()
        {
            var photos = _context.Photos
                .Select(photo => new
                {
                    photo.Id,
                    photo.Title,
                    FileName = $"/images/{photo.FileName}"
                })
                .ToList();

            return Ok(photos);
        }

        [HttpGet("images/{fileName}")]
        public IActionResult GetImage(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
            if (!System.IO.File.Exists(path)) return NotFound();

            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "image/jpeg");
        }

        [HttpPost]
        public ActionResult<Photo> AddPhoto(Photo photo)
        {
            _context.Photos.Add(photo);
            _context.SaveChanges();

            // Build the absolute URL for the response
            var result = new
            {
                photo.Id,
                photo.Title,
                FileName = $"{Request.Scheme}://{Request.Host}/images/{Path.GetFileName(photo.FileName)}"
            };

            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, result);
        }

    }
}
