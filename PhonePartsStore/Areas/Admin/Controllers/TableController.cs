using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Data;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TableController(ApplicationDbContext context)
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

            return View();
        }
    }
}
