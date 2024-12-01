using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class FileUploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FileUploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost("upload-files")]
    public IActionResult UploadFiles([FromForm] IEnumerable<IFormFile> files)
    {
        if (files == null || !files.Any())
        {
            return BadRequest("No files were uploaded.");
        }

        try
        {
            var uploadPath = Path.Combine(_env.WebRootPath, "sample/input");

            // Ensure the directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            foreach (var file in files)
            {
                var filePath = Path.Combine(uploadPath, file.FileName);

                // Save the file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }

            return Ok(new { Message = "Files uploaded successfully.", FileCount = files.Count() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while uploading files.", Error = ex.Message });
        }
    }
}
