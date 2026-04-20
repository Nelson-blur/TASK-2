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
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Producer"))
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

                var ProducerProducts = await _context.Products.Where(p => p.ProducersId == producer.ProducersId).Include(p => p.Producers).ToListAsync();
                return View(ProducerProducts);
            }
            else
            {
                var allProducts = await _context.Products.Include(p => p.Producers).ToListAsync();
                return View(allProducts);
            }
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
        public async Task<IActionResult> Create([Bind("ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod,ImageUrl")] Products products)
        {
            // Get the logged in user's supplier record
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (producer == null) return Forbid();

            // Set server-side fields — never trust the form for these
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
                return RedirectToAction("Index", "SupplierDashboard");
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
        public async Task<IActionResult> Edit(int id, [Bind("ProductsId,ProductName,Price,Stock,Description,IsAvailable,AllergenInformation,FarmingMethod,ImageUrl")] Products products)
        {
            if (id != products.ProductsId) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var producer = await _context.Producers
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (producer == null) return NotFound();

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
