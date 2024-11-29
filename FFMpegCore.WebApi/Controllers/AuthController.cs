using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly JwtSettings _jwtSettings;

    public AuthController(JwtSettings jwtSettings, ILogger<AuthController> logger)
    {
        _jwtSettings = jwtSettings;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("AuthController.Login() called");

        try
        {
            if (request.Username != "admin" || request.Password != "password")
            {
                return Unauthorized("Invalid credentials.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

            var now = DateTime.UtcNow;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
        new Claim(ClaimTypes.Name, request.Username)
    }),
                NotBefore = now,
                Expires = now.AddMinutes(_jwtSettings.TokenLifetimeMinutes).AddSeconds(1), // Add 1 second buffer
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = tokenHandler.WriteToken(token);

            // Return the token in a JSON response
            return Ok(new
            {
                Token = tokenString
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred in AuthController.Login()");
            return StatusCode(500, "Internal Server Error");
        }
    }

    // New protected endpoint
    [Authorize] // Require JWT for access
    [HttpGet("protected-endpoint")]
    public IActionResult GetProtectedData()
    {
        var userName = User.Identity?.Name; // Retrieve the username from the JWT
        var data = new
        {
            Message = $"Hello {userName}, this is protected data.",
            Timestamp = DateTime.UtcNow
        };

        return Ok(data);
    }
}

