using ElliottPhotography.Data;
using ElliottPhotography.Models;
using ElliottPhotography.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElliottPhotography.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public ContactController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessage message)
        {
            if (message == null ||
                string.IsNullOrWhiteSpace(message.Name) ||
                string.IsNullOrWhiteSpace(message.Email) ||
                string.IsNullOrWhiteSpace(message.Message))
            {
                return BadRequest(new { error = "Name, Email, and Message are required." });
            }

            message.SentAt = DateTime.UtcNow;
            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            // Send email notification
            var subject = $"📸 New Message from {message.Name}";
            var body = $"Name: {message.Name}\nEmail: {message.Email}\nPhone: {message.Phone}\n\nMessage:\n{message.Message}";

            await _emailService.SendEmailAsync(message.Email, subject, body);

            return Ok(new { message = "Message received successfully and email sent!" });
        }

        [HttpGet]
        public IActionResult GetMessages()
        {
            var messages = _context.ContactMessages
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return Ok(messages);
        }
    }
}