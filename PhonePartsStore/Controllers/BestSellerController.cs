using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Models;

namespace PhonePartsStore.Controllers;

public class BestSellerController : Controller
{
    private readonly ILogger<BestSellerController> _logger;

    public BestSellerController(ILogger<BestSellerController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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
