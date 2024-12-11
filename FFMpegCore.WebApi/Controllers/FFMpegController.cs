using Microsoft.AspNetCore.Mvc;
using FFMpegCore;
using FFMpegCore.Enums;
using System.Drawing;
using SkiaSharp;
using Microsoft.AspNetCore.Authorization;
using FFPmpegCore.Global;
using FFMpegCore.WebApi.Utils;
using FFMpegCore.Arguments;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FFMpegController : ControllerBase
{
    private readonly ILogger<FFMpegController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string UploadsPath = Path.Combine(Directory.GetCurrentDirectory(), UPLOADS);
    private readonly string OutputPath = Path.Combine(Directory.GetCurrentDirectory(), OUTPUT);
    private const string MP4 = GlobalConstants.FilePaths.MP4;
    private const string MP3 = GlobalConstants.FilePaths.MP3;
    private const string JPG = GlobalConstants.FilePaths.JPG;
    public const string SRT = GlobalConstants.FilePaths.SRT;
    private const string OUTPUT = GlobalConstants.FilePaths.OUTPUT;
    private const string UPLOADS = GlobalConstants.FilePaths.UPLOADS;

    public FFMpegController(ILogger<FFMpegController> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        Directory.CreateDirectory(UploadsPath);
        Directory.CreateDirectory(OutputPath);
        _environment = environment;
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {
        return Ok(GlobalConstants.Messages.EndpointProtect);
    }

    [HttpGet("test")]
    public IActionResult Test() => Ok(GlobalConstants.Messages.ApiWorking);

    [Authorize]
    [HttpPost("analyse")]
    public IActionResult AnalyseVideo([FromBody] string request)
    {
        if (string.IsNullOrEmpty(request))
        {
            return BadRequest("The request body must not be empty.");
        }

        try
        {

            if (string.IsNullOrWhiteSpace(request))
            {
                return BadRequest("The request must contain at least one video file URL and one caption.");
            }

            if (!request.EndsWith(MP3, StringComparison.OrdinalIgnoreCase) & 
                !request.EndsWith(MP4, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"No {MP3} or  {MP4} media file found in the request.");
            }

            // Prepare temp folder for downloaded files
            var tempPath = Help.PrepareTempFolder();

            // Download video file to temp folder
            var tempVideoFile = Help.DownloadFile(request, tempPath);

            // Analyze the video file
            var mediaInfo = FFProbe.Analyse(tempVideoFile);

            // Log and return analysis results
            _logger.LogInformation($"{GlobalConstants.Messages.SuccessAnalysedVideo}: {tempVideoFile}");

            return Ok(new
            {
                Duration = mediaInfo.Duration,
                VideoStreams = mediaInfo.VideoStreams,
                AudioStreams = mediaInfo.AudioStreams
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing video: {request}");
            return StatusCode(500, new
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpPost("convert")]
    public IActionResult ConvertVideo([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, FileNameGenerator.GenerateOutputFileName(MP4));

        if (!System.IO.File.Exists(inputPath))
        {
            _logger.LogWarning($"{GlobalConstants.Messages.InputFileNotFound}: {inputPath}");
            return BadRequest(GlobalConstants.Messages.InputFileNotFound);
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

    [Authorize]
    [HttpPost("snapshot")]
    public IActionResult CreateSnapshot([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, FileNameGenerator.GenerateOutputFileName(JPG));

        if (!System.IO.File.Exists(inputPath))
        {
            return BadRequest(GlobalConstants.Messages.InputFileNotFound);
        }

        // Use SkiaSharp to save the snapshot
        var bitmap = FFMpegCore.Extensions.SkiaSharp.FFMpegImage.Snapshot(inputPath, new Size(200, 400), TimeSpan.FromSeconds(10));

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100); // Save as JPEG
        using var stream = System.IO.File.OpenWrite(outputFilePath);
        data.SaveTo(stream);

        return Ok(new { Message = "Snapshot created successfully.", OutputPath = outputFilePath });
    }

    [Authorize]
    [HttpPost("extract-audio")]
    public IActionResult ExtractAudio([FromBody] VideoProcessingRequest request)
    {
        var inputPath = Path.Combine(UploadsPath, request.InputFileName);
        var outputFilePath = Path.Combine(OutputPath, FileNameGenerator.GenerateOutputFileName(MP3));

        if (!System.IO.File.Exists(inputPath))
        {
            return BadRequest("Input file not found.");
        }

        FFMpeg.ExtractAudio(inputPath, outputFilePath);

        return Ok(new { Message = "Audio extracted successfully.", OutputPath = outputFilePath });
    }

    [Authorize]
    [HttpPost("join-videos")]
    public IActionResult JoinVideos([FromBody] JoinVideosRequest request)
    {
        var outputFilePath = Path.Combine(OutputPath, FileNameGenerator.GenerateOutputFileName(MP4));

        foreach (var file in request.InputFileNames)
        {
            if (!System.IO.File.Exists(Path.Combine(UploadsPath, file)))
            {
                return BadRequest($"{GlobalConstants.Messages.FileNotFound}: {file}");
            }
        }

        var inputFiles = request.InputFileNames.Select(file => Path.Combine(UploadsPath, file)).ToArray();
        FFMpeg.Join(outputFilePath, inputFiles);

        return Ok(new { Message =  GlobalConstants.Messages.VideosJoinedSuccessfully, OutputPath = outputFilePath });
    }

    [Authorize]
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
            var inputPath = Path.Combine(UploadsPath, $"input{MP4}");
            var outputPath = Path.Combine(OutputPath, FileNameGenerator.GenerateOutputFileName(MP4));

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

    /// <summary>
    /// MergeVideos
    /// </summary>
    /// Manual Test - Passed
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("merge-videos")]
    public IActionResult MergeVideos([FromBody] List<string> request)
    {
        if (request == null || request.Count < 2)
        {
            return BadRequest("At least two video paths are required.");
        }

        try
        {
            var tempPath = GetLocalTempPath();

            // Ensure the directory exists
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempFiles = new List<string>();

            foreach (var url in request)
            {
                try
                {
                    // Check if the file exists at the given URL
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(url).Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest($"{GlobalConstants.Messages.FileatUrl} '{url}' does not exist or cannot be accessed.");
                        }

                        // Download the file to the temp folder
                        var fileName = Path.GetFileName(new Uri(url).LocalPath);
                        var localFilePath = Path.Combine(tempPath, fileName);

                        using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            response.Content.CopyToAsync(fileStream).Wait();
                        }

                        tempFiles.Add(localFilePath);
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error processing file at URL '{url}': {ex.Message}");
                }
            }

            // Output path for the merged video
            var outputDirectory = Path.Combine(GlobalConstants.FilePaths.WWWROOT, GlobalConstants.FilePaths.OUTPUT);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var outputFileName = FileNameGenerator.GenerateOutputFileName(MP4);
            var outputPath = Path.Combine(outputDirectory, outputFileName);

            // Use FFMpeg to merge the downloaded files
            FFMpeg.Join(outputPath, tempFiles.ToArray());

            // Clean up temporary files
            foreach (var file in tempFiles)
            {
                if (System.IO.File.Exists(file))
                {
                    System.IO.File.Delete(file);
                }
            }

            // Generate a downloadable URL for the output file
            var outputUrl = $"{Request.Scheme}://{Request.Host}/{OUTPUT}/{outputFileName}";

            return Ok(new
            {
                Success = true,
                OutputPath = outputUrl
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

    /// <summary>
    /// GetLocalTempPath
    /// </summary>
    /// <returns></returns>
    private static string GetLocalTempPath()
    {
        // Get the absolute path for the temporary folder
        var tempBasePath = Help.PrepareTempFolder(); // System temp directory
        var tempPath = Path.Combine(tempBasePath, GlobalConstants.AppInfo.Name, GlobalConstants.FilePaths.TEMP);
        return tempPath;
    }

    /// <summary>
    /// MergeAudioWithVideo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("merge-audio-video")]
    public IActionResult MergeAudioWithVideo([FromBody] List<string> request)
    {
        // Validate the list
        var isValid = ValidateRequest(request, 2, ".mp3;.mp4");

        if (!isValid)
        {
            return BadRequest("Both video and audio paths are required.");
        }

        try
        {
            // Get the absolute path for the temporary folder
            var tempBasePath = Path.GetTempPath(); // System temp directory
            var tempPath = Path.Combine(tempBasePath, GlobalConstants.AppInfo.Name, GlobalConstants.FilePaths.TEMP);

            // Ensure the directory exists
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempVideoFiles = new List<string>();
            var tempAudioFiles = new List<string>();

            foreach (var url in request)
            {
                try
                {
                    // Check if the file exists at the given URL
                    using (var client = new HttpClient())
                    {
                        var response = client.GetAsync(url).Result;

                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest($"{GlobalConstants.Messages.FileatUrl} '{url}' does not exist or cannot be accessed.");
                        }

                        // Download the file to the temp folder
                        var fileName = Path.GetFileName(new Uri(url).LocalPath);
                        var localFilePath = Path.Combine(tempPath, fileName);

                        using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                        {
                            response.Content.CopyToAsync(fileStream).Wait();
                        }

                        if (localFilePath.EndsWith(MP3))
                        {
                            tempAudioFiles.Add(localFilePath);
                        }
                        else if (localFilePath.EndsWith(MP4))
                        {
                            tempVideoFiles.Add(localFilePath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error processing file at URL '{url}': {ex.Message}");
                }
            }

            // Output path for the merged video
            var outputDirectory = Path.Combine(GlobalConstants.FilePaths.WWWROOT, GlobalConstants.FilePaths.OUTPUT);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var outputFileName = FileNameGenerator.GenerateOutputFileName(MP4);
            var outputPath = Path.Combine(outputDirectory, outputFileName);

            // Merge the audio and video
            FFMpeg.ReplaceAudio(tempVideoFiles[0], tempAudioFiles[0], outputPath, true);

            // Generate a downloadable URL for the output file
            var outputUrl = $"{Request.Scheme}://{Request.Host}/{OUTPUT}/{outputFileName}";

            return Ok(new
            {
                Success = true,
                OutputPath = outputUrl
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

    [Authorize]
    [HttpPost("add-audio")]
    public IActionResult AddAudio([FromBody] AddAudioRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoPath) || string.IsNullOrEmpty(request.AudioPath))
        {
            return BadRequest("Both video and audio paths are required.");
        }

        try
        {
            var outputPath = Path.Combine(OUTPUT, FileNameGenerator.GenerateOutputFileName(MP4));
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost("add-subtitles")]
    public IActionResult AddSubtitles([FromBody] AddSubtitleRequest request)
    {
        if (string.IsNullOrEmpty(request.VideoPath) || string.IsNullOrEmpty(request.SubtitlePath))
        {
            return BadRequest("Both video and subtitle paths are required.");
        }

        try
        {
            var outputPath = Path.Combine(OUTPUT, FileNameGenerator.GenerateOutputFileName(MP4));
            var subtitleFilePath = request.SubtitlePath;

            var mp4File = request.VideoPath.EndsWith(MP4, StringComparison.OrdinalIgnoreCase) ? request.VideoPath : "";

            // Get the absolute path for the temporary folder
            var tempBasePath = Path.GetTempPath(); // System temp directory
            var tempPath = Path.Combine(tempBasePath, GlobalConstants.AppInfo.Name, GlobalConstants.FilePaths.TEMP);

            // Ensure the directory exists
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempVideoFiles =   DownloadUrlToLocalPath(mp4File, tempPath, MP4);
            var tempSubtitleFiles = DownloadUrlToLocalPath(subtitleFilePath, tempPath, SRT);

            FFMpegArguments
                     .FromFileInput(tempVideoFiles)
                     .OutputToFile(outputPath, false, opt => opt
                         .WithVideoFilters(filterOptions => filterOptions
                             .HardBurnSubtitle(SubtitleHardBurnOptions
                                 .Create(subtitlePath: tempSubtitleFiles)
                                 .SetCharacterEncoding("UTF-8")
                                 .SetSubtitleIndex(0)
                                 .WithStyle(StyleOptions.Create()
                                     .WithParameter("FontName", "DejaVu Serif")
                                     .WithParameter("PrimaryColour", "&HAA00FF00")
                                     )))
                         )
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

    [HttpPost("add-caption")]
    public IActionResult AddCaption([FromBody] AddCaptionRequest payload)
    {
        if (payload == null || string.IsNullOrEmpty(payload.Request) || string.IsNullOrEmpty(payload.Caption))
        {
            return BadRequest("Request and Caption are required.");
        }

        try
        {
            var mp4File = payload.Request.EndsWith(MP4, StringComparison.OrdinalIgnoreCase) ? payload.Request : "";

            // Get the absolute path for the temporary folder
            var tempBasePath = Path.GetTempPath(); // System temp directory
            var tempPath = Path.Combine(tempBasePath, GlobalConstants.AppInfo.Name, GlobalConstants.FilePaths.TEMP);
            //var tempPath = Help.GetAbsoluteLocalTempFilename(GlobalConstants.FilePaths.TEMP, MP4);
            // Ensure the directory exists
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempLocalVideoFile = DownloadUrlToLocalPath(mp4File, tempPath,MP4);

            if (tempLocalVideoFile.Equals("404"))
            {
                throw new FileNotFoundException($"Video file not found: {tempLocalVideoFile}");
            }             
 

            var outputPath = Help.GetAbsoluteLocalTempFilename(GlobalConstants.FilePaths.OUTPUT, MP4);
              
            FFMpegArguments
                .FromFileInput(tempLocalVideoFile)
                .OutputToFile(outputPath, false, opt => opt
                    .WithVideoFilters(filterOptions => filterOptions
                        .DrawText(DrawTextOptions
                            .Create(payload.Caption, "DejaVu Serif")
                            .WithParameter("fontcolor", "white")
                            .WithParameter("fontsize", "24")
                            .WithParameter("box", "1")
                            .WithParameter("boxcolor", "black@0.5")
                            .WithParameter("boxborderw", "5")
                            .WithParameter("x", "(w-text_w)/2")
                            .WithParameter("y", "(h-text_h)/2"))))
                .ProcessSynchronously();

            // Simulate a successful response
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

    private static string DownloadUrlToLocalPath(string url, string tempPath, string extension)
    {

        var tempLocalFile = "404";

        try
        {
            // Check if the file exists at the given URL
            using (var client = new HttpClient())
            {
                var response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    //return  ($"{GlobalConstants.Messages.FileatUrl} '{url}' does not exist or cannot be accessed.");
                    return tempLocalFile;
                }

                // Download the file to the temp folder
                var _fileName = Path.GetFileName(new Uri(url).LocalPath);
                var localFilePath = Path.Combine(tempPath, _fileName);

                using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                {
                    response.Content.CopyToAsync(fileStream).Wait();
                }

                if (localFilePath.EndsWith(extension))
                {
                    tempLocalFile = localFilePath;
                }
            }

            return tempLocalFile;
        }
        catch (Exception)
        {
            return tempLocalFile;            
        }
    }

    [Authorize]
    [HttpGet("jobs")]
    public IActionResult GetJobs()
    {
        var jobResults = new[]
        {
        new { Name = "AnalyseVideo", Status = "Success", Details = "Video analysis completed successfully." },
        new { Name = "ConvertVideo", Status = "Error", Details = "File format not supported." },
        // Add more jobs here
    };

        return Ok(jobResults);
    }

    private static async Task ProcessVideoWithProgress(string inputPath, string outputPath,
        IProgress<int> progress, CancellationToken cancellationToken)
    {
        var totalFrames = 100; // Example: Total frames (or percentage steps)
        for (var i = 0; i <= totalFrames; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new TaskCanceledException(GlobalConstants.Messages.ProcessingWasCanceled);
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

    private static bool ValidateRequest(List<string> request,int requestNum,  string extensions)
    {
        // Check 1: List must contain more than 2 items
        if (request.Count < requestNum) 
        {
            return false;
        }

        try
        {
            var validExtensions = extensions.Split(";");

            if (validExtensions.Length == 0)
            {
                throw new ArgumentException("No valid extensions provided.");
            }

            // Check for invalid extensions in the request
            var invalidFiles = request.Where(file =>
                !validExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            if (invalidFiles.Any())
            {
                Console.WriteLine("The following files have invalid extensions:");
                foreach (var invalidFile in invalidFiles)
                {
                    Console.WriteLine(invalidFile);
                }

                return false; // Or handle invalid files as needed
            }

            // Find matching files with valid extensions
            var matchingFiles = request.Where(file =>
                validExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            return matchingFiles.Count >= 2;

        }
        catch (Exception  )
        {
            
            return false;
        }
    }   
}
