using ElliottPhotography.Data;
using ElliottPhotography.Models;
using Microsoft.AspNetCore.Mvc;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult BookAppointment(AppointmentRequest request)
        {
            _context.AppointmentRequests.Add(request);
            _context.SaveChanges();
            return Ok(new { message = "Appointment request received!" });
        }
    }
}
