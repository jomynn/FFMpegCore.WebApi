using FFPmpegCore.Global;

public static class FileNameGenerator
{
    public static string GenerateOutputFileName(string extension)
    {
        // Validate and ensure the extension starts with a dot (e.g., ".mp4")
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }

        // Generate a GUID and append the extension
        return Guid.NewGuid().ToString() + extension;
    }
    public static string GetAbsLocalTempFilename(string extension)
    {
        var outputDirectory = Path.Combine(GlobalConstants.FilePaths.WWWROOT, GlobalConstants.FilePaths.OUTPUT);
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        var outputFileName = GenerateOutputFileName(extension);
        var outputPath = Path.Combine(outputDirectory, outputFileName);
        return outputPath;
    }
}
