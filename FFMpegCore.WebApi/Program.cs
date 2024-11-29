using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        throw new InvalidOperationException("JWT configuration is missing or invalid in appsettings.json.");
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

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddSingleton(jwtSettings);

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
