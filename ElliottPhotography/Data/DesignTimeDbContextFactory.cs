using ElliottPhotography.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace ElliottPhotography.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use your PostgreSQL connection string here
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=PhotosDb;Username=postgres;Password=localpassword");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
