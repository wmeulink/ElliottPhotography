using Microsoft.EntityFrameworkCore;
using ElliottPhotography.Models;
using System.Collections.Generic;


namespace ElliottPhotography.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Photo> Photos { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
    }
}

