const apiBaseUrl = "http://localhost:5148/api/FFMpeg";

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("mergeAudioVideoForm");

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        // Get the audio and video file inputs
        const audioFileInput = document.getElementById("audioFile");
        const videoFileInput = document.getElementById("videoFileForAudio");

        // Validate if files are selected
        if (!audioFileInput.files.length || !videoFileInput.files.length) {
            alert("Please select both audio and video files.");
            return;
        }

        const audioFile = audioFileInput.files[0];
        const videoFile = videoFileInput.files[0];

        // Convert files to Base64 strings
        const audioBase64 = await toBase64(audioFile);
        const videoBase64 = await toBase64(videoFile);

        // Prepare the payload
        const payload = {
            audioFileBase64: audioBase64,
            videoFileBase64: videoBase64,
            outputFileName: "merged_output.mp4" // Default output file name
        };

        try {
            // Send the request to the API
            const response = await fetch(`${apiBaseUrl}/merge-audio-video`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            // Check response status
            if (!response.ok) {
                throw new Error(`API error: ${response.statusText}`);
            }

            // Parse and display the result
            const result = await response.json();
            alert(`Audio and Video merged successfully. Output: ${result.outputPath}`);
        } catch (error) {
            console.error("Error merging audio and video:", error);
            alert("Failed to merge audio and video. Please try again.");
        }
    });
});

// Utility function to convert file to Base64
function toBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result.split(",")[1]); // Extract Base64 data
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
    //const outputFileName = document.getElementById("audioVideoOutputName").value || "audio_video.mp4";

    formData.append("audioFile", audioFile);
    formData.append("videoFile", videoFile);
    //formData.append("outputFileName", outputFileName);

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
