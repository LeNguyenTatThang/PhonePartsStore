using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Data;
using BCrypt.Net;
using PhonePartsStore.Models;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var users = _context.Users.Where(u => u.Role == "User").ToList();
            return View(users);
        }

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var user = _context.Users.FirstOrDefault(c => c.Id == id);
            if(user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var existingUser = _context.Users.Find(user.Id);
            if (existingUser == null) return NotFound();

            if(user.PasswordHash != user.ConfirmPasswordHash) 
            {
                ModelState.AddModelError("ConfirmPasswordHash", "Mật khẩu không khớp");
                return View(user);
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.Address = user.Address;
            existingUser.Role = user.Role;

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                if (user.PasswordHash != user.ConfirmPasswordHash)
                {
                    ModelState.AddModelError("ConfirmPasswordHash", "Mật khẩu xác nhận không khớp.");
                    return View(user);
                }

                if (user.PasswordHash.Length < 6)
                {
                    ModelState.AddModelError("PasswordHash", "Mật khẩu phải có ít nhất 6 ký tự.");
                    return View(user);
                }

                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            _context.Update(existingUser);
            _context.SaveChanges();

            TempData["success"] = "Cập nhật tài khoản thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
       public IActionResult Create(User user)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            if (user.PasswordHash != user.ConfirmPasswordHash) 
            {
                ModelState.AddModelError("ConfirmPasswordHash", "Mật khẩu không khớp");
                return View(user);
            }

            if(user.PasswordHash == null || user.PasswordHash.Length < 6 || user.PasswordHash.Length > 100)
            {
                ModelState.AddModelError("PasswordHash", "Mật khẩu phải có ít nhất 6 và dưới 100 ký tự");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                var existingEmail = _context.Users.FirstOrDefault(u => u.Email == user.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(user);
                }

                

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["success"] = "Thêm tài khoản quản lý thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
