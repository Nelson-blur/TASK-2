using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GreenFieldWeb.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GreenFieldWeb.Controllers
{
   [Authorize(Roles = "Producer")]
    public class ProducerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProducerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers.FirstOrDefaultAsync(p => p.UserId == userId);
            if(producer == null)
            {
                return NotFound();
            }
            var products = await _context.Products.Where(p => p.ProducersId == producer.ProducersId).ToListAsync();
            var orders = await _context.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Products)
                                              .Where(o => o.OrderProducts.Any(op => op.Products.ProducersId == producer.ProducersId))
                                              .ToListAsync();
            ViewBag.TotalProducts = products.Count;
            ViewBag.LowStockCount = products.Count(x => x.Stock <= 5);
            ViewBag.RecentOrders = orders;
            return View(products);
        }
    }
}
