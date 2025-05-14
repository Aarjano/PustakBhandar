using Microsoft.AspNetCore.Mvc;
using FinalProject.Data;
using FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public const string AdminAuthScheme = "AdminCookieAuth";

        public BooksApiController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: api/BooksApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre)
                .ToListAsync();
        }

        // GET: api/BooksApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            return book;
        }

        // GET: api/BooksApi/genre/5
        [HttpGet("genre/{genreId}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByGenre(int genreId)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Where(b => b.GenreId == genreId)
                .ToListAsync();
        }

        // GET: api/BooksApi/author/5
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooksByAuthor(int authorId)
        {
            return await _context.Books
                .Include(b => b.Genre)
                .Include(b => b.Publisher)
                .Where(b => b.AuthorId == authorId)
                .ToListAsync();
        }

        // GET: api/BooksApi/search/title
        [HttpGet("search/{term}")]
        public async Task<ActionResult<IEnumerable<Book>>> SearchBooks(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return await _context.Books.Take(10).ToListAsync();
            }

            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Where(b => b.Title.Contains(term) || 
                       (b.Author != null && (b.Author.FirstName.Contains(term) || b.Author.LastName.Contains(term))) ||
                       (b.Isbn != null && b.Isbn.Contains(term)))
                .ToListAsync();
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = AdminAuthScheme, Roles = "Admin")]
        public async Task<ActionResult<Book>> CreateBook([FromForm] Book book, IFormFile coverImage)
        {
            if (coverImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + coverImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(fileStream);
                }
                book.CoverImageUrl = "/uploads/" + uniqueFileName;
            }

            book.DateAdded = DateTime.Now;
            book.DateUpdated = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, book);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = AdminAuthScheme, Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, [FromForm] Book book, IFormFile coverImage)
        {
            if (id != book.BookId)
            {
                return BadRequest();
            }

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
            {
                return NotFound();
            }

            // Update fields
            existingBook.Title = book.Title;
            existingBook.Isbn = book.Isbn;
            existingBook.Description = book.Description;
            existingBook.Author = book.Author;
            existingBook.Genre = book.Genre;
            existingBook.Publisher = book.Publisher;
            existingBook.Language = book.Language;
            existingBook.Format = book.Format;
            existingBook.ListPrice = book.ListPrice;
            existingBook.PublicationDate = book.PublicationDate;
            existingBook.AvailabilityStock = book.AvailabilityStock;
            existingBook.AvailabilityLibrary = book.AvailabilityLibrary;
            existingBook.DateUpdated = DateTime.Now;

            if (coverImage != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + coverImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await coverImage.CopyToAsync(fileStream);
                }
                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingBook.CoverImageUrl))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingBook.CoverImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                existingBook.CoverImageUrl = "/uploads/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = AdminAuthScheme, Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            // Delete cover image if it exists
            if (!string.IsNullOrEmpty(book.CoverImageUrl))
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.BookId == id);
        }
    }
} 