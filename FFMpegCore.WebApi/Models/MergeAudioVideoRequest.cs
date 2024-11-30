public class MergeAudioVideoRequest
{
    public required string VideoFileBase64 { get; set; } // Base64 string of the video file
    public required string AudioFileBase64 { get; set; } // Base64 string of the audio file
}
