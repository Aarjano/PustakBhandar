using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using FinalProject.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    /// <summary>
    /// Authentication endpoints for user registration, user login, and admin login
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Authentication")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="model">User registration details</param>
        /// <returns>JWT token and user details</returns>
        /// <response code="200">Returns the token and user details</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost]
        [Route("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Register([FromBody] RegisterViewModel model)
        {
            _logger.LogInformation("Register endpoint called with email: {Email}", model.Email);
            
            // Basic validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Register validation failed: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Generate token
            var token = GenerateJwtToken("user123", model.Email);

            // Return success result
            _logger.LogInformation("Registration successful for email: {Email}", model.Email);
            return Ok(new
            {
                token,
                user = new
                {
                    id = "user123",
                    email = model.Email,
                    firstName = model.FirstName,
                    lastName = model.LastName,
                    membershipId = model.MembershipId
                }
            });
        }

        /// <summary>
        /// Authenticate a user
        /// </summary>
        /// <param name="model">User login credentials</param>
        /// <returns>JWT token and user details</returns>
        /// <response code="200">Returns the token and user details</response>
        /// <response code="400">If the request is invalid</response>
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            _logger.LogInformation("Login endpoint called with email: {Email}", model.Email);
            
            // Basic validation
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Login validation failed: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            // Generate token
            var token = GenerateJwtToken("user123", model.Email);

            // Return success result
            _logger.LogInformation("Login successful for email: {Email}", model.Email);
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
        /// Authenticate as admin
        /// </summary>
        /// <param name="model">Admin login credentials (use admin/admin123)</param>
        /// <returns>JWT token and admin details</returns>
        /// <response code="200">Returns the token and admin details</response>
        /// <response code="401">If credentials are invalid</response>
        [HttpPost]
        [Route("admin-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult AdminLogin([FromBody] AdminLoginViewModel model)
        {
            _logger.LogInformation("Admin login endpoint called with username: {Username}", model.Username);
            
            // Check hardcoded credentials
            if (model.Username == "admin" && model.Password == "admin123")
            {
                // Generate token
                var token = GenerateJwtToken("admin", "admin@example.com", true);

                // Return success result
                _logger.LogInformation("Admin login successful for username: {Username}", model.Username);
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

            _logger.LogWarning("Admin login failed for username: {Username}", model.Username);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        /// <summary>
        /// Simple API healthcheck endpoint that doesn't require authentication
        /// </summary>
        /// <returns>Current server timestamp</returns>
        [HttpGet]
        [Route("healthcheck")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            _logger.LogInformation("Healthcheck endpoint called");
            return Ok(new
            {
                status = "ok",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
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