namespace FFPmpegCore.Global
{
    public static class GlobalConstants
    {
        public static class ApiEndpoints
        {
            public const string BaseUrl = "https://api.example.com";
            public const string Login = "/auth/login";
            public const string Logout = "/auth/logout";
        }

        public static class Messages
        {
            public const string Unauthorized = "Unauthorized access.";
            public const string NotFound = "The requested resource was not found.";
            public const string Success = "Operation completed successfully.";
            public const string EndpointProtect = "This is a protected endpoint!";
            public const string ApiWorking = "API is working";
            public const string InputFileNotFound = "Input file not found";
            public const string SuccessAnalysedVideo = "Successfully analysed video";
            public const string FileatUrl = "File at URL";
            public const string FileNotFound = "File not found";
            public const string ProcessingWasCanceled = "Processing was canceled.";
            public const string MergeSuccessfull = "Merge successful";
            public const string VideosJoinedSuccessfully = "Videos joined successfully.";
        }

        public static class FilePaths
        {
            public const string Logs = "logs/app.log";
            public const string Config = "config/appsettings.json";
            public const string MP4 = ".mp4";
            public const string MP3 = ".mp3";
            public const string JPG = ".jpg";
            public const string SRT = ".srt";
            public const string OUTPUT = "output";
            public const string UPLOADS = "uploads";
            public const string TEMP = "temp";
            public const string WWWROOT = "wwwroot";
        }

        public static class AppInfo
        {
            public const string Name = "FFPmpegCore.WebApi";
            public const string Version = "1.0.0";
        }
    }
}
