// Load the sidebar dynamically
fetch('/sidebar.html')
    .then(response => response.text())
    .then(html => {
        document.getElementById('sidebar-container').innerHTML = html;

        // Highlight the active link based on the current URL
        const links = document.querySelectorAll('.sidebar a');
        const currentPath = window.location.pathname;

        links.forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });
    })
    .catch(error => console.error('Error loading sidebar:', error));