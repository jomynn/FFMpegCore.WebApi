namespace FFMpegCore.WebApi.Models
{
    public class Video
    {
        public required string Src { get; set; } // Video source URL or path
    }

    public class Audio
    {
        public required string Src { get; set; } // Audio source URL or path
    }

    public class Subtitle
    {
        public required string Language { get; set; } // Subtitle language
        public required string Src { get; set; } // Subtitle file URL or path
    }

    public class Caption
    {
        public required string Text { get; set; } // Caption text
        public int Start { get; set; } // Start time in seconds
        public int End { get; set; } // End time in seconds
        public required CaptionStyle Style { get; set; } // Optional style
    }

    public class CaptionStyle
    {
        public required string Font { get; set; } // Font name
        public int Size { get; set; } // Font size
        public required string Color { get; set; } // Text color (e.g., "#FFFFFF")
        public required string BackgroundColor { get; set; } // Background color
        public required string Position { get; set; } // Position (e.g., "top", "bottom")
    }

    public class Overlay
    {
        public required OverlayImage Image { get; set; } // Overlay image
        public int Start { get; set; } // Start time in seconds
        public int End { get; set; } // End time in seconds
    }

    public class OverlayImage
    {
        public required string Src { get; set; } // Image source URL or path
        public int X { get; set; } // X-coordinate
        public int Y { get; set; } // Y-coordinate
        public int Width { get; set; } // Image width
        public int Height { get; set; } // Image height
    }
}
