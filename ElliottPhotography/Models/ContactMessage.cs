namespace ElliottPhotography.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public ContactMessage(string name, string email, string phone, string message)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Message = message;
        }
    }
}
