using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Models;
using BCrypt.Net;
using PhonePartsStore.Data; 
using Microsoft.EntityFrameworkCore;

namespace PhonePartsStore.Controllers;

public class SignInController : Controller
{
    private readonly ApplicationDbContext _context;

    public SignInController(ApplicationDbContext context)
    {
        _context = context;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        Console.WriteLine($"Received email: {email}");
        Console.WriteLine($"Received password: {password}");

        var user = await _context.Users
                                    .Where(a => a.Email == email)
                                    .Select(a => new
                                    {
                                        a.Id,
                                        a.Email,
                                        a.Address,
                                        a.FullName,
                                        a.Role,
                                        a.PasswordHash
                                    })
                                    .SingleOrDefaultAsync();

        if (user == null)
        {
            Console.WriteLine("User not found.");
            TempData["Error"] = "Vui lòng nhập tài khoản và mật khẩu!";
            return RedirectToAction("Index", "SignIn");
        }

        Console.WriteLine($"Found user: {user.FullName}, PasswordHash: {user.PasswordHash}");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            Console.WriteLine("Password verification failed.");
            TempData["Error"] = "Sai tài khoản hoặc mật khẩu!";
            return RedirectToAction("Index", "SignIn");
        }

        Console.WriteLine("Password verified successfully. Logging in...");

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);

        return RedirectToAction("Index", "Home");
    }

    
}