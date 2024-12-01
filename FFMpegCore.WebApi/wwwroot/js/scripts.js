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
        document.getElementById("outputMessage").innerText = `Merged Video Path: ${result.outputPath}`;
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

// Fetch the list of files from the server and populate checkboxes
async function fetchFileList() {
    try {
        const response = await fetch(`${apiBaseUrl}/file/list-files`);
        if (!response.ok) {
            throw new Error("Failed to fetch file list.");
        }

        const files = await response.json();
        const fileCheckboxContainer = document.getElementById("fileCheckboxContainer");

        // Clear any existing checkboxes
        fileCheckboxContainer.innerHTML = "";

        // Populate checkboxes
        files.forEach(file => {
            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.value = file.fullPath; // Use the full path as the value
            checkbox.id = `file-${file.fileName}`;
            checkbox.className = "file-checkbox";

            const label = document.createElement("label");
            label.htmlFor = `file-${file.fileName}`;
            label.textContent = file.fileName;

            const div = document.createElement("div");
            div.appendChild(checkbox);
            div.appendChild(label);

            fileCheckboxContainer.appendChild(div);
        });
    } catch (error) {
        console.error("Error fetching file list:", error);
        document.getElementById("outputMessage").innerText = `Error: ${error.message}`;
    }
}


// Update the textarea with selected files
function updateSelectedFiles() {
    const selectedFiles = Array.from(
        document.querySelectorAll(".file-checkbox:checked")
    ).map(checkbox => checkbox.value);

    document.getElementById("selectedFiles").value = selectedFiles.join("\n");
}

// Attach event listener to update textarea when checkboxes change
document.getElementById("fileCheckboxContainer").addEventListener("change", updateSelectedFiles);

// Attach event listener to update textarea when checkboxes change
document.getElementById("fileCheckboxContainer").addEventListener("change", updateSelectedFiles);

// Fetch file list on page load
fetchFileList();

// Handle form submission
document.getElementById("mergeVideosForm").addEventListener("submit", async function (event) {
    event.preventDefault();

    const selectedFiles = document.getElementById("selectedFiles").value.split("\n").filter(file => file.trim());
    if (selectedFiles.length < 2) {
        alert("Please select at least two files.");
        return;
    }

    try {
        const token = localStorage.getItem("authToken");
        if (!token) {
            throw new Error("Authentication token is missing. Please log in again.");
        }

        const response = await fetch(`${apiBaseUrl}/ffmpeg/merge-videos`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(selectedFiles)
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
});

document.getElementById("mergeAudioVideoForm").addEventListener("submit", mergeAudioVideo);
