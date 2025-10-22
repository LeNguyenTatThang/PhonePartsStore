using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Models;
using BCrypt.Net;
using PhonePartsStore.Data; 
using Microsoft.EntityFrameworkCore;

namespace PhonePartsStore.Controllers;

public class SignUpController : Controller
{
    private readonly ApplicationDbContext _context;

    public SignUpController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user, string PasswordHash, string ConfirmPasswordHash)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Index", "SignUp");
        }

        if (PasswordHash != ConfirmPasswordHash)
        {
            ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");
            return RedirectToAction("Index", "SignUp");
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email đã tồn tại");
            return RedirectToAction("Index", "SignUp");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordHash);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "SignIn");
    }

}
