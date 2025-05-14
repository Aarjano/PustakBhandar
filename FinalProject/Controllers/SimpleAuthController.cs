using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FinalProject.Controllers
{
    /// <summary>
    /// Simple authentication controller with no external dependencies
    /// </summary>
    [Route("api/simple-auth")]
    [ApiController]
    [EnableCors]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class SimpleAuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SimpleAuthController> _logger;

        public SimpleAuthController(IConfiguration configuration, ILogger<SimpleAuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // Model classes defined directly to reduce dependencies
        /// <summary>
        /// Registration request model
        /// </summary>
        public class RegisterRequestModel
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
            /// User's email address
            /// </summary>
            public string? Email { get; set; }
            
            /// <summary>
            /// User's password
            /// </summary>
            public string? Password { get; set; }
            
            /// <summary>
            /// User's membership ID
            /// </summary>
            public string? MembershipId { get; set; }
        }

        /// <summary>
        /// Login request model
        /// </summary>
        public class LoginRequestModel
        {
            /// <summary>
            /// User's email address
            /// </summary>
            public string? Email { get; set; }
            
            /// <summary>
            /// User's password
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Admin login request model
        /// </summary>
        public class AdminLoginRequestModel
        {
            /// <summary>
            /// Admin username
            /// </summary>
            public string? Username { get; set; }
            
            /// <summary>
            /// Admin password
            /// </summary>
            public string? Password { get; set; }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="model">Registration data</param>
        /// <returns>JWT token and user data</returns>
        /// <response code="200">Returns the user data and authentication token</response>
        /// <response code="400">If the registration data is invalid</response>
        /// <response code="500">If there was a server error</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Register([FromBody] RegisterRequestModel model)
        {
            try
            {
                _logger.LogInformation("SimpleAuth register endpoint called");

                // Basic validation
                if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                {
                    return BadRequest(new { message = "Email and password are required" });
                }

                // Generate a simple token
                var token = GenerateJwtToken("user123", model.Email ?? "user@example.com");

                // Return success result
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
                _logger.LogError(ex, "Error in SimpleAuth register");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Login a user
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user data</returns>
        /// <response code="200">Returns the user data and authentication token</response>
        /// <response code="400">If the login data is invalid</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login([FromBody] LoginRequestModel model)
        {
            _logger.LogInformation("SimpleAuth login endpoint called");

            // Always return success for demo
            var token = GenerateJwtToken("user123", model.Email ?? "user@example.com");

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

        /// <summary>
        /// Admin login
        /// </summary>
        /// <param name="model">Admin credentials (username: admin, password: admin123)</param>
        /// <returns>JWT token and admin data</returns>
        /// <response code="200">Returns the admin data and authentication token</response>
        /// <response code="401">If the admin credentials are invalid</response>
        [HttpPost("admin-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AdminLogin([FromBody] AdminLoginRequestModel model)
        {
            _logger.LogInformation("SimpleAuth admin login endpoint called");

            // Check fixed credentials (admin/admin123)
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

            return Unauthorized(new { message = "Invalid username or password" });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Status and current server timestamp</returns>
        /// <response code="200">Returns status information</response>
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

        /// <summary>
        /// Handle preflight OPTIONS request
        /// </summary>
        /// <param name="path">Route path</param>
        /// <returns>204 No Content with CORS headers</returns>
        [HttpOptions("{*path}")]
        public IActionResult HandleOptions()
        {
            return NoContent();
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