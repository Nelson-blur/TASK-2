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

// Provides the [Authorize] attribute so we can restrict access to logged-in users only
using Microsoft.AspNetCore.Authorization;

namespace GreenFieldWeb.Controllers
{
    // BasketsController manages the customer's shopping basket
    // It handles viewing the basket, calculating totals, and applying loyalty discounts
    public class BasketsController : Controller
    {
        // _context gives us access to all database tables through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — the database context is injected automatically by ASP.NET Core
        public BasketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Baskets — shows the current user's basket with all items and totals
        // [Authorize] ensures only logged-in users can access the basket page
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Get the unique ID of the currently logged-in user from their authentication token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If somehow the user ID is null despite being logged in, return 401
            if (userId == null)
            {
                return Unauthorized();
            }

            // Try to find an existing active basket for this user
            // Status == true means the basket is open and hasn't been converted to an order yet
            var basket = await _context.Basket
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Status);

            // If the user doesn't have a basket yet, create one automatically
            if (basket == null)
            {
                basket = new Basket
                {
                    Status = true,                    // Mark as active/open
                    UserId = userId,                  // Link to the current user
                    BasketCreatedAt = DateTime.UtcNow // Record the creation time
                };

                // Add the new basket to the tracking context
                _context.Basket.Add(basket);

                // Save immediately so we get a BasketId to reference when loading products
                await _context.SaveChangesAsync();
            }

            // Always fetch basket products outside the if block so this runs whether the basket is new or existing
            // Include the related Products so we can display product names and prices
            var basketProducts = await _context.BasketProducts
                .Where(x => x.BasketId == basket.BasketId)
                .Include(x => x.Products)
                .ToListAsync();

            // Calculate the subtotal by multiplying each product's price by its quantity and summing them all
            decimal subtotal = 0m;

            foreach (var basketProduct in basketProducts)
            {
                var productTotal = basketProduct.Products.Price * basketProduct.Quantity;
                subtotal += productTotal;
            }

            // Count how many completed orders this user has placed — used to determine their loyalty tier
            var orderCount = await _context.Orders
                .CountAsync(x => x.UserId == userId);

            // Apply a 10% loyalty discount if the customer has placed 5 or more previous orders
            decimal discount = 0m;

            if (orderCount >= 5)
            {
                discount = subtotal * 0.10m; // 10% off
            }

            // Calculate the final total after the loyalty discount is applied
            decimal total = subtotal - discount;

            // Pass all calculated values to the view through ViewBag so they can be displayed in the basket summary
            ViewBag.TotalAmount = subtotal;
            ViewBag.Discount = discount;
            ViewBag.Total = total;
            ViewBag.OrderCount = orderCount;

            // Pass the list of basket products as the view model so the view can loop through and display each item
            return View(basketProducts);
        }

        // GET: /Baskets/Details/5 — shows details for a specific basket record
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the basket record with the matching ID
            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);

            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // GET: /Baskets/Create — shows the create basket form (scaffolded, rarely used directly)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Baskets/Create — creates a new basket record from form data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BasketId,UserId,Status,BasketCreatedAt")] Basket basket)
        {
            if (ModelState.IsValid)
            {
                // Add the new basket and save it to the database
                _context.Add(basket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(basket);
        }

        // GET: /Baskets/Edit/5 — shows the edit form for a basket record
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the basket by ID
            var basket = await _context.Basket.FindAsync(id);

            if (basket == null)
            {
                return NotFound();
            }
            return View(basket);
        }

        // POST: /Baskets/Edit/5 — saves changes to a basket record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BasketId,UserId,Status,BasketCreatedAt")] Basket basket)
        {
            // Make sure the ID in the URL matches the ID in the submitted form
            if (id != basket.BasketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the basket record in the database
                    _context.Update(basket);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If the record no longer exists, return 404
                    if (!BasketExists(basket.BasketId))
                    {
                        return NotFound(); 
                    }
                    else
                    {
                        throw; // Something unexpected went wrong — rethrow
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(basket);
        }

        // GET: /Baskets/Delete/5 — shows the delete confirmation page for a basket
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);

            if (basket == null)
            {
                return NotFound();
            }

            return View(basket);
        }

        // POST: /Baskets/Delete/5 — performs the deletion after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the basket by its ID
            var basket = await _context.Basket.FindAsync(id);

            if (basket != null)
            {
                // Remove the basket from the database
                _context.Basket.Remove(basket);
            }

            // Save the deletion and redirect back to the basket list
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method — checks if a basket with the given ID still exists
        // Used to handle concurrency exceptions in the Edit method
        private bool BasketExists(int id)
        {
            return _context.Basket.Any(e => e.BasketId == id);
        }
    }
}
