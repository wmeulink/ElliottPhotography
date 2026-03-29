using ElliottPhotography.Data;
using ElliottPhotography.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

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

// Email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

// Swagger
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
// Serve static files from wwwroot/images
var imagesRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "images");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesRoot),
    RequestPath = "/images"
});

// Serve static files from wwwroot/portraits
var portraitsRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "portraits");
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(portraitsRoot),
    RequestPath = "/portraits"
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();