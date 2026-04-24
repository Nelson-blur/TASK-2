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
    // ProducersController manages the local producers listed on the website
    // It handles the public producers page that customers browse, as well as admin management of producer records
    public class ProducersController : Controller
    {
        // _context gives us access to the database through Entity Framework
        private readonly ApplicationDbContext _context;

        // Constructor — ASP.NET Core injects the database context automatically
        public ProducersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Producers — shows all producers as a public listing page
        public async Task<IActionResult> Index()
        {
            // Load all producer records and pass them to the view
            return View(await _context.Producers.ToListAsync());
        }

        // GET: /Producers/Details/5 — shows the full details page for a specific producer
        public async Task<IActionResult> Details(int? id)
        {
            // Return 404 if no ID was provided in the URL
            if (id == null)
            {
                return NotFound();
            }

            // Find the producer with the matching ID
            var producers = await _context.Producers
                .FirstOrDefaultAsync(m => m.ProducersId == id);

            // If no producer was found with that ID, return 404
            if (producers == null)
            {
                return NotFound();
            }

            // Pass the found producer to the view for display
            return View(producers);
        }

        // GET: /Producers/Create — shows the form to add a new producer (Admin use)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Producers/Create — saves a new producer to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProducersId,UserId,ProducerName,Description,BusinessLocation,ContactEmail")] Producers producers)
        {
            if (ModelState.IsValid)
            {
                // Add the new producer record and save it to the database
                _context.Add(producers);
                await _context.SaveChangesAsync();

                // Redirect to the producers list after successful creation
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, show the form again with error messages
            return View(producers);
        }

        // GET: /Producers/Edit/5 — shows the edit form for a producer record
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Find the producer by ID using the faster FindAsync method
            var producers = await _context.Producers.FindAsync(id);

            if (producers == null)
            {
                return NotFound();
            }

            return View(producers);
        }

        // POST: /Producers/Edit/5 — saves changes to an existing producer record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProducersId,UserId,ProducerName,Description,BusinessLocation,ContactEmail")] Producers producers)
        {
            // Verify the ID in the URL matches the ID in the submitted form
            if (id != producers.ProducersId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tell Entity Framework to track and update this record
                    _context.Update(producers);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle the case where this record was deleted by someone else while we were editing
                    if (!ProducersExists(producers.ProducersId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Something unexpected — rethrow the exception
                    }
                }

                // Redirect back to the producers list after a successful update
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, show the form again
            return View(producers);
        }

        // GET: /Producers/Delete/5 — shows the delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load the producer to show their details on the confirmation page
            var producers = await _context.Producers
                .FirstOrDefaultAsync(m => m.ProducersId == id);

            if (producers == null)
            {
                return NotFound();
            }

            return View(producers);
        }

        // POST: /Producers/Delete/5 — performs the deletion after the admin confirms
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the producer by ID
            var producers = await _context.Producers.FindAsync(id);

            if (producers != null)
            {
                // Remove the producer record from the database
                _context.Producers.Remove(producers);
            }

            // Save the deletion and redirect to the producers list
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Helper method — checks if a producer with the given ID still exists
        // Used to handle concurrency exceptions in the Edit method
        private bool ProducersExists(int id)
        {
            return _context.Producers.Any(e => e.ProducersId == id);
        }
    }
}
