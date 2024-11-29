var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Serve static files for your custom dashboard
app.UseDefaultFiles(); // Enables default index.html if no specific file is requested
app.UseStaticFiles();  // Enables serving static files from wwwroot

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FFMpegCore API v1");
    c.RoutePrefix = "swagger"; // Swagger will now be available at /swagger
});

// Map controllers
app.MapControllers();

app.Run();
