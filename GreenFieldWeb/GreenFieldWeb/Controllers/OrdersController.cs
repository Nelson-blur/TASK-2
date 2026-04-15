using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenFieldWeb.Data;
using GreenFieldWeb.Models;
using System.Security.Claims;
using AspNetCoreGeneratedDocument;

namespace GreenFieldWeb.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrdersId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // GET: Orders/Create
        public IActionResult Create(int basketId)
        {
            ViewBag.basketId = basketId;
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrdersId,UserId,OrderDate,OrderStatus,DeliveryMethod,DeliveryDate,DeliveryAddress,DeliveryFee,DiscountApplied,TotalAmount")] Orders orders, int basketId)
        {
           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
           if (userId == null)
            {
                ViewBag.BasketId = basketId;
                return View(orders);
            }
           //Assign Values
           orders.UserId = userId;
           ModelState.Remove("UserId"); 

            orders.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            ModelState.Remove("OdrerDate");
            orders.OrderStatus = "Pending";
            ModelState.Remove("OrderStatus");
            
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.BasketId == basketId && x.UserId == userId && x.Status);
            if (basket == null)
            {
                return NotFound();
            }

            //Get basket products
            var basketProducts = await _context.BasketProducts
                .Where(x => x.BasketId == basketId)
                .Include(x => x.Products)
                .ToListAsync();
            if (!basketProducts.Any())
            {
                ModelState.AddModelError("", "Your basket is empty.");
                ViewBag.BasketId = basketId;
                return View(orders);
            }
            decimal subtotal = 0.00m;
            foreach (var basketProduct in basketProducts)
            {
                var productTotal = basketProduct.Products.Price * basketProduct.Quantity;
                subtotal = productTotal + subtotal;
            }

            var orderCount = await _context.Orders.CountAsync(x => x.UserId == userId);

            //Discount
            decimal discount = 0m;
            if (orderCount >= 5)
            {
                discount = subtotal * 0.10m; // 10% discount for 5 or more orders
            }
            orders.TotalAmount = subtotal - discount;
            ModelState.Remove("Subtotal");
            if (string.IsNullOrWhiteSpace(orders.DeliveryMethod))
            {
                ModelState.AddModelError("DeliveryMethod", "Must choose Collection or Delivery");
            }

            if (orders.DeliveryMethod == "Collection")
            {
                ModelState.Remove("DeliveryAddress");

                if (orders.DeliveryDate == null)
                {
                    ModelState.AddModelError("DeliveryDate", "Collection date is required.");
                }
                else
                {
                    var earliestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

                    if (orders.DeliveryDate.Value < earliestDate)
                    {
                        ModelState.AddModelError("DeliveryDate", "Collection must be at least 2 days from today.");
                    }
                }
            }

            if (orders.DeliveryMethod == "Delivery")
            {
                ModelState.Remove("DeliveryDate");

                if (string.IsNullOrWhiteSpace(orders.DeliveryAddress))
                {
                    ModelState.AddModelError("DeliveryAddress", "Delivery address is required.");
                }
            }
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders.FindAsync(id);
            if (orders == null)
            {
                return NotFound();
            }
            return View(orders);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrdersId,,UserId,OrderDate,OrderStatus,DeliveryMethod,DeliveryDate,DeliveryAddress,DeliveryFee,DiscountApplied,TotalAmount")] Orders orders)
        {
            if (id != orders.OrdersId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orders);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrdersExists(orders.OrdersId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(orders);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrdersId == id);
            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orders = await _context.Orders.FindAsync(id);
            if (orders != null)
            {
                _context.Orders.Remove(orders);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.OrdersId == id);
        }
    }
}
