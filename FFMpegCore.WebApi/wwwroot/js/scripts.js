// Dynamically set the API base URL using the current host and port
import { apiBaseUrl } from './shared.js';

console.log("API Base URL:", apiBaseUrl); // Debugging log to verify the base URL

//const apiBaseUrl = "http://localhost:5148/api/FFMpeg";

// Merge Multiple Videos
async function mergeVideos(event) {
    event.preventDefault();

    try {
        const token = localStorage.getItem("authToken");
        if (!token) {
            console.error("Token not found in localStorage");
            throw new Error("Authentication token not found. Please log in.");
        }

        const videoFilesInput = document.getElementById("videoPaths");

        if (!videoFilesInput || videoFilesInput.files.length < 2) {
            throw new Error("Please select at least two video files.");
        }

        const videoFiles = Array.from(videoFilesInput.files);
        const videoPaths = await Promise.all(
            videoFiles.map(file => toBase64(file))
        );

        const payload = {
            VideoPaths: videoPaths
        };

        const response = await fetch(`${apiBaseUrl}/merge-videos`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.Error || "An error occurred while merging videos.");
        }

        const result = await response.json();
        document.getElementById("outputMessage").innerText = `Merged video successfully: ${result.OutputPath}`;
    } catch (error) {
        console.error("Error merging videos:", error);
        document.getElementById("outputMessage").innerText = `Error: ${error.message}`;
    }
}





// Merge Audio + Video
async function mergeAudioVideo(event) {
    event.preventDefault();

    const formData = new FormData();
    const audioFile = document.getElementById("audioFile").files[0];
    const videoFile = document.getElementById("videoFileForAudio").files[0];

    formData.append("audioFile", audioFile);
    formData.append("videoFile", videoFile);

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
