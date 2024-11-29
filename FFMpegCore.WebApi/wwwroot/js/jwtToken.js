// Retrieve the token from localStorage
const token = localStorage.getItem('jwtToken');

if (!token) {
    alert('You are not logged in!');
    window.location.href = '/index.html'; // Redirect to login if no token
} else {
    // Display a message with the token or fetch protected data
    // document.getElementById('welcomeMessage').textContent = 'Welcome to the Dashboard! Your token: ' + token;
    document.getElementById('welcomeMessage').textContent = 'Welcome to the Dashboard! Your ^_*';

    // Example: Fetch protected data using the token
    fetch('/api/auth/protected-endpoint', {
        method: 'GET',
        headers: {
            'Authorization': `Bearer ${token}`
        }
    })
        .then(response => {
            if (response.ok) {
                return response.json();
            } else {
                throw new Error('Failed to fetch protected data.');
            }
        })
        .then(data => {
            console.log('Protected data:', data);
        })
        .catch(error => {
            console.error(error);
            alert('Failed to fetch protected data. Please log in again.');
            window.location.href = '/index.html'; // Redirect to login on error
        });
}