using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Models;
using PhonePartsStore.Extensions;
using PhonePartsStore.Data;

namespace PhonePartsStore.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSesionKey = "cart";
    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
        if (cart == null)
        {
            cart = new List<CartItem>();
        }

        return View(cart);
    }

    public IActionResult AddToCart(int id, string name, decimal price, string imageUrl, int quantity)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

        if (cart == null)
        {
            cart = new List<CartItem>();
        }
        var item = cart.FirstOrDefault(c => c.Id == id);

        if (item == null)
        {
            cart.Add(new CartItem
            {
                Id = id,
                Name = name,
                Price = price,
                ImageUrl = imageUrl,
                Quantity = quantity
            });
        }
        else
        {
            item.Quantity++;
        }

        HttpContext.Session.SetObjectAsJson("cart", cart);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult RemoveFromCart(int id)
    {
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

        if (cart == null)
        {
            cart = new List<CartItem>();
        }

        var itemToRemove = cart.FirstOrDefault(c => c.Id == id);
        if (itemToRemove != null)
        {
            cart.Remove(itemToRemove);
        }

        HttpContext.Session.SetObjectAsJson("cart", cart);

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Increase(int id){
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
        var item = cart.FirstOrDefault(c => c.Id == id);
        if(item != null){
            item.Quantity++;
        }
        HttpContext.Session.SetObjectAsJson("cart", cart);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Decrease(int id){
        var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
        var item = cart.FirstOrDefault(c => c.Id == id);
        if(item != null && item.Quantity > 1){
            item.Quantity--;
        }
        HttpContext.Session.SetObjectAsJson("cart", cart);
        return RedirectToAction("Index");
    }
 
}
