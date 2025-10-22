using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Data;
using BCrypt.Net;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            Console.WriteLine(email);

            var user = await _context.Users
        .Where(a => a.Email == email)
        .Select(a => new
        {
            a.Id,
            a.Email,
            a.PasswordHash,
            a.FullName,
            a.Role,
            a.Phone,
            a.Address
        })
        .SingleOrDefaultAsync();
            if (user != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    TempData["error"] = "Invalid login attempt. Incorrect password.";
                    return View();
                }

                HttpContext.Session.SetString("Name", user.FullName);
                HttpContext.Session.SetString("UserRole", user.Role);

                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (user.Role == "User")
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View("Index");
            }

            return View("Index");
        }
        [HttpGet("admin/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Name");
            HttpContext.Session.Remove("UserRole");
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
