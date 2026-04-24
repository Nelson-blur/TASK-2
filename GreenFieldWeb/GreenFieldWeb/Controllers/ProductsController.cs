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
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

// Provides IFormFile so we can receive uploaded image files from forms
using Microsoft.AspNetCore.Http;

// Provides Path, Directory, FileStream and FileMode for saving uploaded images to disk
using System.IO;

namespace GreenFieldWeb.Controllers
{
    // ProductsController handles everything to do with products — browsing, searching, filtering, and CRUD operations
    // Customers can browse and search; only Producers and Admins can create, edit and delete
    public class ProductsController : Controller
    {
        // _context gives us access to the database through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Products — shows the products list with optional search and filter parameters
        // Parameters come from the URL query string e.g. /Products?searchString=honey&minPrice=1
        public async Task<IActionResult> Index(string searchString, decimal? minPrice, decimal? maxPrice, string farmingMethod)
        {
            // Start with all products, including their related producer details
            // AsQueryable keeps it as an IQueryable so we can stack Where filters before executing the query
            var products = _context.Products.Include(p => p.Producers).AsQueryable();

            // Apply role-based filtering before the search filters
            if (User.IsInRole("Producer"))
            {
                // Producers should only see their own products in the list
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                // Find the producer record linked to this user account
                var producer = await _context.Producers
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (producer == null) return NotFound();

                // Filter to only this producer's products — all subsequent filters apply within this set
                products = products.Where(p => p.ProducersId == producer.ProducersId);
            }
            else
            {
                // Customers and guests only see products that are marked as available
                products = products.Where(p => p.IsAvailable);
            }

            // Apply the text search filter if the user typed something in the search box
            if (!string.IsNullOrEmpty(searchString))
            {
                // Search both the product name and description — so searching "organic" finds products mentioning it in either field
                products = products.Where(p => p.ProductName.Contains(searchString) || p.Description.Contains(searchString));
            }

            // Apply the farming method filter if one was selected from the dropdown
            if (!string.IsNullOrEmpty(farmingMethod))
            {
                products = products.Where(p => p.FarmingMethod == farmingMethod);
            }

            // Apply the minimum price filter if one was entered
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }

            // Apply the maximum price filter if one was entered
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }

            // Pass the current filter values back to the view so the search inputs stay filled after submitting
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFarmingMethod = farmingMethod;

            // Get the distinct farming methods from the database to populate the filter dropdown
            ViewBag.FarmingMethods = await _context.Products.Select(p => p.FarmingMethod).Distinct().Where(f => f != null).ToListAsync();

            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            // Execute the final filtered query and pass the results to the view
            return View(await products.ToListAsync());
        }

        // GET: /Products/Details/5 — shows the full details page for a single product
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the product with its related producer so we can display the producer's name
            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id);

            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // GET: /Products/Create — shows the create product form (Producers and Admins only)
        [Authorize(Roles = "Producer,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Products/Create — handles the create product form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Create([Bind("ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod,ImageUrl")] Products products, IFormFile imageFile)
        {
            // Get the logged-in user's ID so we can find their producer record
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Look up the producer associated with this user account
            var producer = await _context.Producers
                .FirstOrDefaultAsync(p => p.UserId == userId);

            // If no producer record exists for this user, deny access
            if (producer == null) return Forbid();

            // Handle the image upload if one was provided
            if (imageFile != null && imageFile.Length > 0)
            {
                // Generate a unique filename using a GUID to avoid overwriting existing files with the same name
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

                // Build the full path to the wwwroot/images folder where product images are stored
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                // Create the images folder if it doesn't already exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Combine the folder path with the unique filename to get the full save path
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Open a file stream and copy the uploaded image data into it
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Store just the filename in the database — the view constructs the full URL
                products.ImageUrl = fileName;
            }
            else
            {
                // If no image was uploaded, use a default placeholder image
                products.ImageUrl = "default.jpg";
            }

            // Set the producer ID from the logged-in user's producer record — never trust the form for this
            products.ProducersId = producer.ProducersId;

            // Set the creation and last-updated timestamps automatically on the server
            products.CreatedAt = DateTime.UtcNow;
            products.UpdatedAt = DateTime.UtcNow;

            // Remove these fields from model validation since we set them ourselves
            ModelState.Remove("ProducersId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                // Add the product to the database and save
                _context.Add(products);
                await _context.SaveChangesAsync();

                // Send the producer back to their dashboard after creating a product
                return RedirectToAction("Index", "ProducerDashboard");
            }

            // If validation failed, show the form again with error messages
            return View(products);
        }

        // GET: /Products/Edit/5 — shows the edit form for a product (Producers and Admins only)
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the product by ID
            var products = await _context.Products.FindAsync(id);

            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // POST: /Products/Edit/5 — saves the changes to a product
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductsId,ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod")] Products products, IFormFile imageFile)
        {
            if (id != products.ProductsId) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers.FirstOrDefaultAsync(s => s.UserId == userId);
            if (producer == null) return NotFound();

            // Fetch existing product once — used for ownership check AND preserving old image
            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductsId == id);

            if (existingProduct == null) return NotFound();

            // Ownership check
            if (existingProduct.ProducersId != producer.ProducersId)
                return Forbid();

            // Handle image
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                products.ImageUrl = fileName;
            }
            else
            {
                // Keep existing image
                products.ImageUrl = existingProduct.ImageUrl ?? "default.jpg";
            }

            // Set server-side fields
            products.ProducersId = producer.ProducersId;
            products.CreatedAt = existingProduct.CreatedAt;
            products.UpdatedAt = DateTime.UtcNow;

            // Remove ALL server-set fields from validation before checking ModelState
            ModelState.Remove("ImageUrl");
            ModelState.Remove("ProducersId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(products);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductsExists(products.ProductsId)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Index", "ProducerDashboard");
            }

            return View(products);
        }

        // GET: /Products/Delete/5 — shows the delete confirmation page (Producers and Admins only)
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the product with its producer details for display on the confirmation page
            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id);

            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // POST: /Products/Delete/5 — performs the deletion after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get the logged-in user's ID to verify ownership
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            // Find the producer record linked to this user
            var producer = await _context.Producers.FirstOrDefaultAsync(s => s.UserId == userId);
            if (producer == null)
            {
                return NotFound();
            }

            // Find the product being deleted
            var products = await _context.Products.FindAsync(id);
            if (products == null)
            {
                return NotFound();
            }

            // Ownership check — make sure the product belongs to this producer
            // This prevents a producer from deleting another producer's products by guessing IDs
            if (products.ProducersId != producer.ProducersId)
            {
                return Unauthorized();
            }

            // Remove the product from the database and save
            _context.Products.Remove(products);
            await _context.SaveChangesAsync();

            // Redirect back to the products list
            return RedirectToAction(nameof(Index));
        }

        // Helper method — checks whether a product with the given ID exists in the database
        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductsId == id);
        }
    }
}
