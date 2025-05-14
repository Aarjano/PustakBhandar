using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims; // Required for Claims
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication; // Required for Authentication
using Microsoft.AspNetCore.Authentication.Cookies; // Required for Cookie Authentication
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinalProject.Data;
using FinalProject.Models;
using FinalProject.ViewModels; // Assuming ViewModels are here
using BCrypt.Net; // Required for password hashing
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace FinalProject.Controllers
{
    // Apply Authorize attribute to the entire controller to require authentication by default
    // Actions that should be publicly accessible will use [AllowAnonymous]
    [Authorize(AuthenticationSchemes = AdminAuthScheme, Roles = "Admin")] // Require Admin role for most actions in this controller
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Define a separate authentication scheme for Admins
        // This should also be configured in your Startup.cs or Program.cs
        public const string AdminAuthScheme = "AdminCookieAuth";

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // --- Admin Authentication Actions ---

        // GET: Admin/Register
        /// <summary>
        /// Displays the admin registration form.
        /// </summary>
        /// <returns>The AdminRegister view.</returns>
        [HttpGet]
        [AllowAnonymous] // Allow anonymous access to the registration page
        public IActionResult Register()
        {
            // Note: In a real application, you would likely want to restrict access
            // to this registration page, perhaps only allowing a super-admin to create
            // new admin accounts, or having a one-time setup process.
            // Pass an empty AdminRegisterViewModel to the view, initializing required properties
            return View(new AdminRegisterViewModel
            {
                Username = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty
            });
        }

        // POST: Admin/Register
        /// <summary>
        /// Handles the form submission for admin registration.
        /// </summary>
        /// <param name="model">The AdminRegisterViewModel bound from the form data.</param>
        /// <returns>Redirects to Admin/Login on success, otherwise returns the Register view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // Allow anonymous access to the registration post
        public async Task<IActionResult> Register(AdminRegisterViewModel model) // Using AdminRegisterViewModel
        {
            // Check if the model state is valid based on data annotations.
            if (ModelState.IsValid)
            {
                // Check if an admin with the same username or email already exists
                if (await _context.Admins.AnyAsync(a => a.Username == model.Username || a.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "An admin with this username or email already exists.");
                    return View(model);
                }

                // Hash the password before saving
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Create a new Admin object from the ViewModel
                var admin = new Admin
                {
                    Username = model.Username,
                    Email = model.Email, // Email is now nullable in the model, but required in ViewModel for registration
                    FirstName = model.FirstName, // FirstName is now nullable in the model, but required in ViewModel for registration
                    LastName = model.LastName, // LastName is now nullable in the model, but required in ViewModel for registration
                    PasswordHash = hashedPassword, // Store the hashed password
                    DateAdded = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    // LastLogin will be set on successful login
                };

                // Add the new admin to the context.
                _context.Add(admin);
                // Save changes to the database asynchronously.
                await _context.SaveChangesAsync();

                // Redirect to the Admin Login page after successful registration.
                return RedirectToAction(nameof(Login));
            }

            // If model state is not valid, return the Register view with the current model
            // to display validation errors.
            return View(model);
        }

        // GET: Admin/Login
        /// <summary>
        /// Displays the admin login form.
        /// </summary>
        /// <param name="returnUrl">The URL to return to after successful login.</param>
        /// <returns>The AdminLogin view.</returns>
        [HttpGet]
        [AllowAnonymous] // Allow anonymous access to the login page
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            // Pass an empty AdminLoginViewModel to the view, initializing required properties
             return View(new AdminLoginViewModel
            {
                Username = string.Empty,
                Password = string.Empty,
                RememberMe = false // Initialize boolean
            });
        }

        // POST: Admin/Login
        /// <summary>
        /// Handles the form submission for admin login.
        /// </summary>
        /// <param name="model">The AdminLoginViewModel bound from the form data.</param>
        /// <param name="returnUrl">The URL to return to after successful login.</param>
        /// <returns>Redirects to the returnUrl or Admin/Index on success, otherwise returns the Login view with errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous] // Allow anonymous access to the login post
        public async Task<IActionResult> Login(AdminLoginViewModel model, string? returnUrl = null) // Using AdminLoginViewModel
        {
            ViewData["ReturnUrl"] = returnUrl;

            // Check if the model state is valid.
            if (ModelState.IsValid)
            {
                // Find the admin by username
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Username == model.Username);

                // Verify the admin exists and the password is correct
                if (admin != null && BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
                {
                    // Password is correct, sign in the admin

                    // Create claims for the admin
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()), // Store AdminId
                        new Claim(ClaimTypes.Name, admin.Username), // Store username
                        new Claim(ClaimTypes.GivenName, admin.FullName), // Store full name (using the derived property)
                        new Claim(ClaimTypes.Email, admin.Email ?? ""), // Store email (handle nullable)
                        new Claim(ClaimTypes.Role, "Admin") // Assign an 'Admin' role claim
                        // Add other claims as needed
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, AdminAuthScheme); // Use the Admin specific scheme

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe, // RememberMe from LoginViewModel
                        // Set expiration if persistent (adjust time as needed)
                        ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddMinutes(30) : (DateTimeOffset?)null
                        // Consider adding SlidingExpiration = true
                    };

                    // Sign in the admin using the admin cookie authentication scheme
                    await HttpContext.SignInAsync(
                        AdminAuthScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Update LastLogin timestamp
                    admin.LastLogin = DateTime.Now;
                    _context.Update(admin);
                    await _context.SaveChangesAsync();

                    // Redirect to the return URL or a default admin page
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Admin"); // Redirect to Admin Index page by default
                    }
                }
                else
                {
                    // Invalid login attempt
                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                    return View(model);
                }
            }

            // If model state is not valid, return the Login view with the current model
            // to display validation errors.
            return View(model);
        }

        // POST: Admin/Logout
        /// <summary>
        /// Logs out the current admin.
        /// </summary>
        /// <returns>Redirects to the Admin Login page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        // This action requires authentication to prevent logging out someone who isn't logged in
        // Since the controller has [Authorize], this action is protected by default.
        public async Task<IActionResult> Logout()
        {
            // Sign out the admin using the admin specific scheme
            await HttpContext.SignOutAsync(AdminAuthScheme);

            // Redirect to the Admin Login page
            return RedirectToAction(nameof(Login));
        }


        // --- Standard CRUD Actions (Now protected by [Authorize] on the controller) ---

        // GET: Admin
        // This action is now protected by the [Authorize] attribute on the controller level.
        // Only authenticated admins with the "Admin" role can access this.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Admins.ToListAsync());
        }


        // GET: Admin/Details/5
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.AdminId == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // GET: Admin/Create
        /// <summary>
        /// Displays the admin creation form using the AdminRegisterViewModel.
        /// </summary>
        /// <returns>The Create view with a new AdminRegisterViewModel.</returns>
        [HttpGet]
        // This action is now protected by the [Authorize] attribute on the controller level.
        public IActionResult Create()
        {
            // Pass an empty AdminRegisterViewModel to the view, initializing required properties
            return View(new AdminRegisterViewModel
            {
                Username = string.Empty,
                Password = string.Empty,
                ConfirmPassword = string.Empty,
                Email = string.Empty,
                FirstName = string.Empty,
                LastName = string.Empty
            });
        }

        // POST: Admin/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> Create(AdminRegisterViewModel model) // Accept AdminRegisterViewModel
        {
            if (ModelState.IsValid)
            {
                 // Check if an admin with the same username or email already exists
                if (await _context.Admins.AnyAsync(a => a.Username == model.Username || a.Email == model.Email))
                {
                    ModelState.AddModelError(string.Empty, "An admin with this username or email already exists.");
                    return View(model); // Return the model back to the view
                }

                // Hash the password from the ViewModel
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                // Create the Admin entity from the ViewModel data
                var admin = new Admin
                {
                    Username = model.Username,
                    PasswordHash = hashedPassword, // Use the hashed password
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateAdded = DateTime.Now, // Set DateAdded
                    DateUpdated = DateTime.Now, // Set DateUpdated
                    // LastLogin is set on successful login
                };

                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // If model state is not valid, return the model back to the view
            return View(model);
        }

        // GET: Admin/Edit/5
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            // Note: You should not pass the PasswordHash to the view for editing.
            // Use a ViewModel that doesn't include PasswordHash, and handle password changes
            // separately or require the old password to set a new one.
            return View(admin);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> Edit(int id, [Bind("AdminId,Username,Email,FirstName,LastName,DateAdded")] Admin admin) // Removed PasswordHash, LastLogin, DateUpdated from Bind
        {
            if (id != admin.AdminId)
            {
                return NotFound();
            }

            // Retrieve the existing admin from the database
            var adminToUpdate = await _context.Admins.FindAsync(id);
            if (adminToUpdate == null)
            {
                return NotFound();
            }

            // Update properties from the bound model, excluding PasswordHash, LastLogin, DateAdded
            // Use TryUpdateModelAsync or manually map properties to prevent overposting and
            // avoid overwriting the existing PasswordHash, LastLogin, and DateAdded.
            if (await TryUpdateModelAsync<Admin>(
                adminToUpdate,
                "", // Prefix for form values
                a => a.Username, a => a.Email, a => a.FirstName, a => a.LastName)) // Specify allowed properties
            {
                 // Check for duplicate username or email, excluding the current admin
                if (await _context.Admins.AnyAsync(a => (a.Username == adminToUpdate.Username || a.Email == adminToUpdate.Email) && a.AdminId != id))
                {
                    ModelState.AddModelError(string.Empty, "An admin with this username or email already exists.");
                     // Manually set DateUpdated back to the current time as TryUpdateModelAsync might not handle it
                    adminToUpdate.DateUpdated = DateTime.Now;
                    return View(adminToUpdate); // Return the view with the retrieved admin object
                }

                try
                {
                    adminToUpdate.DateUpdated = DateTime.Now; // Update DateUpdated
                    _context.Update(adminToUpdate); // Mark as modified
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Index));
            }

            // If TryUpdateModelAsync fails or ModelState is invalid
            adminToUpdate.DateUpdated = DateTime.Now;
            return View(adminToUpdate); // Return the view with the retrieved admin object
        }

        // GET: Admin/Delete/5
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var admin = await _context.Admins
                .FirstOrDefaultAsync(m => m.AdminId == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // This action is now protected by the [Authorize] attribute on the controller level.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Consider adding an AccessDenied action and view later for admins
        [HttpGet]
        [AllowAnonymous] // Allow anonymous access to the Access Denied page
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Admin/BookManagement
        public async Task<IActionResult> BookManagement(string searchString, int? pageNumber, int? genreId, int? authorId)
        {
            // Define the number of items per page
            int pageSize = 10;

            // Start with the base query including related entities
            var books = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.Publisher)
                .AsQueryable();

            // Apply search filter if a search string is provided
            if (!string.IsNullOrEmpty(searchString))
            {
                books = books.Where(b => b.Title.Contains(searchString) ||
                                       (b.Author != null && b.Author.FirstName != null && b.Author.FirstName.Contains(searchString)) ||
                                       (b.Author != null && b.Author.LastName != null && b.Author.LastName.Contains(searchString)) ||
                                       (b.Isbn != null && b.Isbn.Contains(searchString)));
            }

            // Apply Genre filter if a genreId is provided
            if (genreId.HasValue && genreId.Value > 0)
            {
                books = books.Where(b => b.GenreId == genreId.Value);
            }

            // Apply Author filter if an authorId is provided
            if (authorId.HasValue && authorId.Value > 0)
            {
                books = books.Where(b => b.AuthorId == authorId.Value);
            }

            // Add ordering
            books = books.OrderByDescending(b => b.DateAdded);

            // Get the total number of items after filtering
            var totalItems = await books.CountAsync();

            // Calculate the total number of pages
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Get the current page number
            var currentPage = pageNumber ?? 1;

            // Skip and take for pagination
            var paginatedBooks = await books
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Pass pagination, search, and filter information to the view
            ViewBag.CurrentPage = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchString = searchString;
            ViewBag.SelectedGenreId = genreId;
            ViewBag.SelectedAuthorId = authorId;

            // Populate dropdown lists for Genre and Author
            ViewBag.Genres = new SelectList(_context.Genres.OrderBy(g => g.Name), "GenreId", "Name", genreId);
            ViewBag.Authors = new SelectList(_context.Authors.OrderBy(a => a.FullName), "AuthorId", "FullName");

            return View(paginatedBooks);
        }

        // POST: Admin/UpdateStock
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int bookId, int newStock)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                book.AvailabilityStock = newStock;
                book.DateUpdated = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred while updating stock" });
            }
        }

        // POST: Admin/ToggleLibraryAvailability
        [HttpPost]
        public async Task<IActionResult> ToggleLibraryAvailability(int bookId, bool isAvailable)
        {
            try
            {
                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                book.AvailabilityLibrary = isAvailable;
                book.DateUpdated = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred while updating library availability" });
            }
        }

        // POST: Admin/Delete
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null)
                {
                    return Json(new { success = false, message = "Book not found" });
                }

                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false, message = "An error occurred while deleting the book" });
            }
        }

        // GET: Admin/CreateBook
        [HttpGet]
        public IActionResult CreateBook()
        {
            // Populate dropdown lists
            ViewBag.Authors = new SelectList(_context.Authors.OrderBy(a => a.FullName), "AuthorId", "FullName");
            ViewBag.Genres = new SelectList(_context.Genres.OrderBy(g => g.Name), "GenreId", "Name");
            ViewBag.Publishers = new SelectList(_context.Publishers.OrderBy(p => p.Name), "PublisherId", "Name");
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook([FromForm] Book book, IFormFile coverImage)
        {
            if (ModelState.IsValid)
            {
                if (coverImage != null)
                {
                    // Create uploads directory if it doesn't exist
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + coverImage.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(fileStream);
                    }

                    // Set the cover image URL
                    book.CoverImageUrl = "/uploads/" + uniqueFileName;
                }

                book.DateAdded = DateTime.Now;
                book.DateUpdated = DateTime.Now;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(BookManagement));
            }
            
            // If we got this far, something failed, redisplay form
            ViewBag.Publishers = new SelectList(_context.Publishers.OrderBy(p => p.Name), "PublisherId", "Name", book.PublisherId);
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int id, [FromForm] Book book, IFormFile coverImage)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (coverImage != null)
                    {
                        // Create uploads directory if it doesn't exist
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // Generate unique filename
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + coverImage.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        // Save the file
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(fileStream);
                        }

                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(book.CoverImageUrl))
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, book.CoverImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Set the new cover image URL
                        book.CoverImageUrl = "/uploads/" + uniqueFileName;
                    }

                    book.DateUpdated = DateTime.Now;
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(BookManagement));
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            
            return View(book);
        }
    }
}
