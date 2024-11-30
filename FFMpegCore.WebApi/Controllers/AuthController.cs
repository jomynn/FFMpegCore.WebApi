using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<FFMpegController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration, ILogger<FFMpegController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("protected-endpoint")]
        public IActionResult GetProtectedData()
        {
            try
            {
                return Ok(new { message = "This is protected data." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred: " + ex.Message });
            }
        }

        // POST: api/Auth/Login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                _logger.LogWarning($"Login Invalid username or password: ");
                return BadRequest(new { message = "Invalid username or password." });
            }

            try
            {
                // Validate user
                if (!ValidateUser(request.Username, request.Password))
                {
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                // Generate JWT token
                var token = GenerateJwtToken(request.Username);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                // Ensure exceptions return JSON
                return StatusCode(500, new { message = ex.Message });
            }
        }

        private static bool ValidateUser(string username, string password)
        {
            // Replace with your own user validation logic
            return username == "admin" && password == "password"; // Example only, don't use in production
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt").Get<JwtSettings>();

            if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Key))
            {
                throw new InvalidOperationException("JWT signing key is missing or invalid.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User") // Example role claim
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings.Issuer,
                audience: jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtSettings.TokenLifetimeMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Request Model for Login
        public class LoginRequest
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
        }

        // JwtSettings Configuration Class
        public class JwtSettings
        {
            public required string Key { get; set; }
            public required string Issuer { get; set; }
            public required string Audience { get; set; }
            public int TokenLifetimeMinutes { get; set; }
        }
    }
}
