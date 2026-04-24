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

// Provides the [Authorize] and [Authorize(Roles = "...")] attributes for access control
using Microsoft.AspNetCore.Authorization;

namespace GreenFieldWeb.Controllers
{
    // OrdersController handles everything related to orders — placing them, viewing them, updating their status, and deleting them
    // Different functionality is available depending on the user's role: Admin, Producer, or standard customer
    public class OrdersController : Controller
    {
        // _context gives us access to the database through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Orders — shows the orders list, filtered by the logged-in user's role
        // [Authorize] ensures only logged-in users can view orders
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get the ID of the currently logged-in user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If the user ID is null return 401 — shouldn't happen with [Authorize] but good to be safe
            if (userId == null)
            {
                return Unauthorized();
            }

            if (User.IsInRole("Admin"))
            {
                // Admins can see every single order in the system, with all products included
                var allOrders = await _context.Orders.Include(o => o.OrderProducts).ThenInclude(op => op.Products).ToListAsync();
                return View(allOrders);
            }
            else if (User.IsInRole("Producer"))
            {
                // Producers should only see orders that contain their own products
                // Step 1: Find all product IDs that belong to this producer
                var producerProducts = await _context.Products.Where(p => p.Producers.UserId == userId).Select(p => p.ProductsId).ToListAsync();

                // Step 2: Find all order product records that contain any of those product IDs, and load their related orders
                var producerOrders = await _context.OrderProducts.Where(op => producerProducts.Contains(op.ProductsId)).Include(op => op.Orders).ThenInclude(o => o.OrderProducts).ThenInclude(op => op.Products).ToListAsync();

                // Step 3: Return the distinct orders (removing duplicates where multiple products from the same producer appear in one order)
                return View(producerOrders.Select(op => op.Orders).Distinct().ToList());
            }
            else
            {
                // Standard customers only see their own orders, with all products in each order included
                var userOrders = await _context.Orders.Where(o => o.UserId == userId).Include(o => o.OrderProducts).ThenInclude(op => op.Products).ToListAsync();
                return View(userOrders);
            }
        }

        // GET: /Orders/Details/5 — shows the full details of a specific order
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            // Return 404 if no ID was provided in the URL
            if (id == null)
            {
                return NotFound();
            }

            // Get the logged-in user's ID for the ownership check below
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch all order product records for this order, including the product and order details
            // The view is typed to IEnumerable<OrderProducts> rather than a single Order
            var orders = await _context.OrderProducts
                .Where(op => op.OrdersId == id)
                .Include(op => op.Products)
                .Include(op => op.Orders)
                .ToListAsync();

            // If no records were found for this order ID, return 404
            if (!orders.Any()) return NotFound();

            // Security check — customers can only view their own orders
            // Admins and Producers can view any order they have access to
            if (!User.IsInRole("Admin") && !User.IsInRole("Producer"))
            {
                // Compare the order's UserId against the logged-in user's ID
                if (orders.First().Orders.UserId != userId)
                {
                    // If they don't match, return 403 Forbidden — they're trying to view someone else's order
                    return Forbid();
                }
            }

            // All checks passed — show the order details
            return View(orders);
        }

        // GET: /Orders/Create?basketId=X — shows the checkout form
        // The basketId comes from the URL query string so we know which basket to convert to an order
        [Authorize]
        public IActionResult Create(int basketId)
        {
            // Pass the basketId to the view so the form can include it as a hidden field
            ViewBag.basketId = basketId;
            return View();
        }

        // POST: /Orders/Create — processes the checkout form and creates the order
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("OrdersId,DeliveryMethod,DeliveryDate,DeliveryAddress")] Orders orders, int basketId)
        {
            // Get the logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If the user somehow isn't logged in, show the form again rather than crashing
            if (userId == null)
            {
                ViewBag.BasketId = basketId;
                return View(orders);
            }

            // Assign server-side values that should never come from the form
            orders.UserId = userId;
            ModelState.Remove("UserId"); // Remove from validation since we set it ourselves

            // Set the order date to today's date automatically
            orders.OrderDate = DateOnly.FromDateTime(DateTime.Now);
            ModelState.Remove("OrderDate");

            // All new orders start as Pending
            orders.OrderStatus = "Pending";
            ModelState.Remove("OrderStatus");

            // Find the user's active basket — it must match the basketId AND belong to this user AND be open
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.BasketId == basketId && x.UserId == userId && x.Status);

            // If the basket doesn't exist or isn't theirs, return 404
            if (basket == null) return NotFound();

            // Load all the products in the basket so we can calculate totals and create order records
            var basketProducts = await _context.BasketProducts
                .Where(x => x.BasketId == basketId)
                .Include(x => x.Products)
                .ToListAsync();

            // If the basket is empty, show an error rather than placing an empty order
            if (!basketProducts.Any())
            {
                ModelState.AddModelError("", "Your basket is empty.");
                ViewBag.BasketId = basketId;
                return View(orders);
            }

            // Calculate the subtotal by adding up price × quantity for every item in the basket
            decimal subtotal = 0.00m;
            foreach (var basketProduct in basketProducts)
            {
                subtotal += basketProduct.Products.Price * basketProduct.Quantity;
            }

            // Determine the loyalty discount based on how many orders this user has placed before
            var orderCount = await _context.Orders.CountAsync(x => x.UserId == userId);
            decimal discount = 0m;

            if (orderCount >= 10)
                discount = subtotal * 0.15m; // 15% for 10+ orders — top tier
            else if (orderCount >= 5)
                discount = subtotal * 0.10m; // 10% for 5-9 orders — mid tier
            else if (orderCount >= 1)
                discount = subtotal * 0.05m; // 5% for 1-4 orders — entry tier

            // Store the discount amount on the order and remove it from model validation
            orders.DiscountApplied = discount;
            ModelState.Remove("DiscountApplied");

            // Calculate the delivery fee — £3.99 for home delivery, free for collection
            decimal deliveryFee = 0m;
            if (orders.DeliveryMethod == "Delivery")
                deliveryFee = 3.99m;

            orders.DeliveryFee = deliveryFee;
            ModelState.Remove("DeliveryFee");

            // Calculate the final total — subtotal minus discount plus any delivery fee
            orders.TotalAmount = subtotal - discount + deliveryFee;
            ModelState.Remove("TotalAmount");

            // Validate that the user actually chose a delivery method
            if (string.IsNullOrWhiteSpace(orders.DeliveryMethod))
            {
                ModelState.AddModelError("DeliveryMethod", "Must choose Collection or Delivery");
            }

            if (orders.DeliveryMethod == "Collection")
            {
                // Collection orders don't need a delivery address — remove it from validation
                ModelState.Remove("DeliveryAddress");
                orders.DeliveryAddress = string.Empty; // Set to empty rather than null to avoid database constraint errors

                // Collection date is required
                if (orders.DeliveryDate == null)
                {
                    ModelState.AddModelError("DeliveryDate", "Collection date is required.");
                }
                else
                {
                    // Collection must be booked at least 2 days in advance to give time to prepare the order
                    var earliestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
                    if (orders.DeliveryDate.Value < earliestDate)
                        ModelState.AddModelError("DeliveryDate", "Collection must be at least 2 days from today.");
                }
            }

            if (orders.DeliveryMethod == "Delivery")
            {
                // Delivery orders require both a date and an address
                if (orders.DeliveryDate == null)
                    ModelState.AddModelError("DeliveryDate", "Delivery date is required.");

                if (string.IsNullOrWhiteSpace(orders.DeliveryAddress))
                    ModelState.AddModelError("DeliveryAddress", "Delivery address is required.");
            }

            // If any validation errors exist, show the form again with the error messages
            if (!ModelState.IsValid)
            {
                ViewBag.BasketId = basketId;
                return View(orders);
            }

            // Stock check — verify we have enough stock for every item BEFORE saving anything
            // This prevents orphaned order records if a stock check fails midway through
            foreach (var basketProduct in basketProducts)
            {
                if (basketProduct.Products.Stock < basketProduct.Quantity)
                {
                    // Not enough stock — show an error and don't create the order
                    ModelState.AddModelError("", $"Not enough stock for {basketProduct.Products.ProductName}");
                    ViewBag.BasketId = basketId;
                    return View(orders);
                }
            }

            // All checks passed — save the order to the database
            _context.Orders.Add(orders);
            await _context.SaveChangesAsync(); // Save now so we get an OrdersId to reference in the next step

            // Loop through basket items to create an OrderProduct record for each one and reduce stock
            foreach (var basketProduct in basketProducts)
            {
                // Create a record linking this order to this product with the quantity ordered
                _context.OrderProducts.Add(new OrderProducts
                {
                    OrdersId = orders.OrdersId,               // Link to the order we just created
                    ProductsId = basketProduct.ProductsId,    // Link to the product
                    Quantity = basketProduct.Quantity          // Record how many were ordered
                });

                // Reduce the product's stock level to reflect the quantity that was just sold
                basketProduct.Products.Stock -= basketProduct.Quantity;
            }

            // Close the basket by marking it as inactive — it's been converted to an order
            basket.Status = false;

            // Save all the order product records and stock changes together
            await _context.SaveChangesAsync();

            // Redirect the customer to their order history so they can see the newly placed order
            return RedirectToAction("Index", "Orders");
        }

        // GET: /Orders/Edit/5 — shows the order status update form
        // Only Producers and Admins can update order status
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the order by ID
            var orders = await _context.Orders.FindAsync(id);

            if (orders == null)
            {
                return NotFound();
            }

            // Pass the full order to the view so we can display the order summary alongside the status dropdown
            return View(orders);
        }

        // POST: /Orders/Edit/5 — saves the updated order status
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int id, string OrderStatus)
        {
            // Fetch the real order from the database — we only accept the new status from the form
            // This prevents the producer from changing any other order fields
            var order = await _context.Orders.FindAsync(id);

            if (order == null) return NotFound();

            // Only update the status field — everything else stays exactly as it was
            order.OrderStatus = OrderStatus;

            try
            {
                // Tell Entity Framework to track and save this update
                _context.Update(order);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // If the order was deleted by someone else while we were editing, return 404
                if (!OrdersExists(id)) return NotFound();
                else throw; // Otherwise something unexpected happened — rethrow
            }

            // Redirect back to the orders list after a successful status update
            return RedirectToAction(nameof(Index));
        }

        // GET: /Orders/Delete/5 — shows the delete confirmation page (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the order to display its details on the confirmation page
            var orders = await _context.Orders
                .FirstOrDefaultAsync(m => m.OrdersId == id);

            if (orders == null)
            {
                return NotFound();
            }

            return View(orders);
        }

        // POST: /Orders/Delete/5 — performs the deletion after admin confirms (Admin only)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Load the order AND its related OrderProducts in one query using Include
            var orders = await _context.Orders
                .Include(o => o.OrderProducts)
                .FirstOrDefaultAsync(o => o.OrdersId == id);

            if (orders == null)
            {
                return NotFound();
            }

            // Delete the OrderProducts records first — they have a foreign key reference to the Order
            // If we deleted the Order first, the database would throw a foreign key constraint error
            _context.OrderProducts.RemoveRange(orders.OrderProducts);

            // Now that the child records are gone, we can safely delete the parent order
            _context.Orders.Remove(orders);

            // Save both deletions in a single database transaction
            await _context.SaveChangesAsync();

            // Redirect back to the orders list
            return RedirectToAction(nameof(Index));
        }

        // Helper method — checks if an order with the given ID exists in the database
        private bool OrdersExists(int id)
        {
            return _context.Orders.Any(e => e.OrdersId == id);
        }
    }
}
