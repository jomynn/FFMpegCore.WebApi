// Dynamically set the API base URL using the current host and port
import { apiBaseUrl } from './shared.js';

console.log("API Base URL:", apiBaseUrl); // Debugging log to verify the base URL

 
export async function mergeVideos(event) {
    event.preventDefault();

    try {
        const videoFilesInput = document.getElementById("videoPaths");

        if (!videoFilesInput || videoFilesInput.files.length < 2) {
            throw new Error("Please select at least two video files.");
        }

        // Extract video paths (e.g., filenames)
        const videoPaths = Array.from(videoFilesInput.files).map(file => file.name);

        const payload = videoPaths; // Send the array of file names directly

        // Retrieve the JWT token from localStorage
        const token = localStorage.getItem("authToken");
        if (!token) {
            throw new Error("Authentication token is missing. Please log in again.");
        }

        const response = await fetch(`${apiBaseUrl}/ffmpeg/merge-videos`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}` // Include the token
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to merge videos: ${errorText}`);
        }

        const result = await response.json();
        document.getElementById("outputMessage").innerText = `Merged Video Path: ${result.OutputPath}`;
    } catch (error) {
        console.error("Error merging videos:", error);
        document.getElementById("outputMessage").innerText = `Error: ${error.message}`;
    }
}


// Utility function to convert a file to Base64
function toBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
        reader.readAsDataURL(file);
    });
}







// Merge Audio + Video
async function mergeAudioVideo(event) {
    event.preventDefault();

    const formData = new FormData();
    const audioFile = document.getElementById("audioFile").files[0];
    const videoFile = document.getElementById("videoFileForAudio").files[0];

    formData.append("audioFile", audioFile);
    formData.append("videoFile", videoFile);

    const response = await fetch(`${apiBaseUrl}/ffmpeg/merge-audio-video`, {
        method: "POST",
        body: formData,
    });

    const result = await response.json();
    document.getElementById("outputMessage").innerText = `Merged Audio + Video: ${result.outputPath}`;
}

// Attach event listeners
document.getElementById("mergeVideosForm").addEventListener("submit", mergeVideos);
document.getElementById("mergeAudioVideoForm").addEventListener("submit", mergeAudioVideo);
