using Microsoft.EntityFrameworkCore;
using ElliottPhotography.Models;
using System.Collections.Generic;

    namespace ElliottPhotography.Data
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {
            }

            // Existing tables
            public DbSet<Photo> Photos { get; set; }

            // New Landscapes table
            public DbSet<Landscape> Landscapes { get; set; }

        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<AppointmentRequest> AppointmentRequests { get; set; }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Portrait> Portraits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Example: configure Landscapes if you want
                modelBuilder.Entity<Landscape>(entity =>
                {
                    entity.Property(e => e.FileName)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.Property(e => e.Title)
                        .HasMaxLength(255);
                });
            }
    }
}

