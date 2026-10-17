using Ecommerce.Data;
using Ecommerce.Models;
using Ecommerce.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

namespace Ecommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IMemoryCache cache)
        {
            _logger = logger;
            _context = context;
            _cache = cache;
        }

        // PHASE 6: Caching + Logging + Premium Search
        public async Task<IActionResult> Index(string category, string search)
        {
            _logger.LogInformation("Home page accessed at {Time} - Category: {Cat}, Search: {Search}", DateTime.Now, category, search);

            var query = _context.Products.AsNoTracking().AsQueryable();

            var searchTerm = search?.Trim();
            var categoryTerm = category?.Trim();

            if (!string.IsNullOrWhiteSpace(categoryTerm) && categoryTerm.ToLower() != "all")
            {
                query = query.Where(p => p.Category.ToLower() == categoryTerm.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var s = searchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(s) ||
                    p.Category.ToLower().Contains(s) ||
                    (p.Description != null && p.Description.ToLower().Contains(s))
                );
            }

            query = query.OrderByDescending(p => p.Id);
            var products = await query.ToListAsync();

            // CROSS CUTTING - CACHING: Categories 10 min tak memory me
            var categories = await _cache.GetOrCreateAsync("Categories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                _logger.LogWarning("CACHE MISS - Loading Categories from DB");
                return await _context.Products.AsNoTracking()
                                .Select(p => p.Category)
                                .Distinct()
                                .ToListAsync();
            });

            ViewBag.Categories = categories;
            ViewBag.CurrentCategory = categoryTerm;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Count = products.Count;

            if (products.Count == 0 && !string.IsNullOrWhiteSpace(searchTerm))
            {
                ViewBag.NoResultMessage = $"No results for '{searchTerm}' - Try iPhone, Samsung, Vivo";
            }

            var vm = new HomeViewModel
            {
                Products = products
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.AsNoTracking()
                                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}