using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Data; 
using PhonePartsStore.Models;
using PhonePartsStore.Services;
using System.Diagnostics;

namespace PhonePartsStore.Controllers;

public class SignInController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly EmailService _emailService;

    public SignInController(ApplicationDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
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
            TempData["Error"] = "Vui lòng nhập tài khoản và mật khẩu!";
            return RedirectToAction("Index", "SignIn");
        }


        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            TempData["Error"] = "Sai tài khoản hoặc mật khẩu!";
            return RedirectToAction("Index", "SignIn");
        }


        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)

    {
        Console.WriteLine("Email: " + email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            TempData["Error"] = "Email không tồn tại!";
            return RedirectToAction("ForgotPassword");
        }

        var otp = new Random().Next(100000, 999999).ToString();
        Console.WriteLine("OTP: " + otp);
        user.ResetPasswordToken = otp;
        user.ResetTokenExpires = DateTime.UtcNow.AddMinutes(10);

        await _context.SaveChangesAsync();

        await _emailService.SendEmailAsync(email, "Mã xác nhận", $"Mã OTP của bạn: {otp}");

        TempData["Success"] = "OTP đã được gửi!";
        return RedirectToAction("VerifyOTP", new { email });
    }
    [HttpGet("VerifyOTP")]
    public IActionResult VerifyOTP(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("ForgotPassword");
        }

        ViewBag.Email = email;
        return View();
    }
    [HttpPost("VerifyOTP")]
    public async Task<IActionResult> VerifyOTP(string email, string otp)
    {
        Console.WriteLine("Email: " + email);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            TempData["Error"] = "Email không tồn tại!";
            return RedirectToAction("ForgotPassword");
        }

        if (user.ResetPasswordToken != otp || user.ResetTokenExpires < DateTime.UtcNow)
        {
            TempData["Error"] = "Mã OTP không hợp lệ hoặc đã hết hạn!";
            return RedirectToAction("VerifyOTP", new { email });
        }

        TempData["Success"] = "OTP hợp lệ, vui lòng nhập mật khẩu mới.";
        return RedirectToAction("ResetPassword", new { email, otp });
    }
    [HttpGet]
    public IActionResult ResetPassword(string email, string otp)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
        {
            return RedirectToAction("ForgotPassword");
        }
        ViewBag.Email = email;
        ViewBag.OTP = otp;
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPassword model)
    {
        Console.WriteLine(model.Email);
        Console.WriteLine(model.Token);
        Console.WriteLine(model.Password);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            TempData["Error"] = "Email không tồn tại!";
            return RedirectToAction("ForgotPassword");
        }

        if (user.ResetPasswordToken != model.Token || user.ResetTokenExpires < DateTime.UtcNow)
        {
            TempData["Error"] = "Mã OTP không hợp lệ hoặc đã hết hạn!";
            return RedirectToAction("VerifyOTP", new { email = model.Email });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        user.ResetPasswordToken = null;
        user.ResetTokenExpires = null;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Mật khẩu đã được cập nhật!";
        return RedirectToAction("Index", "SignIn");
    }


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "SignIn");
    }
}