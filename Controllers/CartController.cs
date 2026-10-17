using Ecommerce.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Ecommerce.Controllers
{
    [Authorize] // Pura Cart login ke baad hi khulega - Perfect Security
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account", new { area = "Identity" });

            var carts = await _context.Carts.Include(c => c.Product)
                        .Where(c => c.UserId == userId).ToListAsync();

            decimal subtotal = carts.Sum(c => (c.Product!.Price * c.Quantity));
            ViewBag.Subtotal = subtotal;
            ViewBag.Discount = subtotal * 0.10m;
            ViewBag.Tax = 0;
            ViewBag.FinalTotal = subtotal - (subtotal * 0.10m);

            return View(carts);
        }

        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge(); // Warning Fix + Security

            var existing = await _context.Carts.FirstOrDefaultAsync(c => c.ProductId == productId && c.UserId == userId);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _context.Carts.Add(new Models.Cart
                {
                    ProductId = productId,
                    UserId = userId!, // FIX:! lagane se warning khatam
                    Quantity = quantity
                });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var item = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (item != null) { item.Quantity++; await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Decrease(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var item = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (item != null)
            {
                if (item.Quantity > 1) item.Quantity--;
                else _context.Carts.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var item = await _context.Carts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            if (item != null) { _context.Carts.Remove(item); await _context.SaveChangesAsync(); }
            return RedirectToAction("Index");
        }
    }
}