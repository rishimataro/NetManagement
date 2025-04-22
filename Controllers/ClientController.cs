using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetManagement.Data;
using NetManagement.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace NetManagement.Controllers
{
    public class ClientController : BaseController
    {
        private readonly NetManagementContext _context;

        public ClientController(NetManagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var availableComputers = await _context.Computer
                .Where(c => c.Status == ComputerStatus.Available)
                .ToListAsync();
            return View(availableComputers);
        }

        public async Task<IActionResult> UseComputer(int id)
        {
            var computer = await _context.Computer.FindAsync(id);
            if (computer == null || computer.Status != ComputerStatus.Available)
            {
                return NotFound();
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value);
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Create new order
            var order = new Order
            {
                UserId = userId,
                ComputerId = id,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                CreateAt = DateTime.Now,
                isPay = false,
                TotalCost = 0
            };

            // Update computer status
            computer.Status = ComputerStatus.InUse;
            _context.Update(computer);
            _context.Add(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("CurrentSession", new { orderId = order.Id });
        }

        public async Task<IActionResult> CurrentSession(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Computer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Calculate current cost
            var duration = DateTime.Now - order.StartTime;
            var currentCost = (decimal)duration.TotalHours * order.Computer.PricePerHour;

            ViewBag.CurrentCost = currentCost;
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> EndSession(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Computer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Update order
            order.EndTime = DateTime.Now;
            var duration = order.EndTime.Value - order.StartTime;
            order.TotalCost = (decimal)duration.TotalHours * order.Computer.PricePerHour;

            // Update computer status
            order.Computer.Status = ComputerStatus.Available;
            order.Computer.BookingCount++; // Increment booking count
            _context.Update(order.Computer);
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Payment", new { orderId = order.Id });
        }

        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Computer)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.User.Balance >= order.TotalCost)
            {
                // Deduct from user's balance
                order.User.Balance -= order.TotalCost;
                order.isPay = true;

                _context.Update(order.User);
                _context.Update(order);
                await _context.SaveChangesAsync();

                return RedirectToAction("PaymentSuccess", new { orderId = order.Id });
            }
            else
            {
                return RedirectToAction("PaymentFailed", new { orderId = order.Id });
            }
        }

        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Computer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public async Task<IActionResult> PaymentFailed(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Computer)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        public IActionResult TopUp()
        {
            var userId = int.Parse(User.FindFirst("Id")?.Value);
            var user = _context.User.Find(userId);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessTopUp(int amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Số tiền nạp phải lớn hơn 0";
                return RedirectToAction("TopUp");
            }

            var userId = int.Parse(User.FindFirst("Id")?.Value);
            var user = await _context.User.FindAsync(userId);
            if (user == null)
            {
                return RedirectToAction("SignIn", "Account");
            }

            // Update user balance
            user.Balance += amount;
            _context.Update(user);
            await _context.SaveChangesAsync();

            // Update claims
            var claims = User.Claims.ToList();
            var balanceClaim = claims.FirstOrDefault(c => c.Type == "Balance");
            if (balanceClaim != null)
            {
                claims.Remove(balanceClaim);
            }
            claims.Add(new Claim("Balance", user.Balance.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            TempData["Success"] = $"Nạp tiền thành công: {amount:N0} VNĐ";
            return RedirectToAction("TopUp");
        }
    }
} 