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
        const response = await fetch("/partials/sidebar.html");
        if (!response.ok) {
            throw new Error("Failed to load sidebar.");
        }

        const sidebarHtml = await response.text();
        document.getElementById("sidebar-container").innerHTML = sidebarHtml;

        // Add logout event listener
        document.getElementById("logoutLink").addEventListener("click", () => {
            localStorage.removeItem("authToken");
            window.location.href = "/login.html";
        });
    } catch (error) {
        console.error("Error loading sidebar:", error);
        document.getElementById("sidebar-container").innerHTML = `<p style="color: red;">Error loading sidebar</p>`;
    }
}

async function loadLoginDetails() {
    try {
        const token = localStorage.getItem("authToken");
        if (!token) {
            throw new Error("Authentication token is missing. Please log in again.");
        }

        const decodedToken = JSON.parse(atob(token.split('.')[1]));
        const username = decodedToken.name || "User";
        const role = decodedToken.role || "Unknown";

        const response = await fetch("/partials/login-details.html");
        if (!response.ok) {
            throw new Error("Failed to load login details.");
        }

        const loginDetailsHtml = await response.text();
        document.getElementById("login-detail-container").innerHTML = loginDetailsHtml;

        // Populate user details
        document.getElementById("username").textContent = username;
        document.getElementById("role").textContent = role;
    } catch (error) {
        console.error("Error loading login details:", error);
        document.getElementById("login-detail-container").innerHTML = `<p style="color: red;">Error loading login details</p>`;
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

loadLoginDetails();