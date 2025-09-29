namespace ElliottPhotography.Models
{
    public class AppointmentRequest
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public DateTime?  PreferredDate { get; set; }
        public string? Notes { get; set; }
    }
}
