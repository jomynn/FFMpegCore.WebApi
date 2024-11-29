public class FileUploadRequest
{
    public required string AudioFileBase64 { get; set; }
    public required string VideoFileBase64 { get; set; }
    public required string OutputFileName { get; set; }
}
