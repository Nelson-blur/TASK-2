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
using Microsoft.AspNetCore.Http;
using System.IO;

namespace GreenFieldWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: Products
        public async Task<IActionResult> Index(string searchString,decimal? minPrice,decimal? maxPrice, string farmingMethod )
         
        {
            

            var products = _context.Products.Include(p => p.Producers).AsQueryable();
            if (User.IsInRole("Producer"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized();

                var producer = await _context.Producers
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                if (producer == null) return NotFound();

                products = products.Where(p => p.ProducersId == producer.ProducersId);
            }
            else
            {
                products = products.Where(p => p.IsAvailable);
            }



            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.ProductName.Contains(searchString) || p.Description.Contains(searchString));
            }
            
            if (!string.IsNullOrEmpty(farmingMethod))
            {
                products = products.Where(p => p.FarmingMethod == farmingMethod);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
            }
           

            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentFarmingMethod = farmingMethod;
            ViewBag.FarmingMethods = await _context.Products.Select(p => p.FarmingMethod).Distinct().Where(f => f != null).ToListAsync();
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id);
            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // GET: Products/Create
        [Authorize(Roles = "Producer,Admin")]
        public IActionResult Create()
        {
            
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Create([Bind("ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod,ImageUrl")] Products products, IFormFile imageFile)
        {
            // Get the logged in user's supplier record
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (producer == null) return Forbid();
            if (imageFile != null && imageFile.Length > 0)
            {
                // Generate unique filename to avoid overwriting
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                // With this — uses the actual wwwroot path properly
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                products.ImageUrl = fileName;
            }
            else
            {
                // Default image if none uploaded
                products.ImageUrl = "default.jpg";
            }


            products.ProducersId = producer.ProducersId;
            products.CreatedAt = DateTime.UtcNow;
            products.UpdatedAt = DateTime.UtcNow;

            ModelState.Remove("ProducersId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("UpdatedAt");

            if (ModelState.IsValid)
            {
                _context.Add(products);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "ProducerDashboard");
            }

            return View(products);
        }



        // GET: Products/Edit/5
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products.FindAsync(id);
            if (products == null)
            {
                return NotFound();
            }
            
            return View(products);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ProductsId,ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod,ImageUrl")] Products products, IFormFile imageFile)
        {
            if (id != products.ProductsId) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (producer == null) return NotFound();

            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                // With this — uses the actual wwwroot path properly
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                products.ImageUrl = fileName;
            }
            else
            {
                // Keep the existing image — fetch it from DB
                var existing = await _context.Products.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ProductsId == products.ProductsId);
                products.ImageUrl = existing?.ImageUrl ?? "default.jpg";
            }

            ModelState.Remove("ImageUrl");

            // Verify ownership — same as Delete
            var existingProduct = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductsId == id);

            if (existingProduct == null) return NotFound();

            if (existingProduct.ProducersId != producer.ProducersId)
                return Forbid();

            // Set server-side fields
            products.ProducersId = producer.ProducersId;
            products.CreatedAt = existingProduct.CreatedAt; // preserve original creation date
            products.UpdatedAt = DateTime.UtcNow;

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




        // GET: Products/Delete/5
        [Authorize(Roles = "Producer,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var products = await _context.Products
                .Include(p => p.Producers)
                .FirstOrDefaultAsync(m => m.ProductsId == id);
            if (products == null)
            {
                return NotFound();
            }

            return View(products);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var producer = await _context.Producers.FirstOrDefaultAsync(s => s.UserId == userId);
            if (producer == null)
            {
                return NotFound();
            }


            var products = await _context.Products.FindAsync(id);
            if (products == null)
            {
                return NotFound();
            }

            // Checks the product actually belongs to this producer
            if (products.ProducersId != producer.ProducersId)
            {
                return Unauthorized();
            }

            _context.Products.Remove(products);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductsExists(int id)
        {
            return _context.Products.Any(e => e.ProductsId == id);
        }



    }
}
