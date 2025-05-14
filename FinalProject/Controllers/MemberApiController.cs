using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FinalProject.Data;
using FinalProject.Models;
using FinalProject.ViewModels;
using Microsoft.AspNetCore.Cors;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    [Produces("application/json")]
    public class MemberApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MemberApiController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new member
        /// </summary>
        /// <param name="model">Registration details</param>
        /// <returns>User details and authentication token</returns>
        /// <response code="200">Returns the user details and token</response>
        /// <response code="400">If the model is invalid or user already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if a member with the same email or membership ID already exists
                if (await _context.Members.AnyAsync(m => m.Email == model.Email || m.MembershipId == model.MembershipId))
                {
                    return BadRequest(new { message = "A member with this email or membership ID already exists." });
                }

                // Hash the password before saving
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Create a new Member object from the ViewModel
                var member = new Member
                {
                    MembershipId = model.MembershipId,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = hashedPassword,
                    RegistrationDate = DateTime.Now,
                    DateAdded = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    OrderCount = 0,
                    StackableDiscount = 0.00M
                };

                // Add the new member to the context
                _context.Add(member);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(member);

                // Return user info and token
                return Ok(new 
                { 
                    token, 
                    user = new
                    {
                        id = member.MemberId,
                        email = member.Email,
                        firstName = member.FirstName,
                        lastName = member.LastName,
                        membershipId = member.MembershipId
                    }
                });
            }

            return BadRequest(ModelState);
        }

        /// <summary>
        /// Authenticate a member
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>User details and authentication token</returns>
        /// <response code="200">Returns the user details and token</response>
        /// <response code="401">If the credentials are invalid</response>
        /// <response code="400">If the model is invalid</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<object>> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Find the member by email
                var member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Email == model.Email);

                // Verify the member exists and the password is correct
                if (member == null || !BCrypt.Net.BCrypt.Verify(model.Password, member.PasswordHash))
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                // Update LastLogin timestamp
                member.LastLogin = DateTime.Now;
                _context.Update(member);
                await _context.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(member);

                // Return user info and token
                return Ok(new 
                { 
                    token, 
                    user = new
                    {
                        id = member.MemberId,
                        email = member.Email,
                        firstName = member.FirstName,
                        lastName = member.LastName,
                        membershipId = member.MembershipId
                    }
                });
            }

            return BadRequest(ModelState);
        }

        // Helper method to generate JWT token
        private string GenerateJwtToken(Member member)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),
                new Claim(ClaimTypes.Name, $"{member.FirstName} {member.LastName}"),
                new Claim(ClaimTypes.Email, member.Email),
                new Claim("MembershipId", member.MembershipId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "defaultsecretkey1234567890abcdefghijklmnopqrstuvwxyz"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
} 