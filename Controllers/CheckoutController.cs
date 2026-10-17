using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, IEmailService emailService, ILogger<CheckoutController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var carts = await _context.Carts.Include(c => c.Product)
                    .Where(c => c.UserId == userId).ToListAsync();

            if (!carts.Any()) return RedirectToAction("Index", "Cart");

            decimal subtotal = carts.Sum(c => c.Product!.Price * c.Quantity);
            ViewBag.Subtotal = subtotal;
            ViewBag.Discount = subtotal * 0.10m;
            ViewBag.FinalTotal = subtotal - (subtotal * 0.10m);
            return View(carts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string CustomerName, string Phone, string Address, string PaymentMethod)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var carts = await _context.Carts.Include(c => c.Product)
                    .Where(c => c.UserId == userId).ToListAsync();

            if (!carts.Any()) return RedirectToAction("Index", "Cart");

            decimal total = carts.Sum(c => c.Product!.Price * c.Quantity) * 0.90m;

            var order = new Order
            {
                UserId = userId!,
                CustomerName = CustomerName,
                Phone = Phone,
                Address = Address,
                TotalAmount = total,
                PaymentMethod = PaymentMethod,
                OrderDate = DateTime.Now,
                Status = PaymentMethod == "Cash on Delivery" ? "Pending" : "Paid"
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var cart in carts)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cart.ProductId,
                    Quantity = cart.Quantity,
                    Price = cart.Product!.Price
                });
            }

            // PHASE 6: CROSS CUTTING - Email + Logging (YAHAN EMAIL HOGI)
            _logger.LogInformation("Order Placed: {OrderId} by {UserId}", order.Id, userId);
            await _emailService.SendOrderEmail(CustomerName, order.Id);

            _context.Carts.RemoveRange(carts);
            await _context.SaveChangesAsync();

            return RedirectToAction("Success", new { id = order.Id });
        }

        public async Task<IActionResult> Success(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
            if (order == null) return NotFound();

            ViewBag.OrderId = id;
            ViewBag.EmailSent = $"Email sent to {order.CustomerName} for Order #{order.Id} ✓ (Check Output window)";
            return View(order);
        }

        // PHASE 5: BILL PDF
        public async Task<IActionResult> Invoice(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var order = await _context.Orders
              .Include(o => o.OrderItems)
              .ThenInclude(oi => oi.Product)
              .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}