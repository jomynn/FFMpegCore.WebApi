using FFPmpegCore.Global;

namespace FFMpegCore.WebApi.Utils
{
    public static class Help
    {
        /// <summary>
        /// Sample
        /// outputPath = "wwwroot\\output\\9d5d9ca8-5549-43d4-991a-3147adbe9bcf.mp4"
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetRelateLocalTempFilename(string folder, string extension)
        {
            if (string.IsNullOrEmpty(folder))
            {
                throw new ArgumentException($"'{nameof(folder)}' cannot be null or empty.", nameof(folder));
            }

            var outputDirectory = Path.Combine(GlobalConstants.FilePaths.WWWROOT, folder);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            var outputFileName = FileNameGenerator.GenerateOutputFileName(extension);
            var outputPath = Path.Combine(outputDirectory, outputFileName);
            return outputPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GetAbsoluteLocalTempFilename(string folder, string extension)
        {
            // Get the absolute path of the output directory
            var outputDirectory = Path.GetFullPath(Path.Combine(GlobalConstants.FilePaths.WWWROOT, folder));

            // Ensure the directory exists
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            // Generate the output file name
            var outputFileName = FileNameGenerator.GenerateOutputFileName(extension);

            // Combine the directory and file name to get the absolute path
            var outputPath = Path.Combine(outputDirectory, outputFileName);

            // Return the absolute path
            return outputPath;
        }

        // Helper method to prepare temp folder
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string PrepareTempFolder()
        {
            var tempBasePath = Path.GetTempPath();
            var tempPath = Path.Combine(tempBasePath, GlobalConstants.AppInfo.Name, GlobalConstants.FilePaths.TEMP);

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            return tempPath;
        }

        // Helper method to download file from URL
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <param name="tempPath"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string DownloadFile(string fileUrl, string tempPath)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = httpClient.GetAsync(fileUrl).Result;

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"The file at URL '{fileUrl}' does not exist or cannot be accessed.");
                    }

                    var fileName = Path.GetFileName(new Uri(fileUrl).LocalPath);
                    var localFilePath = Path.Combine(tempPath, fileName);

                    using (var fileStream = new FileStream(localFilePath, FileMode.Create))
                    {
                        response.Content.CopyToAsync(fileStream).Wait();
                    }

                    return localFilePath;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error downloading file from URL '{fileUrl}': {ex.Message}");
            }
        }
    }
}
