using ElliottPhotography.Data;
using ElliottPhotography.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using DbSeeder = ElliottPhotography.Data.DbSeeder;

var builder = WebApplication.CreateBuilder(args);

// ------------------ SERVICES ------------------

// Add controllers
builder.Services.AddControllers();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

// Swagger / API explorer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------ MIDDLEWARE ------------------

// Swagger
bool enableSwaggerInProd = app.Environment.IsProduction() &&
                           builder.Configuration.GetValue<bool>("EnableSwaggerInProduction");

if (app.Environment.IsDevelopment() || enableSwaggerInProd)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Static files
app.UseStaticFiles(); // wwwroot

// Thumbnails
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "thumbs")),
    RequestPath = "/images/thumbnails"
});

// Full images
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "full")),
    RequestPath = "/images/full"
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ------------------ DATABASE MIGRATION & SEEDING ------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        Console.WriteLine("Applying pending migrations...");
        db.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration failed: {ex}");
        throw;
    }

    // Seed images and categories
    try
    {
        Console.WriteLine("Seeding database...");
        DbSeeder.SeedImages(db, app.Environment.ContentRootPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding failed: {ex}");
        throw;
    }
}

// ------------------ RUN ------------------
app.Run();