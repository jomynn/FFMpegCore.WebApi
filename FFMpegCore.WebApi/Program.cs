using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;
using System.Text;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog as the logging provider
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Configure JWT
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
    if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Key))
    {
        //throw new InvalidOperationException(
        //    "JWT configuration is missing or invalid in appsettings.json. Please ensure the 'Jwt' section includes 'Key', 'Issuer', and 'Audience'.");
        jwtSettings = new JwtSettings
        {
            Key = "fE84he8$45@9plKzM0Nq!xRvtD6Jw7YB",
            Issuer = "YourApp",
            Audience = "YourAppUsers",
            TokenLifetimeMinutes = 60
        };
        Console.WriteLine("Warning: Using fallback JWT key.");
    }

    Console.WriteLine($"JWT Settings Loaded: Issuer={jwtSettings.Issuer}, Audience={jwtSettings.Audience}");

    if (jwtSettings.Key.Length < 32)
    {
        throw new InvalidOperationException("JWT signing key must be at least 256 bits (32 characters) long.");
    }

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

    // Log JWT configuration (optional)
 
    logger.Info("JWT authentication configured with Issuer: {Issuer} and Audience: {Audience}",
        jwtSettings.Issuer, jwtSettings.Audience);

    // Add services to the container
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite("Data Source=JobLogs.db"));

    // Add services to the container
    builder.Services.AddControllersWithViews();
    builder.Services.AddSingleton(jwtSettings);

    // Add Swagger for API documentation
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "FFMpegCore API",
            Version = "v1",
            Description = "API for video and audio processing using FFMpegCore."
        });

        // Add support for file uploads
        c.OperationFilter<SwaggerRequestBodyFilter>();
    });

    var app = builder.Build();

    // Log the dynamically assigned URL
    var serverAddresses = app.Urls;
    Console.WriteLine("Application is running on the following URLs:");
    foreach (var address in serverAddresses)
    {
        Console.WriteLine(address);
    }

    // Migrate database on startup
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
           
            logger.Error(ex, "An error occurred while migrating the database.");
            throw; // Re-throw to catch the root cause
        }
    }

    //app.UseHttpsRedirection();

    //app.UseAuthentication();
    //app.UseAuthorization();

    //// Serve static files for your custom dashboard
    //app.UseDefaultFiles(); // Enables default index.html if no specific file is requested
    //app.UseStaticFiles();  // Enables serving static files from wwwroot
    
    

    // Enable Swagger
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FFMpegCore API v1");
            c.RoutePrefix = "swagger"; // Swagger available at /swagger
        });
    }

    // Enable static files (to serve HTML, CSS, JS, etc.)
    app.UseDefaultFiles(); // This ensures index.html or dashboard.html is served by default
    app.UseStaticFiles();  // Serves static files from wwwroot folder
    // Configure the routing to default to dashboard.html
    app.UseRouting();

    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/dashboard.html", permanent: false);
            return;
        }

        await next();
    });

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();

}
catch (Exception ex)
{
    // Log fatal errors during startup
    logger.Error(ex, "Application stopped due to an exception");
    throw;
}
finally
{
    // Ensure proper shutdown of NLog
    NLog.LogManager.Shutdown();
}
