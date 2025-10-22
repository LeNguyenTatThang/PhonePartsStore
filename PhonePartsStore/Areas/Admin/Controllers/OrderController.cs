using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhonePartsStore.Data;

namespace PhonePartsStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var orders = _dbContext.Orders.ToList();
            return View(orders);
        }

        [HttpGet("Detail")]
        public async Task<IActionResult> Detail(string orderId)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var order = await _dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = $"Không tìm thấy đơn hàng với mã {orderId}." });
            }

            return View(order);
        }


        [HttpPost("UpdateStatus")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var order = _dbContext.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = status;
            _dbContext.SaveChanges();

            TempData["Message"] = "Trạng thái đơn hàng đã được cập nhật.";
            return RedirectToAction("Index");
        }

        [HttpPost("Delete")]
        public IActionResult Delete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin" && role != "Manager")
            {
                return RedirectToAction("Index", "Login");
            }

            var order = _dbContext.Orders.Find(id);

            _dbContext.Orders.Remove(order);
            _dbContext.SaveChanges();

            TempData["Message"] = "Đã xóa đơn hàng.";
            return RedirectToAction("Index");
        }

    }
}
