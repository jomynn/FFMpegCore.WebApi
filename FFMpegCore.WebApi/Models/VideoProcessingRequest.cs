public class VideoProcessingRequest
{
    public required string InputFileName { get; set; }
    public required string OutputFileName { get; set; }
    public int Width { get; set; } = 200; // Default width
    public int Height { get; set; } = 400; // Default height
}
