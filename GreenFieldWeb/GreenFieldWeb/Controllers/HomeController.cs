// Import the application's data context so we can query the database
using GreenFieldWeb.Data;

// Import the application's models so we can use the Products and Producers classes
using GreenFieldWeb.Models;

// Import the core MVC namespace which gives us access to Controller and IActionResult
using Microsoft.AspNetCore.Mvc;

// Import Entity Framework Core so we can use async database methods like ToListAsync
using Microsoft.EntityFrameworkCore;

// Import System.Diagnostics so we can access Activity.Current for error tracking
using System.Diagnostics;

// All controllers in this project live inside the GreenFieldWeb.Controllers namespace
namespace GreenFieldWeb.Controllers
{
    // HomeController handles the public-facing pages of the website — home, privacy, contact and errors
    public class HomeController : Controller
    {
        // _logger is used to write log messages — useful for debugging issues in production
        private readonly ILogger<HomeController> _logger;

        // _context gives us access to the database through Entity Framework Core
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core automatically injects the logger and database context here when the controller is created
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            // Store the injected logger so it can be used throughout this controller
            _logger = logger;

            // Store the injected database context so we can query the database in our action methods
            _context = context;
        }

        // GET: / or /Home/Index — loads the main home page
        public async Task<IActionResult> Index()
        {
            // Count all products that are marked as available and pass the total to the view
            // This is used to display the accurate product count in the hero stats section
            ViewBag.TotalProductCount = await _context.Products.CountAsync(p => p.IsAvailable);

            // Fetch the 4 most recently added available products with stock, including their producer details
            // These are displayed in the Featured Products section on the home page
            ViewBag.FeaturedProducts = await _context.Products
                .Where(p => p.IsAvailable && p.Stock > 0)   // Only show products that are available and in stock
                .Include(p => p.Producers)                   // Load the related producer so we can show their name
                .OrderByDescending(p => p.ProductsId)        // Show the newest products first
                .Take(4)                                     // Only take 4 to keep the home page clean
                .ToListAsync();

            // Fetch up to 4 producers to display in the producers preview section on the home page
            ViewBag.Producers = await _context.Producers
                .Take(4)
                .ToListAsync();

            // Return the Index view — no model needed since everything is passed through ViewBag
            return View();
        }

        // GET: /Home/Privacy — loads the privacy policy page
        public IActionResult Privacy()
        {
            // Simply returns the Privacy view with no data needed from the database
            return View();
        }

        // GET: /Home/ContactUs — loads the contact us page
        public IActionResult ContactUs()
        {
            // Simply returns the ContactUs view — the form on this page is handled client-side
            return View();
        }

        // GET: /Home/Error — displays an error page when something goes wrong
        // ResponseCache ensures this page is never cached so users always see fresh error information
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Pass an ErrorViewModel to the view containing the current request ID
            // Activity.Current?.Id gives the trace ID if available, otherwise falls back to the HttpContext trace identifier
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
