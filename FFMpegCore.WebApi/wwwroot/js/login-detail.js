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

loadSidebar();
