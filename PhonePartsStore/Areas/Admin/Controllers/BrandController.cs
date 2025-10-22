using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Data;
using PhonePartsStore.Models;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _context;
        public BrandController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var brands = _context.Brands.ToList();
            return View(brands);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ToggleStatusAdmin(int id)
        {
            var brand = _context.Brands.FirstOrDefault(b => b.Id == id);
            if (brand == null)
                return NotFound();

            brand.IsActive = !brand.IsActive;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var brand = _context.Brands.FirstOrDefault(c => c.Id == id);
            if (brand == null)
                return NotFound();

            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand)
        {
            if(!ModelState.IsValid){
                return View(brand);
            }
            _context.Brands.Add(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

         [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                return View(brand);
            }

            var existing = await _context.Brands.FindAsync(brand.Id);
            if (existing == null)
                return NotFound();

            existing.Name = brand.Name;
            existing.Description = brand.Description;
            existing.IsActive = brand.IsActive;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if( id == 0)
            {
                return NotFound();
            }
            var brand = _context.Brands.Find(id);
            if(brand == null)
            {
                return NotFound();
            }
            
            bool hasRelatedProducts = _context.Products.Any(p => p.BrandId == id);
            if(hasRelatedProducts)
            {
                return BadRequest();
            }
            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
