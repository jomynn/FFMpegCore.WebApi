namespace FFMpegCore.WebApi.Models
{
    public abstract class Element
    {
        public required string Type { get; set; }
    }

    public class VideoElement : Element
    {
        public  string Src { get; set; } = "";
    }

    public class ImageElement : Element
    {
        public  string Src { get; set; } = "";
        public int? X { get; set; }
        public int? Y { get; set; }
    }

    public class AudioElement : Element
    {
        public  string Src { get; set; } = "";
        public double? Start { get; set; }
    }

    public class TextElement : Element
    {
        public  string Text { get; set; } = "";
        public string Font { get; set; } = "Roboto Mono";
        public string Color { get; set; } = "white";
    }
}
