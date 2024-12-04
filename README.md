# FFMpegCore.WebApi

**FFMpegCore.WebApi** is a feature-enhanced project built on the foundation of [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore). It provides a web API for video and audio processing tasks, making it easy to integrate FFMpeg functionalities with JSON-based requests and responses. 

This project allows users to merge videos, add audio, extract audio, and more, with output files available as downloadable URLs.

---

## **Features**

### **Core Functionalities**
- **Video Processing**: Merge multiple videos, add subtitles, or overlay audio tracks.
- **Audio Processing**: Extract or replace audio from video files.
- **Logging**: Track render jobs with detailed logs.
- **Downloadable Outputs**: All processed files are accessible via unique URLs for easy downloading.

### **Enhanced Capabilities**
- **Authentication**: Secure API access with JWT-based user login.
- **API Keys**: Access endpoints using API keys for automated workflows.
- **Webhook Support**: Receive notifications when processing jobs are completed.

---

## **How to Use**

### **Download the Project**

1. Clone or download the repository:
 
   git clone https://github.com/your-repo/FFMpegCore.WebApi.git


2. Navigate to the project directory:
 
cd FFMpegCore.WebApi

## Run the Application

# Install dependencies:

```bash
dotnet restore
```

# Build the project:

```bash
dotnet build
```

# Run the application:

```bash
dotnet run
```

The application will start at:

https://localhost:5001 (default)
Download Processed Files
Once video or audio processing is complete, the API returns a URL for the output file. You can download it directly using the URL provided.

Example Usage
Merge Videos
Request:
```bash
json
POST /api/ffmpeg/merge-videos
Content-Type: application/json

[
    "https://localhost:5001/sample/input/video1.mp4",
    "https://localhost:5001/sample/input/video2.mp4"
]
```

Response:
```bash
json
 
{
    "Success": true,
    "OutputPath": "https://localhost:5001/output/merged-video.mp4"
}
```

Download:
Access the OutputPath URL to download the merged video:
```bash
https://localhost:5001/output/merged-video.mp4
```

## File Structure
plaintext
 
FFMpegCore.WebApi/
├── wwwroot/
│   ├── sample/
│   │   └── input/       # Sample input files for testing
│   └── output/          # Directory for downloadable output files
├── Controllers/
│   ├── FFMpegController.cs
│   ├── AuthController.cs
│   └── WebhookController.cs
├── Models/
│   ├── LoginRequest.cs
│   ├── MergeVideosRequest.cs
│   └── RenderLog.cs
├── Logs/                # Directory for render logs
├── Program.cs           # Main entry point for the application
├── appsettings.json     # Configuration file
└── README.md            # Project documentation

# Downloadable Output
All processed files are saved in the wwwroot/output directory. The API provides a URL pointing to these files, allowing for direct downloads.

# Example Output Path
```bash
https://localhost:5001/output/processed-file.mp4
```

# System Requirements
.NET 8 or later
FFmpeg binaries (configured in the system path)
Contributing
Contributions are welcome! To contribute:

Fork the repository.
Create a feature branch:
```bash
 
git checkout -b feature-name
```

# Commit your changes:
```bash
 
git commit -m "Add new feature"
```
# Push your branch:
```bash
 
git push origin feature-name
```

# Open a Pull Request.
License
This project is licensed under the MIT License. See the LICENSE file for details.

Acknowledgments
This project is based on the excellent work in FFMpegCore by Rosen Bjerg.

vbnet


Acknowledgments
```bash
Special thanks to <a href="https://github.com/rosenbjerg">Rosen Bjerg</a> for the original <a href="https://github.com/rosenbjerg/FFMpegCore">FFMpegCore</a> project that made this enhancement possible.
```


This version emphasizes the "output" aspect of the project, ensuring users understand how to ac



This `README.md` focuses on providing clear instructions for downloading and using the project, emphasizing the downloadable output feature. Let me know if further customization is needed! 🚀

### License

Copyright © 2023

Released under [MIT license](https://github.com/rosenbjerg/FFMpegCore/blob/master/LICENSE)


<a class="no-underline" href="https://buymeacoffee.com/jomynn"><img data-testid="logo-img" src="./Buymeacoffee.png" alt="Knowledge Base | Buy Me a Coffee" class="max-h-8 contrast-80 inline"> <br /> Buy me Coffee </a>
    
