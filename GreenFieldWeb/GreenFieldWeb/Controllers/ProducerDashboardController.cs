// Provides [Authorize] and [Authorize(Roles = "...")] for access control
using Microsoft.AspNetCore.Authorization;

// Core MVC namespace for the Controller base class and IActionResult
using Microsoft.AspNetCore.Mvc;

// Application database context for querying the database
using GreenFieldWeb.Data;

// Entity Framework Core for async LINQ queries like FirstOrDefaultAsync
using Microsoft.EntityFrameworkCore;

// Provides ClaimTypes.NameIdentifier so we can get the logged-in user's ID
using System.Security.Claims;

namespace GreenFieldWeb.Controllers
{
    // The entire dashboard is restricted to Producers, Developers and Admins
    // Any user without one of these roles is automatically redirected to the login page
    [Authorize(Roles = "Producer,Developer,Admin")]
    public class ProducerDashboardController : Controller
    {
        // _context gives us access to the database through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically
        public ProducerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /ProducerDashboard — the main dashboard page showing stats, stock levels and recent orders
        public async Task<IActionResult> Index()
        {
            // Get the ID of the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Find the producer record linked to this user account
            var producer = await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);

            // If no producer record exists for this user, return 404
            // This handles edge cases like an admin who isn't also a producer
            if (producer == null)
            {
                return NotFound();
            }

            // Load all products that belong to this producer
            var products = await _context.Products.Where(p => p.ProducersId == producer.ProducersId).ToListAsync();

            // Load all orders that contain at least one product from this producer
            // ThenInclude loads the products within each order product so we can check the ProducersId
            var orders = await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Products)
                .Where(o => o.OrderProducts.Any(op => op.Products.ProducersId == producer.ProducersId))
                .ToListAsync();

            // Pass the total number of this producer's products to the stats card in the view
            ViewBag.TotalProducts = products.Count;

            // Count how many products have 5 or fewer units left — shown as a low stock warning
            ViewBag.LowStockCount = products.Count(x => x.Stock <= 5);

            // Pass the orders list to the view for the recent orders table
            ViewBag.RecentOrders = orders;

            // Pass the products list as the view model for the stock levels table
            return View(products);
        }
    }
}
