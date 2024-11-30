// Define the base API URL dynamically
import { apiBaseUrl } from './shared.js';
function decodeToken(token) {
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    return decoded;
}

// Retrieve the token from localStorage
const token = localStorage.getItem("authToken");

if (token) {
    const decodedToken = decodeToken(token);
    const expiry = decodedToken.exp * 1000; // Convert to milliseconds

    if (Date.now() >= expiry) {
        alert("Session expired. Please log in again.");
        localStorage.removeItem("authToken");
        window.location.href = "/login.html"; // Redirect to login
    } else {
        console.log("Token is valid.");
    }
} else {
    alert("Authentication token is missing. Please log in again.");
    window.location.href = "/login.html"; // Redirect to login
}

try {
    const response = await fetch(`${apiBaseUrl}/auth/protected-endpoint`, {
        method: "GET",
        headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
        }
    });

    if (!response.ok) {
        throw new Error("Failed to fetch protected data.");
    }

    const data = await response.json();
    console.log("Protected data:", data);
} catch (error) {
    console.error("Error fetching protected data:", error);
    alert("Failed to fetch protected data. Please log in again.");
    window.location.href = "/login.html"; // Redirect to login
}
