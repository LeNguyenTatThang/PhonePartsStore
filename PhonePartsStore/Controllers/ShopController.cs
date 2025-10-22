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

    public IActionResult Index(int? brand, int? category, string keyword, string sort = "", int page = 1, int pageSize = 9)
    {
        ViewData["ActivePage"] = "Product";
        var categories = _context.Categories
            .Where(c => c.IsActive == true)
             .Select(c => new
             {
                 c.Id,
                 c.Name,
                 ProductCount = _context.Products.Count(p => p.CategoryId == c.Id)
             })
             .Where(c => c.ProductCount > 0)
            .ToList();

        var brands = _context.Brands
            .Where(b => b.IsActive == true)
            .Select(c => new
            {
                c.Id,
                c.Name,
                ProductCount = _context.Products.Count(p => p.BrandId == c.Id)
            })
            .Where(b => b.ProductCount > 0)
            .ToList();

        ViewBag.Category = categories;
        ViewBag.Brand = brands;

        var products = _context.Products
            .Include(l => l.Category)
            .Include(l => l.Brand)
            .AsQueryable();
        if (category.HasValue)
        {
            products = products.Where(p => p.CategoryId == category.Value);
        }

        if (brand.HasValue)
        {
            products = products.Where(p => p.BrandId == brand.Value);
        }


        if (!string.IsNullOrEmpty(keyword))
        {
            products = products.Where(p => p.Name.Contains(keyword));
        }

        if (!string.IsNullOrEmpty(sort))
        {
            if (sort == "price-asc")
            {
                products = products.OrderBy(p => p.Price);
            }
            else if (sort == "price-desc")
            {
                products = products.OrderByDescending(p => p.Price);
            }
        }


        int totalItems = products.Count();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        products = products.Skip((page - 1) * pageSize).Take(pageSize);
        ViewBag.SelectedBrand = brand;
        ViewBag.SelectedCategory = category;
        ViewBag.Sort = sort;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        return View(products.ToList());
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
