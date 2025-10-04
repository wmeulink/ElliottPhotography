using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElliottPhotography.Data;
using ElliottPhotography.Models;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LandscapesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LandscapesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Landscapes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Landscape>>> GetLandscapes()
        {
            return await _context.Landscapes.ToListAsync();
        }

        // GET: api/Landscapes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Landscape>> GetLandscape(int id)
        {
            var landscape = await _context.Landscapes.FindAsync(id);

            if (landscape == null)
            {
                return NotFound();
            }

            return landscape;
        }

        // ✅ GET: api/Landscapes/category/Mountains
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Landscape>>> GetLandscapesByCategory(string category)
        {
            var landscapes = await _context.Landscapes
                .Where(l => l.Category.ToLower() == category.ToLower())
                .ToListAsync();

            if (landscapes.Count == 0)
            {
                return NotFound($"No landscapes found in category '{category}'.");
            }

            return landscapes;
        }

        // POST: api/Landscapes
        [HttpPost]
        public async Task<ActionResult<Landscape>> PostLandscape(Landscape landscape)
        {
            _context.Landscapes.Add(landscape);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLandscape", new { id = landscape.Id }, landscape);
        }

        // DELETE: api/Landscapes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLandscape(int id)
        {
            var landscape = await _context.Landscapes.FindAsync(id);
            if (landscape == null)
            {
                return NotFound();
            }

            _context.Landscapes.Remove(landscape);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
