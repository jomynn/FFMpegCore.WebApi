namespace FFMpegCore.WebApi.Models
{
    public class Body
    {
        public List<string>? Text { get; set; } // List of text lines
        public string Color { get; set; } = "400";// Example: "white"
        public string FontFamily { get; set; } = "Roboto Mono";// Example: "Roboto Mono"
        public string FontWeight { get; set; } = "400";// Example: "400"
    }
}
