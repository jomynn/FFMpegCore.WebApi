import { apiBaseUrl } from './shared.js';

function decodeToken(token) {
    const payload = token.split('.')[1];
    const decoded = JSON.parse(atob(payload));
    return decoded;
}

async function initializeSidebar() {
    const token = localStorage.getItem("authToken");

    if (!token) {
        displayError("Authentication token is missing. Please log in again.");
        return redirectToLogin();
    }

    const decodedToken = decodeToken(token);
    const expiry = decodedToken.exp * 1000;

    if (Date.now() >= expiry) {
        displayError("Session expired. Please log in again.");
        localStorage.removeItem("authToken");
        return redirectToLogin();
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
        displayError("Failed to fetch protected data. Please log in again.");
        redirectToLogin();
    }
}

async function loadSidebar() {
    try {
        const sidebarContainer = document.getElementById("sidebar-container");

        // Fetch the sidebar HTML
        const response = await fetch("/partials/sidebar.html");
        if (!response.ok) {
            throw new Error("Failed to load sidebar.");
        }

        // Inject the sidebar content into the container
        const sidebarHtml = await response.text();
        sidebarContainer.innerHTML = sidebarHtml;
    } catch (error) {
        console.error("Error loading sidebar:", error);
        document.getElementById("sidebar-container").innerHTML = `<p style="color: red;">Error loading sidebar</p>`;
    }
}

function displayError(message) {
    const errorMessage = document.getElementById("errorMessage");
    errorMessage.style.display = "block";
    errorMessage.textContent = message;
}

function redirectToLogin() {
    window.location.href = "/login.html";
}

initializeSidebar();
// Call the function to load the sidebar
loadSidebar();