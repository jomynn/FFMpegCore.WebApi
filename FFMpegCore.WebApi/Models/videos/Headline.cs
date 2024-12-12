namespace FFMpegCore.WebApi.Models
{
    public class Headline
    {
        public List<string>? Text { get; set; } // List of text lines
        public string Color { get; set; } = "white"; // Example: "white"
        public string FontFamily { get; set; } = "Roboto Mono"; // Example: "Roboto Mono"
        public string FontWeight { get; set; } = "400"; // Example: "400"
        public string LetterSpacing { get; set; } = "0.5px"; // Example: "0.5px"
    }
}
