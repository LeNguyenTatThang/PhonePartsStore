using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Data;
using PhonePartsStore.Models;
using System.Diagnostics;

namespace PhonePartsStore.Controllers;

public class ShopController : Controller
{
    private readonly ApplicationDbContext _context;

    public ShopController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }
        
    public IActionResult Detail(int id)
    {
        var product = _context.Products
        .Include(p => p.Category)
        .Include(p => p.Brand)
        .FirstOrDefault(p => p.Id == id);

        if (product == null)
            return NotFound();

        var related = _context.Products
        .Where(p => p.Id != product.Id)
        .OrderBy(x => Guid.NewGuid())
        .Take(4)
        .ToList();

        var viewModel = new ProductDetailViewModel
        {
            Product = product,
            RelatedProducts = related
        };

        return View(viewModel);
    }

}
