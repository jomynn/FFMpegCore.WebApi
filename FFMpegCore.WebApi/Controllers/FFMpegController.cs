using Microsoft.AspNetCore.Mvc;
using FFMpegCore;
using FFMpegCore.Enums;
using System.Drawing;
using SkiaSharp;

[ApiController]
[Route("api/[controller]")]
public class FFMpegController : ControllerBase
{
    private readonly ILogger<FFMpegController> _logger;
    private readonly string UploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    private readonly string OutputPath = Path.Combine(Directory.GetCurrentDirectory(), "output");

    public FFMpegController(ILogger<FFMpegController> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(UploadsPath);
        Directory.CreateDirectory(OutputPath);
    }

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
}
