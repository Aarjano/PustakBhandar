using Microsoft.AspNetCore.Mvc;
using FinalProject.Data;
using FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthorsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AuthorsApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Author>>> GetAuthors()
        {
            return await _context.Authors
                .Include(a => a.Books)
                .ToListAsync();
        }

        // GET: api/AuthorsApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Author>> GetAuthor(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound();
            }

            return author;
        }

        // GET: api/AuthorsApi/search/name
        [HttpGet("search/{term}")]
        public async Task<ActionResult<IEnumerable<Author>>> SearchAuthors(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return await _context.Authors.Take(10).ToListAsync();
            }

            return await _context.Authors
                .Where(a => a.FirstName.Contains(term) || 
                       a.LastName.Contains(term) ||
                       (a.Biography != null && a.Biography.Contains(term)))
                .ToListAsync();
        }
    }
} 