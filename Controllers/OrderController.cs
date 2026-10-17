using Ecommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context) => _context = context;

        // Ye 2 route add kiye hain - Ab /Orders aur /Order/MyOrders dono chalenge
        [Route("Orders")]
        [Route("Orders/Index")]
        [Route("Order/MyOrders")]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var orders = await _context.Orders
             .Include(o => o.OrderItems)
             .ThenInclude(i => i.Product)
             .Where(o => o.UserId == userId)
             .OrderByDescending(o => o.OrderDate)
             .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var order = await _context.Orders
             .Include(o => o.OrderItems)
             .ThenInclude(i => i.Product)
             .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            if (!User.IsInRole("Admin") && order.UserId != userId)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}