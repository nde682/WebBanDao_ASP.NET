using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BusinessLayers;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using SV22T1080053.Shop.AppCodes;
using SV22T1080053.Shop.Models;
namespace SV22T1080053.Shop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private const string SHOPPING_CART_SESSION_KEY = "SHOPPING_CART";

        private List<CartItem> GetShoppingCart()
        {
            var sessionData = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART_SESSION_KEY);
            if (sessionData == null)
                return new List<CartItem>();
            return sessionData;
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(SHOPPING_CART_SESSION_KEY);
        }

        // 1. TRANG CHECKOUT (Điền thông tin)
        public async Task<IActionResult> Checkout()
        {
            var cart = GetShoppingCart();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            var userData = User.GetUserData();
            int customerId = 0;
            int.TryParse(userData.UserId, out customerId);

            var customer = await CommonDataService.CustomerDB.GetAsync(customerId);

            if (customer == null)
            {
                customer = new Customer();
            }

            ViewBag.Cart = cart;

            // 3. Truyền model Customer sang View thay vì UserAccount
            return View(customer);
        }
        [HttpGet]
        public async Task<IActionResult> GetCustomerInfo()
        {
            // 1. Lấy UserID từ cookie đăng nhập
            var userData = User.GetUserData();
            if (userData == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            int customerId = int.Parse(userData.UserId);

            // 2. Lấy thông tin từ DB
            var customer = await CommonDataService.CustomerDB.GetAsync(customerId);

            if (customer != null)
            {
                return Json(new
                {
                    success = true,
                    address = customer.Address,
                    province = customer.Province,
                    name = customer.CustomerName
                });
            }
            return Json(new { success = false, message = "Không tìm thấy thông tin" });
        }

        // 2. XỬ LÝ ĐẶT HÀNG (Action Init)
        [HttpPost]
        public async Task<IActionResult> Init(string deliveryProvince, string deliveryAddress)
        {
            var cart = GetShoppingCart();
            if (cart.Count == 0) return RedirectToAction("Index", "Cart");

            var user = User.GetUserData();
            int customerID = int.Parse(user.UserId);

            // Tạo danh sách chi tiết
            var orderDetails = cart.Select(item => new OrderDetail
            {
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                SalePrice = item.Price
            }).ToList();
            var order = new Order()
            {
                CustomerID = customerID,
                OrderTime = DateTime.Now,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                Status = Constants.ORDER_INIT,
            };

            // Gọi Service tạo đơn
            int orderID = await OrderDataService.OrderDB.CustomerAddAsync(order);

            if (orderID > 0)
            {
                var Cart = GetShoppingCart();
                if(Cart.Count != 0)
                {
                    foreach (var item in cart)
                    {
                        bool save = await OrderDataService.OrderDB.SaveDetailAsync(orderID, item.ProductID, item.Quantity, item.Price);

                    }
                }
                ClearCart();
                return RedirectToAction("Details", new { id = orderID });
            }
            else
            {
                TempData["Message"] = "Hệ thống đang bận, vui lòng thử lại sau.";
                return RedirectToAction("Checkout");
            }
        }

        // 3. LỊCH SỬ ĐƠN HÀNG
        public async Task<IActionResult> History()
        {
            var user = User.GetUserData();
            int customerID = int.Parse(user.UserId);

            var orders = await OrderDataService.OrderDB.GetByCustomerAsync(customerID);
            return View(orders);
        }

        // 4. CHI TIẾT ĐƠN HÀNG
        public async Task<IActionResult> Details(int id)
        {
            var user = User.GetUserData();
            int customerID = int.Parse(user.UserId);

            var order = await OrderDataService.OrderDB.GetAsync(id);

            if (order == null || order.CustomerID != customerID)
            {
                return RedirectToAction("History");
            }
            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View(order);
        }
        // 1. XỬ LÝ HỦY ĐƠN HÀNG
        [HttpPost] // Thêm cái này để bảo mật, chỉ chấp nhận request POST
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var user = User.GetUserData();
                int customerID = int.Parse(user.UserId);

                var order = await OrderDataService.OrderDB.GetAsync(id);

                // 1. Kiểm tra quyền sở hữu
                if (order == null || order.CustomerID != customerID)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thao tác trên đơn hàng này." });
                }

                // 2. Kiểm tra trạng thái
                if (order.Status == Constants.ORDER_INIT || order.Status == Constants.ORDER_ACCEPTED)
                {
                    await OrderDataService.OrderDB.CancelAsync(id);
                    return Json(new { success = true, message = "Đã hủy đơn hàng thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Đơn hàng đang vận chuyển hoặc đã hoàn tất, không thể hủy!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        [HttpPost] // Thêm cái này
        public async Task<IActionResult> ConfirmFinished(int id)
        {
            try
            {
                var user = User.GetUserData();
                int customerID = int.Parse(user.UserId);

                var order = await OrderDataService.OrderDB.GetAsync(id);

                // 1. Kiểm tra quyền sở hữu
                if (order == null || order.CustomerID != customerID)
                {
                    return Json(new { success = false, message = "Bạn không có quyền thao tác trên đơn hàng này." });
                }

                // 2. Kiểm tra trạng thái
                if (order.Status == Constants.ORDER_SHIPPING)
                {
                    await OrderDataService.OrderDB.FinishAsync(id);
                    return Json(new { success = true, message = "Cảm ơn bạn đã xác nhận nhận hàng!" });
                }
                else
                {
                    return Json(new { success = false, message = "Trạng thái đơn hàng không hợp lệ để xác nhận." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}