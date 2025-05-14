using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using FinalProject.ViewModels;

namespace FinalProject.Controllers
{
    [Route("api/register")]
    [ApiController]
    [EnableCors]
    public class MemberRegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MemberRegistrationController> _logger;

        public MemberRegistrationController(IConfiguration configuration, ILogger<MemberRegistrationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpOptions]
        public IActionResult PreflightRoute()
        {
            return NoContent();
        }

        [HttpPost]
        public IActionResult RegisterUser([FromBody] RegisterViewModel model)
        {
            _logger.LogInformation("Direct register endpoint called with email: {Email}", model.Email);
            
            // Basic validation
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Generate token
                var token = GenerateJwtToken("user123", model.Email);

                // Return success result
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "Internal server error during registration" });
            }
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