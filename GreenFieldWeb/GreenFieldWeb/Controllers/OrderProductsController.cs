using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GreenFieldWeb.Data;
using GreenFieldWeb.Models;

namespace GreenFieldWeb.Controllers
{
    // OrderProductsController manages the individual line items within an order
    // Each OrderProduct record links an order to a specific product with a quantity
    // This controller is mostly scaffolded and used for admin/debug purposes
    // In normal use, order products are created automatically when an order is placed
    public class OrderProductsController : Controller
    {
        // _context gives us access to the database through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically
        public OrderProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /OrderProducts — lists all order product records in the database
        public async Task<IActionResult> Index()
        {
            // Load all order product records and return them to the view
            return View(await _context.OrderProducts.ToListAsync());
        }

        // GET: /OrderProducts/Details/5 — shows the details of a single order product record
        public async Task<IActionResult> Details(int? id)
        {
            // Return 404 if no ID was provided
            if (id == null)
            {
                return NotFound();
            }

            // Find the order product record with the matching ID
            var orderProducts = await _context.OrderProducts
                .FirstOrDefaultAsync(m => m.OrderProductsId == id);

            // Return 404 if no record was found
            if (orderProducts == null)
            {
                return NotFound();
            }

            return View(orderProducts);
        }

        // GET: /OrderProducts/Create — shows the scaffolded create form
        // Note: in normal use, order products are created automatically through the checkout process
        public IActionResult Create()
        {
            return View();
        }

        // POST: /OrderProducts/Create — saves a new order product record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderProductsId,OrdersId,ProductsId,Quantity")] OrderProducts orderProducts)
        {
            if (ModelState.IsValid)
            {
                // Add the record to the database and save
                _context.Add(orderProducts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, show the form again
            return View(orderProducts);
        }

        // GET: /OrderProducts/Edit/5 — shows the edit form for an order product record
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the record by ID
            var orderProducts = await _context.OrderProducts.FindAsync(id);

            if (orderProducts == null)
            {
                return NotFound();
            }

            return View(orderProducts);
        }

        // POST: /OrderProducts/Edit/5 — saves changes to an order product record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderProductsId,OrdersId,ProductsId,Quantity")] OrderProducts orderProducts)
        {
            // Verify the ID in the URL matches the submitted form data
            if (id != orderProducts.OrderProductsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the record in the database
                    _context.Update(orderProducts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If the record was deleted by someone else while editing, return 404
                    if (!OrderProductsExists(orderProducts.OrderProductsId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Unexpected error — rethrow it
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(orderProducts);
        }

        // GET: /OrderProducts/Delete/5 — shows the delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the record to display on the confirmation page
            var orderProducts = await _context.OrderProducts
                .FirstOrDefaultAsync(m => m.OrderProductsId == id);

            if (orderProducts == null)
            {
                return NotFound();
            }

            return View(orderProducts);
        }

        // POST: /OrderProducts/Delete/5 — performs the deletion after confirmation
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the record by ID
            var orderProducts = await _context.OrderProducts.FindAsync(id);

            if (orderProducts != null)
            {
                // Remove the record from the database
                _context.OrderProducts.Remove(orderProducts);
            }

            // Save the deletion and redirect to the list
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method — checks if an order product record with the given ID exists
        // Used to handle concurrency exceptions gracefully in the Edit method
        private bool OrderProductsExists(int id)
        {
            return _context.OrderProducts.Any(e => e.OrderProductsId == id);
        }
    }
}
