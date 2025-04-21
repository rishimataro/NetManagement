using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetManagement.Data;
using NetManagement.Models;

namespace NetManagement.Controllers
{
    public class OrdersController : BaseController
    {
        private readonly NetManagementContext _context;

        public OrdersController(NetManagementContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var netManagementContext = _context.Order.Include(o => o.Computer);
            return View(await netManagementContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Computer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["ComputerId"] = new SelectList(_context.Computer, "Id", "Name");

            var order = new Order
            {
                StartTime = DateTime.Now,
                CreateAt = DateTime.Now,
                isPay = false
            };

            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ComputerId,StartTime,EndTime,TotalCost,CreateAt,isPay")] Order order)
        {
            if (ModelState.IsValid)
            {
                // Ensure the Computer exists
                var computer = await _context.Computer.FindAsync(order.ComputerId);
                if (computer == null)
                {
                    ModelState.AddModelError("ComputerId", "The selected computer does not exist.");
                    ViewData["ComputerId"] = new SelectList(_context.Computer, "Id", "Name", order.ComputerId);
                    return View(order);
                }

                // Calculate the total cost based on the duration of the order
                if (order.EndTime.HasValue)
                {
                    var duration = order.EndTime.Value - order.StartTime;
                    order.TotalCost = (decimal)duration.TotalHours * computer.PricePerHour;
                }
                else
                {
                    order.TotalCost = 0; // Default value if EndTime is not set
                }

                // Update the computer's booking count
                computer.BookingCount++;
                _context.Update(computer);

                // Set default values for the order
                order.CreateAt = DateTime.Now;
                order.isPay = false;

                // Add the order to the database
                _context.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            // Repopulate the Computer dropdown in case of validation errors
            ViewData["ComputerId"] = new SelectList(_context.Computer, "Id", "Name", order.ComputerId);
            return View(order);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["ComputerId"] = new SelectList(_context.Computer, "Id", "Name", order.ComputerId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ComputerId,StartTime,EndTime,TotalCost,CreateAt,isPay")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["ComputerId"] = new SelectList(_context.Computer, "Id", "Name", order.ComputerId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Computer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.Id == id);
        }
    }
}
