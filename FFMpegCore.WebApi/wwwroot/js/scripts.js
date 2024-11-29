const apiBaseUrl = "http://localhost:5148/api/FFMpeg";

// Merge Multiple Videos
async function mergeVideos(event) {
    event.preventDefault();

    const formData = new FormData();
    const files = document.getElementById("videoFiles").files;
    const outputFileName = document.getElementById("mergedVideoName").value || "merged.mp4";

    for (const file of files) {
        formData.append("files", file);
    }
    formData.append("outputFileName", outputFileName);

    const response = await fetch(`${apiBaseUrl}/merge-videos`, {
        method: "POST",
        body: formData,
    });

    const result = await response.json();
    document.getElementById("outputMessage").innerText = `Merged Video: ${result.outputPath}`;
}

// Merge Audio + Video
async function mergeAudioVideo(event) {
    event.preventDefault();

    const formData = new FormData();
    const audioFile = document.getElementById("audioFile").files[0];
    const videoFile = document.getElementById("videoFileForAudio").files[0];
    const outputFileName = document.getElementById("audioVideoOutputName").value || "audio_video.mp4";

    formData.append("audioFile", audioFile);
    formData.append("videoFile", videoFile);
    formData.append("outputFileName", outputFileName);

    const response = await fetch(`${apiBaseUrl}/merge-audio-video`, {
        method: "POST",
        body: formData,
    });

    const result = await response.json();
    document.getElementById("outputMessage").innerText = `Merged Audio + Video: ${result.outputPath}`;
}

// Attach event listeners
document.getElementById("mergeVideosForm").addEventListener("submit", mergeVideos);
document.getElementById("mergeAudioVideoForm").addEventListener("submit", mergeAudioVideo);
