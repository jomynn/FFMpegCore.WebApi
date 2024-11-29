public class MergeVideosRequest
{
    public required List<string> VideoPaths { get; set; } // Paths or URLs of videos
    public required string OutputFileName { get; set; }
}
