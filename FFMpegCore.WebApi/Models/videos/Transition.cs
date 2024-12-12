namespace FFMpegCore.WebApi.Models
{
    public class Transition
    {
        public required string Style { get; set; } // Example: "circleopen", "fade"
        public double Duration { get; set; } // Duration in seconds
    }
}
