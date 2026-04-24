// Standard library imports for collections, LINQ and async operations
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Core MVC namespaces for controller base class, rendering helpers and routing
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// Entity Framework Core for async database operations
using Microsoft.EntityFrameworkCore;

// Application-specific namespaces for the database context and data models
using GreenFieldWeb.Data;
using GreenFieldWeb.Models;

// Provides ClaimTypes.NameIdentifier so we can get the logged-in user's ID
using System.Security.Claims;

namespace GreenFieldWeb.Controllers
{
    // BasketProductsController manages the items inside a customer's basket
    // Each BasketProduct record links a basket to a product with a quantity
    public class BasketProductsController : Controller
    {
        // _context provides access to all database tables through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically at runtime
        public BasketProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /BasketProducts — lists all basket product records (mainly for admin/debug use)
        public async Task<IActionResult> Index()
        {
            // Load all basket products and include their related Basket and Product details
            var applicationDbContext = _context.BasketProducts.Include(b => b.Basket).Include(b => b.Products);

            // Execute the query and pass the results to the view
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: /BasketProducts/Details/5 — shows details of a single basket product record
        public async Task<IActionResult> Details(int? id)
        {
            // If no ID was provided in the URL, return a 404 response
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the basket product with the matching ID, including its related Basket and Product
            var basketProducts = await _context.BasketProducts
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductsId == id);

            // If no record was found with that ID, return 404
            if (basketProducts == null)
            {
                return NotFound();
            }

            // Pass the found record to the view for display
            return View(basketProducts);
        }

        // GET: /BasketProducts/Create — shows the default scaffolded create form
        // Note: customers never use this directly — they use the Add to Basket button on the products page
        public IActionResult Create()
        {
            // Populate dropdown lists for the BasketId and ProductsId fields in the form
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId");
            ViewData["ProductsId"] = new SelectList(_context.Set<Products>(), "ProductsId", "ProductsId");
            return View();
        }

        // POST: /BasketProducts/Create — handles the Add to Basket form submission from the products page
        // This is the core add-to-basket logic — it finds or creates the user's basket and adds the product
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against cross-site request forgery attacks
        public async Task<IActionResult> Create(int ProductsId)
        {
            // Look up the product the user wants to add by its ID
            var product = await _context.Products.FirstOrDefaultAsync(x => x.ProductsId == ProductsId);

            // If the product doesn't exist in the database, return 404
            if (product == null)
            {
                return NotFound();
            }

            // Get the ID of the currently logged-in user from their authentication token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // If the user isn't logged in, return 401 Unauthorized
            if (userId == null)
            {
                return Unauthorized();
            }

            // Try to find an existing active basket for this user (Status == true means open/active)
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status == true);

            // If the user doesn't have an active basket yet, create one for them
            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,              // Link the basket to this user
                    Status = true,                // Mark it as active/open
                    BasketCreatedAt = DateTime.UtcNow  // Record when it was created
                };

                // Add the new basket to the database context
                _context.Basket.Add(basket);

                // Save immediately so we get a BasketId before we try to add products to it
                await _context.SaveChangesAsync();
            }

            // Check if this product is already in the basket
            var basketProduct = await _context.BasketProducts.FirstOrDefaultAsync(bp => bp.BasketId == basket.BasketId && bp.ProductsId == ProductsId);

            if (basketProduct != null)
            {
                // Product is already in the basket — just increase the quantity by 1
                basketProduct.Quantity++;
            }
            else
            {
                // Product is not in the basket yet — create a new basket product record
                basketProduct = new BasketProducts
                {
                    BasketId = basket.BasketId,   // Link to the user's basket
                    ProductsId = ProductsId,       // Link to the product being added
                    Quantity = 1                   // Start with a quantity of 1
                };
                _context.BasketProducts.Add(basketProduct);
            }

            // Save the changes (either the quantity update or the new basket product record)
            await _context.SaveChangesAsync();

            // Redirect the user to the basket page so they can see their updated basket
            return RedirectToAction("Index", "Baskets");
        }

        // GET: /BasketProducts/Edit/5 — shows the edit form for a basket product record
        public async Task<IActionResult> Edit(int? id)
        {
            // Return 404 if no ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Find the basket product by ID
            var basketProducts = await _context.BasketProducts.FindAsync(id);

            // Return 404 if not found
            if (basketProducts == null)
            {
                return NotFound();
            }

            // Populate the dropdown lists, pre-selecting the current values
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProducts.BasketId);
            ViewData["ProductsId"] = new SelectList(_context.Set<Products>(), "ProductsId", "ProductsId", basketProducts.ProductsId);

            return View(basketProducts);
        }

        // POST: /BasketProducts/Edit/5 — saves changes to a basket product record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BasketProductsId,BasketId,ProductsId,Quantity")] BasketProducts basketProducts)
        {
            // Verify the ID in the URL matches the ID in the submitted form data
            if (id != basketProducts.BasketProductsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tell Entity Framework to track and update this record
                    _context.Update(basketProducts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle the case where another user edited or deleted this record at the same time
                    if (!BasketProductsExists(basketProducts.BasketProductsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Something else went wrong — rethrow the exception
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, repopulate the dropdowns and show the form again with errors
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProducts.BasketId);
            ViewData["ProductsId"] = new SelectList(_context.Set<Products>(), "ProductsId", "ProductsId", basketProducts.ProductsId);
            return View(basketProducts);
        }

        // GET: /BasketProducts/Delete/5 — shows the delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the basket product along with its related basket and product for display on the confirmation page
            var basketProducts = await _context.BasketProducts
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductsId == id);

            if (basketProducts == null)
            {
                return NotFound();
            }

            return View(basketProducts);
        }

        // POST: /BasketProducts/Delete/5 — performs the actual deletion after the user confirms
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the basket product record by its ID
            var basketProducts = await _context.BasketProducts.FindAsync(id);

            if (basketProducts != null)
            {
                // Remove the record from the database context
                _context.BasketProducts.Remove(basketProducts);
            }

            // Save the deletion to the database
            await _context.SaveChangesAsync();

            // Redirect back to the basket page after removing the item
            return RedirectToAction("Index", "Baskets");
        }

        // Helper method — checks whether a basket product with the given ID exists in the database
        // Used to handle concurrency exceptions gracefully in the Edit method
        private bool BasketProductsExists(int id)
        {
            return _context.BasketProducts.Any(e => e.BasketProductsId == id);
        }
    }
}
