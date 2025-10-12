using System.Net;
using System.Net.Mail;
using ElliottPhotography.Models;
using Microsoft.Extensions.Options;

namespace ElliottPhotography.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string from, string subject, string body)
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.UserName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            // Send to your own inbox
            mailMessage.To.Add(_settings.UserName);

            // Optionally, CC or BCC the sender:
            // mailMessage.ReplyToList.Add(from);

            await client.SendMailAsync(mailMessage);
        }
    }
}
