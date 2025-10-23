using Microsoft.AspNetCore.Mvc;
using PhonePartsStore.Data;
using PhonePartsStore.Extensions;
using PhonePartsStore.Models;
using PhonePartsStore.Models.Vnpay;
using PhonePartsStore.Services.Momo;
using PhonePartsStore.Services.Vnpay;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhonePartsStore.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        public CheckOutController(ApplicationDbContext context, IMomoService momoService, IVnPayService vnPayService)
        {
            _context = context;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart") ?? new List<CartItem>();

            decimal subtotal = cart.Sum(i => i.Price * i.Quantity);

            ViewBag.CartItems = cart;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = 30000; 
            ViewBag.Total = subtotal + ViewBag.ShippingFee;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderInfo model, PaymentInformationModel model1, string FullName, string address, string phone, decimal Amount, string paymentMethod)
        {
            HttpContext.Session.SetString("CustomerName", FullName);
            HttpContext.Session.SetString("Address", address);
            HttpContext.Session.SetString("Phone", phone);
            HttpContext.Session.SetDecimal("TotalAmount", Amount);

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Vui lòng chọn phương thức thanh toán.";
                return RedirectToAction("Index", "Cart");
            }
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }
            var productIds = cart.Select(c => c.Id).ToList();
            var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();

            foreach (var cartItem in cart)
            {
                var product = products.FirstOrDefault(p => p.Id == cartItem.Id);
                if (product != null && cartItem.Quantity > product.StockQuantity )
                {
                    TempData["Error"] = $"Sản phẩm {product.Name} chỉ còn {product.StockQuantity } sản phẩm trong kho.";
                    return RedirectToAction("Index", "Cart");
                }
            }
            switch (paymentMethod)
            {
                case "momo":
                    var response = await _momoService.CreatePaymentAsync(model);
                    return Redirect(response.PayUrl);

                case "vnpay":
                    var url = _vnPayService.CreatePaymentUrl(model1, HttpContext);
                    return Redirect(url);

                case "cod":
                    var order = new Order
                    {
                        OrderId = DateTime.Now.ToString("yyyyMMddHHmmss"),
                        CustomerName = FullName,
                        Address = address,
                        Phone = phone,
                        TotalAmount = Amount,
                        PaymentMethod = "COD",
                        PaymentStatus = "Success",
                        OrderStatus = "Processing",
                        OrderDate = DateTime.Now
                    };
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    if (cart == null || !cart.Any())
                    {
                        throw new ArgumentException("Cart không hợp lệ!");
                    }
                    var orderDetails = cart.Select(item => new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Id,
                        ProductName = item.Name,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Quantity * item.Price
                    }).ToList();
                    _context.OrderDetails.AddRange(orderDetails);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.Remove("cart");

                    ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Apple Store.";
                    TempData["PaymentSuccess"] = "Thanh toán thành công!";
                    return Redirect("/ThankYou.html");

                default:
                    TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
                    return RedirectToAction("Index", "Cart");
            }

        }

        private List<OrderDetail> SaveOrderDetails(List<CartItem> cart, string orderId)
        {
            if (cart == null || !cart.Any())
            {
                throw new ArgumentException("Cart không hợp lệ!");
            }

            var orderDetails = cart.Select(item => new OrderDetail
            {
                OrderId = orderId,
                ProductId = item.Id,
                ProductName = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                Total = item.Quantity * item.Price
            }).ToList();


            _context.OrderDetails.AddRange(orderDetails);
            _context.SaveChanges();

            HttpContext.Session.Remove("cart");
            TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            return orderDetails;
        }

        [Route("CheckOut/PaymentCallbackVnpay")]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            Console.WriteLine("VnPay start");
            var response = _vnPayService.PaymentExecute(Request.Query);
            Console.WriteLine("data:", response);
            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");
            if (response.VnPayResponseCode == "00")
            {

                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = Request.Query["vnp_TxnRef"].ToString(),
                    PaymentStatus = "Success",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(checkOrder);
                await _context.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);

                foreach (var item in cart)
                {
                    var product = _context.Products.FirstOrDefault(p => p.Id == item.Id);
                    if (product != null && product.StockQuantity  >= item.Quantity)
                    {
                        product.StockQuantity  -= item.Quantity;
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = response.PaymentMethod,
                    OrderId = response.OrderId,
                    PaymentStatus = "Fail",
                    OrderStatus = "Processing",
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(checkOrder);
                await _context.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
            }

            return Redirect("/ThankYou.html");


        }

        [HttpGet]
        [Route("CheckOut/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack()
        {
            var response = await _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;

            var CustomerName = HttpContext.Session.GetString("CustomerName");
            var Address = HttpContext.Session.GetString("Address");
            var Phone = HttpContext.Session.GetString("Phone");
            var TotalAmount = HttpContext.Session.GetDecimal("TotalAmount");

            if (response.IsSuccess)
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Success",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(checkOrder);
                await _context.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store.";
                TempData["Success"] = "Thanh toán thành công. Cảm ơn bạn đã mua hàng tại Shoe Store!";
            }
            else
            {
                var checkOrder = new Order
                {
                    CustomerName = CustomerName,
                    Address = Address,
                    Phone = Phone,
                    TotalAmount = TotalAmount,
                    PaymentMethod = "Momo",
                    OrderId = requestQuery["orderId"],
                    PaymentStatus = "Fail",
                    OrderStatus = "Procesing",
                    OrderDate = DateTime.Now
                };
                _context.Orders.Add(checkOrder);
                await _context.SaveChangesAsync();
                var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("cart");

                if (cart == null || !cart.Any())
                {
                    ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Home");
                }

                SaveOrderDetails(cart, checkOrder.OrderId);
                ViewBag.Message = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
                TempData["Error"] = "Thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.!";
            }
            return Redirect("/ThankYou.html");
        }

    }
}