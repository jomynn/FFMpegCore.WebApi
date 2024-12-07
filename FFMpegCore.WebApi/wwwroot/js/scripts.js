// Dynamically set the API base URL using the current host and port
import { apiBaseUrl } from './shared.js';

// Centralized API Endpoints
const endpoints = {
    mergeVideos: `${apiBaseUrl}/ffmpeg/merge-videos`,
    mergeAudioVideo: `${apiBaseUrl}/ffmpeg/merge-audio-video`,
    listFiles: `${apiBaseUrl}/file/list-files`,
    analyze: `${apiBaseUrl}/analyse`,
};

console.log("API Base URL:", apiBaseUrl); // Debugging log to verify the base URL

/*
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
 */


export async function mergeVideos(event) {
    event.preventDefault();

    try {
        // Get all checked checkboxes
        const selectedFiles = Array.from(document.querySelectorAll(".file-checkbox:checked"))
            .map(checkbox => checkbox.value);

        // Validate that at least two files are selected
        if (selectedFiles.length < 2) {
            throw new Error("Please select at least two video files.");
        }

        // The payload should be a flat array of strings
        const payload = selectedFiles;

        // Retrieve the JWT token from localStorage
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
            body: JSON.stringify(payload) // Send the array of strings
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to merge videos: ${errorText}`);
        }

        const result = await response.json();
        document.getElementById("outputMessage1").innerText = `Merged Video Path: ${result.outputPath}`;
    } catch (error) {
        console.error("Error merging videos:", error);
        document.getElementById("outputMessage1").innerText = `Error: ${error.message}`;
    }
}

// Merge Audio + Video
export async function mergeAudioVideo(event) {
    event.preventDefault();
    try {
        // Get all checked checkboxes
        const selectedFiles = Array.from(document.querySelectorAll(".file-checkbox2:checked"))
            .map(checkbox => checkbox.value);

        // Validate that at least two files are selected
        if (selectedFiles.length < 2) {
            throw new Error("Please select at least two video files.");
        }

        // The payload should be a flat array of strings
        const payload = selectedFiles;

        // Retrieve the JWT token from localStorage
        const token = localStorage.getItem("authToken");
        if (!token) {
            throw new Error("Authentication token is missing. Please log in again.");
        }

        const response = await fetch(`${apiBaseUrl}/ffmpeg/merge-audio-video`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload) // Send the array of strings
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to merge videos: ${errorText}`);
        }

        const result = await response.json();
        document.getElementById("outputMessage2").innerText = `Merged Audio + Video: ${result.outputPath}`;
    } catch (error) {
        console.error("Error merging Audio + Video:", error);
        document.getElementById("outputMessage2").innerText = `Error: ${error.message}`;
    }
    
}

export async function analyseMedia(event) {
    event.preventDefault(); // Prevent default form submission

    try {
        // Get all checked checkboxes
        const selectedFiles = Array.from(document.querySelectorAll(".file-checkbox3:checked"))
            .map(checkbox => checkbox.value);

        // Validate that at least one file is selected
        if (selectedFiles.length < 1) {
            throw new Error("Please select at least a video file.");
        }

        // Convert the selected files into a single string
        // Assuming the API expects a single string (e.g., the first file or concatenated values)
        const payload = selectedFiles[0]; // Send the first selected file

        // Retrieve the JWT token from localStorage
        const token = localStorage.getItem("authToken");
        if (!token) {
            throw new Error("Authentication token is missing. Please log in again.");
        }

        const response = await fetch(`${apiBaseUrl}/ffmpeg/analyse`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload) // Send a single string as payload
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to Analyse Media: ${errorText}`);
        }

        const result = await response.json();
        purgeResult(result,"outputMessage3");

    } catch (error) {
        console.error("Error:", error);
        alert(`An error occurred: ${error.message}`);
    }
}

// Example function to process and display the result
function purgeResult(result, targetId) {
    // Check if the result object has the expected properties
    if (!result || typeof result !== 'object') {
        console.error("Invalid result object");
        return;
    }

    // Extract information
    const duration = result.duration || "Unknown duration";
    const videoStreams = result.videoStreams?.length || 0;
    const audioStreams = result.audioStreams?.length || 0;

    // Format the output
    const outputMessage = `
        Video Duration: ${duration}
        Video Streams: ${videoStreams}
        Audio Streams: ${audioStreams}
    `;

    // Display the formatted result in the desired output element
    const outputElement = document.getElementById(targetId);
    if (outputElement) {
        outputElement.innerText = outputMessage.trim();
    } else {
        console.warn("Output element not found");
    }

    // Log the detailed result to the console for debugging
    console.log("Detailed Result:", result);
}

// Example usage
const result = {
    duration: '00:02:29.7600000',
    videoStreams: [],
    audioStreams: [{}]
};




// Fetch the list of files from the server and populate checkboxes
// Consolidated Fetch File List Function
async function fetchFileList(containerId, checkboxClass) {
    try {
        const response = await fetch(endpoints.listFiles);
        if (!response.ok) {
            throw new Error("Failed to fetch file list.");
        }

        const files = await response.json();
        const container = document.getElementById(containerId);

        if (!container) return;

        container.innerHTML = ""; // Clear any existing checkboxes
        files.forEach((file) => {
            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.value = file.fullPath;
            checkbox.id = `file-${file.fileName}`;
            checkbox.className = checkboxClass;

            const label = document.createElement("label");
            label.htmlFor = `file-${file.fileName}`;
            label.textContent = file.fileName;

            const div = document.createElement("div");
            div.appendChild(checkbox);
            div.appendChild(label);

            container.appendChild(div);
        });
    } catch (error) {
        console.error("Error fetching file list:", error);
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

// Update the textarea with selected files
function updateSelectedFiles() {
    const selectedFiles = Array.from(
        document.querySelectorAll(".file-checkbox:checked")
    ).map(checkbox => checkbox.value);

    document.getElementById("selectedFiles").value = selectedFiles.join("\n");
}
function updateSelectedFiles2() {
    const selectedFiles = Array.from(
        document.querySelectorAll(".file-checkbox2:checked")
    ).map(checkbox => checkbox.value);

    document.getElementById("selectedFiles2").value = selectedFiles.join("\n");
}
function updateSelectedFiles3() {
    const selectedFiles = Array.from(
        document.querySelectorAll(".file-checkbox3:checked")
    ).map(checkbox => checkbox.value);

    document.getElementById("selectedFiles3").value = selectedFiles.join("\n");
}

// Attach Event Listeners with Error Handling
document.addEventListener("DOMContentLoaded", () => {
    const fileContainer1 = document.getElementById("fileCheckboxContainer");
    if (fileContainer1) {
        fileContainer1.addEventListener("change", updateSelectedFiles);
    }

    const fileContainer2 = document.getElementById("fileCheckboxContainer2");
    if (fileContainer2) {
        fileContainer2.addEventListener("change", updateSelectedFiles2);
    }

    const fileContainer3 = document.getElementById("fileCheckboxContainer3");
    if (fileContainer3) {
        fileContainer3.addEventListener("change", updateSelectedFiles3);
    }

    // Attach form submissions
    document.getElementById("mergeVideosForm")?.addEventListener("submit", mergeVideos);
    document.getElementById("mergeAudioVideoForm")?.addEventListener("submit", mergeAudioVideo);
    document.getElementById("analyseMediaForm")?.addEventListener("submit", analyseMedia);

    // Fetch file lists
    fetchFileList("fileCheckboxContainer", "file-checkbox");
    fetchFileList("fileCheckboxContainer2", "file-checkbox2");
    fetchFileList("fileCheckboxContainer3", "file-checkbox3");
});