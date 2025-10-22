using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Data;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }
    }
}
