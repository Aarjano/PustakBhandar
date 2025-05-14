using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinalProject.Controllers
{
    /// <summary>
    /// Authentication endpoints for user registration and login
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    [EnableCors]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Model classes defined within the controller
        /// <summary>
        /// User registration request model
        /// </summary>
        public class RegisterRequest
        {
            /// <summary>
            /// User's first name
            /// </summary>
            public string? FirstName { get; set; }
            
            /// <summary>
            /// User's last name
            /// </summary>
            public string? LastName { get; set; }
            
            /// <summary>
            /// User's email address (required)
            /// </summary>
            public string? Email { get; set; }
            
            /// <summary>
            /// User's password (required)
            /// </summary>
            public string? Password { get; set; }
            
            /// <summary>
            /// User's membership ID (optional)
            /// </summary>
            public string? MembershipId { get; set; }
        }

        /// <summary>
        /// User login request model
        /// </summary>
        public class LoginRequest
        {
            /// <summary>
            /// User's email address (required)
            /// </summary>
            public string? Email { get; set; }
            
            /// <summary>
            /// User's password (required)
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Admin login request model
        /// </summary>
        public class AdminLoginRequest
        {
            /// <summary>
            /// Admin username (required)
            /// </summary>
            public string? Username { get; set; }
            
            /// <summary>
            /// Admin password (required)
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="model">User registration information</param>
        /// <returns>User information and authentication token</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Register([FromBody] RegisterRequest model)
        {
            try
            {
                _logger.LogInformation("Register endpoint called");

                // Validate input
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Generate JWT token
                var token = GenerateJwtToken("user123", model.Email ?? "user@example.com");

                // Return success with token and user info
                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = "user123",
                        email = model.Email,
                        firstName = model.FirstName ?? "Default",
                        lastName = model.LastName ?? "User",
                        membershipId = model.MembershipId ?? "MEMBER123"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in register endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Login an existing user
        /// </summary>
        /// <param name="model">User login credentials</param>
        /// <returns>User information and authentication token</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            try
            {
                _logger.LogInformation("Login endpoint called");

                // Validate input
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // For demo purposes, auto-authenticate
                var token = GenerateJwtToken("user123", model.Email ?? "user@example.com");

                // Return success with token and user info
                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = "user123",
                        email = model.Email,
                        firstName = "Test",
                        lastName = "User",
                        membershipId = "TEST123"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in login endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Login as admin
        /// </summary>
        /// <param name="model">Admin credentials (username: admin, password: admin123)</param>
        /// <returns>Admin information and authentication token</returns>
        [HttpPost("admin-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest model)
        {
            try
            {
                _logger.LogInformation("Admin login endpoint called");

                // Validate input
                if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Username and password are required" });
                }

                // Check admin credentials
                if (model.Username == "admin" && model.Password == "admin123")
                {
                    var token = GenerateJwtToken("admin", "admin@example.com", true);

                    return Ok(new
                    {
                        token,
                        admin = new
                        {
                            id = 1,
                            username = "admin",
                            role = "Administrator"
                        }
                    });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin login endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if API is running
        /// </summary>
        [HttpGet("healthcheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Healthcheck()
        {
            return Ok(new
            {
                status = "ok",
                timestamp = DateTime.UtcNow
            });
        }

        private string GenerateJwtToken(string userId, string email, bool isAdmin = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Administrator"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "defaultsecretkey1234567890abcdefghijklmnopqrstuvwxyz"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"] ?? "PustakBhandar",
                _configuration["Jwt:Audience"] ?? "PustakBhandarUsers",
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 