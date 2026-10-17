using Ecommerce.Data;
using Microsoft.AspNetCore.Authorization; // 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Areas.Dashboard.Controllers
{
    [Area("Dashboard")]
    [Authorize(Roles = "Admin")] 
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalSales = (await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0).ToString("N0");
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
            return View();
        }

        public IActionResult Products()
        {
            return RedirectToAction("Index", "Products", new { area = "Dashboard" });
        }

        public async Task<IActionResult> Orders()
        {
            var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("OrderDetails", new { id = id });
        }

        public IActionResult Customers()
        {
            return View();
        }

        public async Task<IActionResult> Reports()
        {
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.TotalSales = (await _context.Orders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0).ToString("N0");
            return View();
        }

        public IActionResult Logout()
        {
            return Redirect("/");
        }
    }
}