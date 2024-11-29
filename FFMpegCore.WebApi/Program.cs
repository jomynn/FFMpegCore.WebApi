using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    logger.Debug("Starting application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure NLog as the logging provider
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer(); // Enables discovery of endpoints
    builder.Services.AddSwaggerGen();          // Adds Swagger services

    var app = builder.Build();

    // Configure Swagger middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();                      // Generates the Swagger JSON
        app.UseSwaggerUI();                    // Serves the Swagger UI
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped because of an exception");
    throw;
}
finally
{
    // Ensure proper shutdown of NLog
    LogManager.Shutdown();
}
