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

        public DbSet<EmailSettings> EmailSettings { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Portrait> Portraits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Existing Landscape configuration ---
            modelBuilder.Entity<Landscape>(entity =>
            {
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Title)
                    .HasMaxLength(255);
            });

            // --- New Tag relationships ---

            // Portrait ↔ Tag (many-to-many)
            modelBuilder.Entity<Portrait>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Portraits)
                .UsingEntity(j => j.ToTable("PortraitTags"));

            // Landscape ↔ Tag (many-to-many)
            modelBuilder.Entity<Landscape>()
                .HasMany(l => l.Tags)
                .WithMany(t => t.Landscapes)
                .UsingEntity(j => j.ToTable("LandscapeTags"));
        }

    }
}

