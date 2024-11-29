namespace FFMpegCore.WebApi.Models
{
    public class RenderLog
    {
        public Guid VideoId { get; set; }
        public required string status {  get; set; }
        public required string Resolution { get; set; }
        public required int DurationSeconds { get; set; }
        public required int Renderingtime { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
