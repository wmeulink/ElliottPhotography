using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _context.Tags
                .Select(t => new { t.Id, t.Name })
                .ToListAsync();
            return Ok(tags);
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Tag name cannot be empty.");

            var existing = await _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (existing != null)
                return Conflict("Tag already exists.");

            var tag = new Tag { Name = name };
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTags), new { id = tag.Id }, tag);
        }
    }
}
