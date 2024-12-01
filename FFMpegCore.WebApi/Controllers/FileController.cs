using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FileController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("list-files")]
    public IActionResult ListFiles()
    {
        try
        {
            var folderPath = Path.Combine(_env.WebRootPath, "sample/input");
            if (!Directory.Exists(folderPath))
            {
                return NotFound("Input folder does not exist.");
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}/sample/input/";
            var files = Directory.GetFiles(folderPath)
                                 .Select(file => new
                                 {
                                     FileName = Path.GetFileName(file),
                                     FullPath = baseUrl + Path.GetFileName(file)
                                 })
                                 .ToList();

            return Ok(files);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while retrieving files.", Error = ex.Message });
        }
    }
}
