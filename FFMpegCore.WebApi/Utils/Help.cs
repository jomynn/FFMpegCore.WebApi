using FFPmpegCore.Global;

namespace FFMpegCore.WebApi.Utils
{
    public static class Help
    {
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
