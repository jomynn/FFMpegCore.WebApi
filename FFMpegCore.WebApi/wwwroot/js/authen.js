// Define the base API URL dynamically
import { apiBaseUrl } from './shared.js';

// Handle login submission
async function handleLogin(event) {
    event.preventDefault();

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value.trim();
    const errorMessage = document.getElementById("errorMessage");

    // Clear previous error messages
    errorMessage.style.display = "none";
    errorMessage.textContent = "";

    try {
        const response = await fetch(`${apiBaseUrl}/auth/login`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ username, password })
        });

        if (!response.ok) {
            const errorText = await response.text();
            let errorData;
            try {
                errorData = JSON.parse(errorText);
                throw new Error(errorData.message || "Unexpected error occurred.");
            } catch {
                throw new Error("Unexpected error: " + errorText);
            }
        }

        const result = await response.json();
        localStorage.setItem("authToken", result.token);
        localStorage.setItem("username", username);

        window.location.href = "/dashboard.html";
    } catch (error) {
        console.error("Login error:", error);
        errorMessage.style.display = "block";
        errorMessage.textContent = error.message;
    }
}

document.addEventListener("DOMContentLoaded", () => {
    document.getElementById("loginForm").addEventListener("submit", handleLogin);
});
