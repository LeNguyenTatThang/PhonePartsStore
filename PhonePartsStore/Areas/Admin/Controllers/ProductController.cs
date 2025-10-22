using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Data;
using PhonePartsStore.Models;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
                var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToList();
            
            return View(products);
        }
         [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories.Where(c => c.IsActive), "Id", "Name");
            ViewBag.Brands = new SelectList(_context.Brands.Where(c => c.IsActive), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                ViewBag.Brands = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
                return View(product);
            }

             if (ImageFile != null && ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(ImageFile.FileName)}";
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/images/" + fileName;
            }

            _context.Update(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
            ViewBag.Brands = _context.Brands.ToList();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? ImageFile)
        {
            Console.WriteLine($">>> POST Edit called for Product ID: {product.Id}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine(">>> Model invalid:");
                foreach (var kvp in ModelState)
                {
                    if (kvp.Value.Errors.Count > 0)
                    {
                        foreach (var err in kvp.Value.Errors)
                        {
                            Console.WriteLine($"- {kvp.Key}: {err.ErrorMessage}");
                        }
                    }
                }

                ViewBag.Categories = _context.Categories.Where(c => c.IsActive).ToList();
                ViewBag.Brands = _context.Brands.ToList();
                return View(product);
            }

            var existingProduct = _context.Products.Find(product.Id);
            if (existingProduct == null)
            {
                Console.WriteLine(">>> Product not found");
                return NotFound();
            }

            Console.WriteLine(">>> Updating product basic info");
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.BrandId = product.BrandId;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                Console.WriteLine(">>> Image file uploaded");

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldFilePath = Path.Combine(uploadsFolder, Path.GetFileName(existingProduct.ImageUrl));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                        Console.WriteLine($">>> Old image deleted: {existingProduct.ImageUrl}");
                    }
                }

                var newFileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, newFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                    Console.WriteLine($">>> New image saved: {newFileName}");
                }

                existingProduct.ImageUrl = "/images/products/" + newFileName;
            }
            else
            {
                Console.WriteLine(">>> No new image uploaded");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($">>> Product updated successfully: {existingProduct.Name}, ID: {existingProduct.Id}");

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null){
                return NotFound();
            }
             if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var filePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
