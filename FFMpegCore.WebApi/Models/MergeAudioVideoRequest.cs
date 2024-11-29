public class MergeAudioVideoRequest
{
    public required IFormFile AudioFile { get; set; }
    public required IFormFile VideoFile { get; set; }
    public required string OutputFileName { get; set; }
}
