using Microsoft.EntityFrameworkCore;
using FinalProject.Data; // Make sure this namespace matches your DbContext location
using Microsoft.Extensions.DependencyInjection; // Added for IServiceCollection and related extensions
using Microsoft.Extensions.Hosting; // Added for IHostEnvironment
using Microsoft.Extensions.Configuration; // Added for IConfiguration
using Microsoft.Extensions.Logging; // Added for ILogger
using Microsoft.AspNetCore.Authentication.Cookies; // Needed for CookieAuthenticationDefaults
using Microsoft.OpenApi.Models; // Added for Swagger support
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinalProject.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // This registers MVC controllers and views

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder
                .WithOrigins("http://localhost:3000") // Use your frontend URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

// --- Add Swagger services ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "E-Library API",
        Version = "v1",
        Description = "An API for managing the E-Library application",
        Contact = new OpenApiContact
        {
            Name = "PustakBhandar"
        }
    });

    // Add security definition for JWT Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add security definition for cookie authentication
    c.AddSecurityDefinition("cookieAuth", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Cookie,
        Name = "FinalProject.Admin",
        Description = "Cookie-based authentication for admin endpoints"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        },
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "cookieAuth"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable XML comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
// --- End Add Swagger services ---

// --- Configure Authentication Schemes ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "FinalProject.Admin";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.LoginPath = "/api/Account/Login";
    options.LogoutPath = "/api/Account/Logout";
});

// Add Authorization services
builder.Services.AddAuthorization();

// --- Register ApplicationDbContext with Dependency Injection ---
// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Check if the connection string was found (good practice)
if (string.IsNullOrEmpty(connectionString))
{
    // Log an error or throw an exception if the connection string is missing
    // For now, we'll just print to console (you should use a logger in a real app)
    Console.WriteLine("DefaultConnection connection string is not configured in appsettings.json!");
    // Optionally, you could throw an exception here to prevent the app from starting
    // throw new InvalidOperationException("Database connection string is not configured.");
}
else
{
    // Add the ApplicationDbContext to the services
    // Configure it to use PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(connectionString));
}
// --- End Register ApplicationDbContext ---

var app = builder.Build();

// Configure the HTTP request pipeline.
// In Development, you might want to use the DeveloperExceptionPage
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "E-Library API v1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
        c.EnableValidator();
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Configure endpoint routing
app.UseRouting();

// Add CORS middleware before routing
app.UseCors("AllowFrontend");

// Add a custom middleware to directly handle CORS preflight requests
app.Use(async (context, next) =>
{
    // If this is a preflight OPTIONS request
    if (context.Request.Method == "OPTIONS")
    {
        // Handle it manually with the appropriate headers
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, X-Requested-With");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.Headers.Add("Access-Control-Max-Age", "86400"); // 24 hours
        
        // Return 200 OK status
        context.Response.StatusCode = 200;
        return;
    }
    
    await next();
});

// Comment out HttpsRedirection for development
// app.UseHttpsRedirection();
app.UseStaticFiles();

// Authentication middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure endpoint routing
app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
