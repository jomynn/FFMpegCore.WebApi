namespace FFMpegCore.WebApi.Models
{
    public class VideoRequest
    {
        public string Resolution { get; set; } = "full-hd";
        public string Quality { get; set; } = "high";
        public required List<Scene> Scenes { get; set; }
    }
}
