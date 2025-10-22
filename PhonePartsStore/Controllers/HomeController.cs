using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Models;
using PhonePartsStore.Data; 
using Microsoft.EntityFrameworkCore;

namespace PhonePartsStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var allProducts = _context.Products.Take(8).Include(p => p.Category).Include(p => p.Brand).ToList();

            var newProducts = _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            var featuredProducts = _context.Products
                .Take(4)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            var bestSellers = _context.Products
                .Take(4)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();

            var viewModel = new HomeViewModel
            {
                AllProducts = allProducts,
                NewProducts = newProducts,
                FeaturedProducts = featuredProducts,
                BestSellers = bestSellers
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}