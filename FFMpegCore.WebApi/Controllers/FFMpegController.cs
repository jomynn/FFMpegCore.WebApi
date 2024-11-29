using Microsoft.AspNetCore.Mvc;
using FFMpegCore;
using FFMpegCore.Enums;
using System.Drawing;
using SkiaSharp;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FFMpegController : ControllerBase
{
    private readonly ILogger<FFMpegController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string UploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    private readonly string OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");

    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {
        return Ok("This is a protected endpoint!");
    }

    public FFMpegController(ILogger<FFMpegController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        Directory.CreateDirectory(UploadsPath);
        Directory.CreateDirectory(OutputPath);
        _environment = environment;
    }

    [HttpGet("test")]
    public IActionResult Test() => Ok("API is working");

    [HttpPost("analyse")]
    public IActionResult AnalyseVideo([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);

        if (!System.IO.File.Exists(inputPath))
        {
            _logger.LogWarning($"Input file not found: {inputPath}");
            return BadRequest("Input file not found.");
        }

        try
        {
            var mediaInfo = FFProbe.Analyse(inputPath);
            _logger.LogInformation($"Successfully analysed video: {inputPath}");

            return Ok(new
            {
                Duration = mediaInfo.Duration,
                VideoStreams = mediaInfo.VideoStreams,
                AudioStreams = mediaInfo.AudioStreams
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing video: {inputPath}");
            return StatusCode(500, "An error occurred while analyzing the video.");
        }
    }

    [HttpPost("convert")]
    public IActionResult ConvertVideo([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, request.OutputFileName ?? "converted.mp4");

        if (!System.IO.File.Exists(inputPath))
        {
            _logger.LogWarning($"Input file not found: {inputPath}");
            return BadRequest("Input file not found.");
        }

        try
        {
            FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(outputFilePath, overwrite: true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithConstantRateFactor(21)
                    .WithFastStart())
                .ProcessSynchronously();

            _logger.LogInformation($"Video converted successfully: {inputPath} -> {outputFilePath}");
            return Ok(new { Message = "Video converted successfully.", OutputPath = outputFilePath });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error converting video: {inputPath}");
            return StatusCode(500, "An error occurred while converting the video.");
        }
    }

    [HttpPost("snapshot")]
    public IActionResult CreateSnapshot([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, request.OutputFileName ?? "snapshot.jpg");

        if (!System.IO.File.Exists(inputPath))
        {
            return BadRequest("Input file not found.");
        }

        // Use SkiaSharp to save the snapshot
        var bitmap = FFMpegCore.Extensions.SkiaSharp.FFMpegImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromSeconds(10));

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100); // Save as JPEG
        using var stream = System.IO.File.OpenWrite(outputFilePath);
        data.SaveTo(stream);

        return Ok(new { Message = "Snapshot created successfully.", OutputPath = outputFilePath });
    }

    [HttpPost("extract-audio")]
    public IActionResult ExtractAudio([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, request.OutputFileName ?? "audio.mp3");

        if (!System.IO.File.Exists(inputPath))
        {
            return BadRequest("Input file not found.");
        }

        FFMpeg.ExtractAudio(inputPath, outputFilePath);

        return Ok(new { Message = "Audio extracted successfully.", OutputPath = outputFilePath });
    }

    [HttpPost("join-videos")]
    public IActionResult JoinVideos([FromBody] JoinVideosRequest request)
    {
        var outputFilePath = Path.Combine(OutputPath, request.OutputFileName ?? "joined.mp4");

        foreach (var file in request.InputFileNames)
        {
            if (!System.IO.File.Exists(Path.Combine(UploadsPath, file)))
            {
                return BadRequest($"File not found: {file}");
            }
        }

        var inputFiles = request.InputFileNames.Select(file => Path.Combine(UploadsPath, file)).ToArray();
        FFMpeg.Join(outputFilePath, inputFiles);

        return Ok(new { Message = "Videos joined successfully.", OutputPath = outputFilePath });
    }

    [HttpGet("process-with-progress")]
    public async Task ProcessWithProgress(CancellationToken cancellationToken)
    {
        // Set the HTTP headers for Server-Sent Events
        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.Append("Cache-Control", "no-cache");
        HttpContext.Response.Headers.Append("Connection", "keep-alive");

        // Initialize progress tracking
        var progress = new Progress<int>(async percent =>
        {
            await HttpContext.Response.WriteAsync($"data: {{\"progress\": {percent}}}\n\n");
            await HttpContext.Response.Body.FlushAsync();
        });

        try
        {
            var inputPath = Path.Combine(UploadsPath, "input.mp4");
            var outputPath = Path.Combine(OutputPath, "output.mp4");

            if (!System.IO.File.Exists(inputPath))
            {
                await HttpContext.Response.WriteAsync("data: {\"error\": \"Input file not found.\"}\n\n");
                return;
            }

            // Process video with progress
            await ProcessVideoWithProgress(inputPath, outputPath, progress, cancellationToken);

            await HttpContext.Response.WriteAsync("data: {\"status\": \"Completed\"}\n\n");
        }
        catch (Exception ex)
        {
            await HttpContext.Response.WriteAsync($"data: {{\"error\": \"{ex.Message}\"}}\n\n");
        }
    }

    private static async Task ProcessVideoWithProgress(string inputPath, string outputPath, IProgress<int> progress, CancellationToken cancellationToken)
    {
        var totalFrames = 100; // Example: Total frames (or percentage steps)
        for (var i = 0; i <= totalFrames; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException("Processing was canceled.");
            }

            // Simulate processing work
            await Task.Delay(50, cancellationToken); // Simulate frame processing

            // Report progress
            progress.Report(i);
        }

        // After processing is complete, perform file operations
        await FFMpegArguments
            .FromFileInput(inputPath)
            .OutputToFile(outputPath, true, options => options
                .WithVideoCodec("libx264")
                .WithFastStart())
            .ProcessAsynchronously();
    }

    [HttpPost("merge-videos")]
    public IActionResult MergeVideos([FromBody] MergeVideosRequest request)
    {
        if (request.VideoPaths == null || request.VideoPaths.Count < 2)
        {
            return BadRequest("At least two video paths are required.");
        }

        try
        {
            var outputPath = Path.Combine("output", request.OutputFileName ?? "merged_video.mp4");
            FFMpeg.Join(outputPath, request.VideoPaths.ToArray());

            return Ok(new
            {
                Success = true,
                OutputPath = outputPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpPost("merge-audio-video")]
    public IActionResult MergeAudioWithVideo([FromBody] MergeAudioVideoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VideoFileBase64) ||
            string.IsNullOrWhiteSpace(request.AudioFileBase64) ||
            string.IsNullOrWhiteSpace(request.OutputFileName))
        {
            return BadRequest("All fields (videoFileBase64, audioFileBase64, outputFileName) are required.");
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");
        var outputPath = Path.Combine(_environment.WebRootPath, "output");

        // Ensure directories exist
        Directory.CreateDirectory(uploadsPath);
        Directory.CreateDirectory(outputPath);

        var videoFilePath = Path.Combine(uploadsPath, "uploaded_video.mp4");
        var audioFilePath = Path.Combine(uploadsPath, "uploaded_audio.mp3");
        var outputFilePath = Path.Combine(outputPath, request.OutputFileName);

        try
        {
            // Decode and save the video file
            var videoBytes = Convert.FromBase64String(request.VideoFileBase64);
            System.IO.File.WriteAllBytes(videoFilePath, videoBytes);

            // Decode and save the audio file
            var audioBytes = Convert.FromBase64String(request.AudioFileBase64);
            System.IO.File.WriteAllBytes(audioFilePath, audioBytes);

            // Merge the audio and video
            FFMpeg.ReplaceAudio(videoFilePath, audioFilePath, outputFilePath);

            // Return the output file path
            var resultUrl = Url.Content($"~/output/{request.OutputFileName}");
            return Ok(new { Message = "Merge successful", OutputUrl = resultUrl });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
        finally
        {
            // Clean up temporary files
            if (System.IO.File.Exists(videoFilePath))
            {
                System.IO.File.Delete(videoFilePath);
            }

            if (System.IO.File.Exists(audioFilePath))
            {
                System.IO.File.Delete(audioFilePath);
            }
        }
    }

    //[HttpPost("merge-audio-video")]
    //public IActionResult MergeAudioWithVideo(
    //   [FromForm] IFormFile audioFile,
    //   [FromForm] IFormFile videoFile,
    //   [FromForm] string outputFileName)
    //{
    //    if (audioFile == null || videoFile == null)
    //    {
    //        return BadRequest("Both audio and video files are required.");
    //    }

    //    var audioPath = Path.Combine("uploads", audioFile.FileName);
    //    var videoPath = Path.Combine("uploads", videoFile.FileName);
    //    var outputPath = Path.Combine("output", outputFileName);

    //    try
    //    {
    //        using (var audioStream = new FileStream(audioPath, FileMode.Create))
    //        {
    //            audioFile.CopyTo(audioStream);
    //        }

    //        using (var videoStream = new FileStream(videoPath, FileMode.Create))
    //        {
    //            videoFile.CopyTo(videoStream);
    //        }

    //        // Example usage of FFMpeg library (implementation not included)
    //        FFMpeg.ReplaceAudio(videoPath, audioPath, outputPath);

    //        return Ok(new { OutputPath = outputPath });
    //    }
    //    finally
    //    {
    //        // Clean up temporary files
    //        if (System.IO.File.Exists(audioPath))
    //        {
    //            System.IO.File.Delete(audioPath);
    //        }

    //        if (System.IO.File.Exists(videoPath))
    //        {
    //            System.IO.File.Delete(videoPath);
    //        }
    //    }
    //}

    [HttpPost("add-audio")]
    public IActionResult AddAudio([FromBody] AddAudioRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoPath) || string.IsNullOrEmpty(request.AudioPath))
        {
            return BadRequest("Both video and audio paths are required.");
        }

        try
        {
            var outputPath = Path.Combine("output", request.OutputFileName ?? "video_with_audio.mp4");
            FFMpeg.ReplaceAudio(request.VideoPath, request.AudioPath, outputPath);

            return Ok(new
            {
                Success = true,
                OutputPath = outputPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
    [HttpPost("add-subtitles")]
    public IActionResult AddSubtitles([FromBody] AddSubtitleRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoPath) || string.IsNullOrEmpty(request.SubtitlePath))
        {
            return BadRequest("Both video and subtitle paths are required.");
        }

        try
        {
            var outputPath = Path.Combine("output", request.OutputFileName ?? "video_with_subtitle.mp4");

            // Use FFMpegArguments to add subtitles
            FFMpegArguments
                .FromFileInput(request.VideoPath)
                .OutputToFile(outputPath, true, options => options
                    .WithCustomArgument($"-vf subtitles=\"{request.SubtitlePath}\"") // Add subtitles using custom argument
                    .WithVideoCodec("libx264") // Ensure proper video codec
                    .WithConstantRateFactor(23)
                    .WithAudioCodec("aac"))
                .ProcessSynchronously();

            return Ok(new
            {
                Success = true,
                OutputPath = outputPath
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }
}
