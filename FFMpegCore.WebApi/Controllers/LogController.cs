using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api")]
public class LogController : ControllerBase
{
    [HttpGet("joblogs")]
    public IActionResult GetJobLogs()
    {
        var jobLogs = new[]
        {
            new { Id = 1, JobName = "AnalyseVideo", Status = "Success", Details = "Analysis complete", CreatedAt = "2024-11-29" },
            new { Id = 2, JobName = "ConvertVideo", Status = "Error", Details = "Unsupported format", CreatedAt = "2024-11-28" }
        };
        return Ok(jobLogs);
    }

    [HttpGet("renderlogs")]
    public IActionResult GetRenderLogs()
    {
        var renderLogs = new[]
        {
            new { VideoId = Guid.NewGuid(), Status = "Success", Resolution = "1080p", DurationSeconds = 300, RenderingTime = 2000, CreateDate = "2024-11-29" },
            new { VideoId = Guid.NewGuid(), Status = "Error", Resolution = "720p", DurationSeconds = 150, RenderingTime = 1000, CreateDate = "2024-11-28" }
        };
        return Ok(renderLogs);
    }
}
