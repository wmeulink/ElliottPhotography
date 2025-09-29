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
                    FileName = $"/images/{System.IO.Path.GetFileName(photo.FileName)}"
                })
                .ToList();

            return Ok(photos);
        }

        [HttpPost]
        public ActionResult<Photo> AddPhoto(Photo photo)
        {
            _context.Photos.Add(photo);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetPhotos), new { id = photo.Id }, photo);
        }
    }
}
