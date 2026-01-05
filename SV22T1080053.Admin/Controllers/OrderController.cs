using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BusinessLayers;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using System.Globalization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class OrderController : Controller
    {
        private const string PRODUCT_SEARCH_FOR_SALE = "ProductSearchForSale";
        public const int PRODUCT_PSAGE_SIZE= 5;
        private const int PAGE_SIZE = 20;
        private const string ORDER_SEARCH = "order_search";
        public IActionResult Index()
        {
            // Lấy điều kiện tìm kiếm từ Session (nếu có)
            var input = ApplicationContext.GetSessionData<OrderSearchInput>(ORDER_SEARCH);
            if (input == null)
            {
                DateTime today = DateTime.Today;
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                string defaultRange = $"{firstDayOfMonth:dd/MM/yyyy} - {today:dd/MM/yyyy}";
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    Status = 0,
                    DateRange = defaultRange
                };
            }
            return View(input);
        }
        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            int rowCount = 0;
            var data = Enumerable.Empty<Order>();

            DateTime? fromTime = null;
            DateTime? toTime = null;
            if (!string.IsNullOrWhiteSpace(input.DateRange))
            {
                string[] dates = input.DateRange.Split(" - ");
                bool isParseSuccess = false;
                if (dates.Length == 2)
                {
                    DateTime t1, t2;
                    if (DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out t1) &&
                        DateTime.TryParseExact(dates[1].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out t2))
                    {
                        fromTime = t1;
                        toTime = t2.AddDays(1).AddSeconds(-1);
                        isParseSuccess = true;
                    }
                    if (!isParseSuccess)
                    {
                        DateTime today = DateTime.Today;
                        fromTime = new DateTime(today.Year, today.Month, 1);
                        toTime = today.AddDays(1).AddSeconds(-1);
                        input.DateRange = $"{fromTime:dd/MM/yyyy} - {toTime:dd/MM/yyyy}";
                    }
                }
            }

            data = await OrderDataService.OrderDB.ListAsync(input.Page, input.PageSize, input.Status, fromTime, toTime, input.SearchValue ?? "");
            rowCount = await OrderDataService.OrderDB.CountAsync(input.Status, fromTime, toTime, input.SearchValue ?? "");

            // Lưu lại điều kiện tìm kiếm vào Session
            ApplicationContext.SetSessionData(ORDER_SEARCH, input);

            var model = new OrderSearchResult()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue ?? "",
                Status = input.Status,
                DateRange = input.DateRange ?? "",
                RowCount = rowCount,
                PageCount = (int)Math.Ceiling(1.0 * rowCount / input.PageSize),
                Data = data
            };

            return View("Search", model);
        }


        public async Task<IActionResult> Create()
        {
            ProductSearchCondition condition = ApplicationContext.GetSessionData<ProductSearchCondition>(PRODUCT_SEARCH_FOR_SALE);
            if(condition == null)
            {
                condition = new ProductSearchCondition()
                {
                    Page = 1,
                    PageSize = PRODUCT_PSAGE_SIZE,
                    SearchValue="",
                    CategoryID=0,
                    MaxPrice=0,
                    MinPrice=0,
                    SupplierID = 0
                };
            }
            return View(condition);
        }

        public async Task<IActionResult> SearchProducts(ProductSearchCondition condition)
        {
            if(condition == null)
            {
                return Content("Yêu cầu không hợp lệ");
            }
            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = await ProductDataService.ProductDB.CountAsync(condition.SearchValue,
                                                                         condition.CategoryID,
                                                                         condition.SupplierID,
                                                                         condition.MinPrice,
                                                                         condition.MaxPrice
                                                                         ),
                Data = await ProductDataService.ProductDB.ListAsync(    condition.Page, condition.PageSize,
                                                                        condition.SearchValue,
                                                                         condition.CategoryID,
                                                                         condition.SupplierID,
                                                                         condition.MinPrice,
                                                                         condition.MaxPrice
                                                                         )
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_FOR_SALE, condition);
            return View(model);
        }
        private const string CART = "Cart";
        private List<OrderDetail> GetSessionCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetail>>(CART);
            if(cart == null)
            {
                cart = new List<OrderDetail>();
            }
            return cart;
        }
        private void AddSessionCart(OrderDetail data)
        {
            var cart = GetSessionCart();
            var existOderDetails= cart.Find(m=>m.ProductID == data.ProductID);
            if(existOderDetails == null)
            {
                cart.Add(data);
            }
            else
            {
                existOderDetails.Quantity += data.Quantity;
                existOderDetails.SalePrice = data.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }
        public IActionResult AddToCart(OrderDetail data)
        {
            AddSessionCart(data);
            return View("GetCart",GetSessionCart());
        }

        public IActionResult GetCart()
        {
            return View(GetSessionCart());
        }
        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            var shoppingCart = GetSessionCart();
            int index = shoppingCart.FindIndex(m => m.ProductID == id);
            if (index >= 0)
            {
                shoppingCart.RemoveAt(index);
                //return Json(new ApiResult() { Code = 1 });
            }
            ApplicationContext.SetSessionData(CART, shoppingCart);
            return View("GetCart", GetSessionCart());
        }
        [HttpPost]
        public IActionResult ClearCart()
        {
            var shoppingCart = GetSessionCart();
            shoppingCart.Clear();
            ApplicationContext.SetSessionData(CART, shoppingCart);
            return View("GetCart", GetSessionCart());
        }
        public IActionResult EditDetail(int id,int productid)
        {
            return View();
        }

        // Giao diện Modal chuyển giao hàng (GET)
        [HttpGet]
        public async Task<IActionResult> Shipping(int id) // 1. Thêm async Task
        {
            ViewBag.OrderID = id;
            var shippers = await CommonDataService.ShipperDB.ListAsync(1, 0, "");

            return View(shippers);
        }

        // Xử lý chuyển giao hàng (POST)
        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            if (shipperID <= 0)
            {
                TempData["Error"] = "Vui lòng chọn người giao hàng.";
                return RedirectToAction("Details", new { id = id });
            }

            bool result = await OrderDataService.OrderDB.ShipAsync(id, shipperID);
            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Đã chuyển đơn hàng cho đơn vị vận chuyển.";
            }
            else
            {
                TempData["Error"] = "Không thể chuyển giao hàng (Trạng thái đơn hàng không hợp lệ).";
            }

            return RedirectToAction("Details", new { id = id });
        }
        /// <summary>
        /// Khởi tạo đơn hàng (Lập đơn hàng)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Init(int customerID = 0, string deliveryProvince = "", string deliveryAddress = "")
        {
            var shoppingCart = GetSessionCart();
            if (shoppingCart.Count == 0)
            {
                return Json(new ApiResult { Code = 0, Message = "Giỏ hàng trống, không thể lập đơn hàng." });
            }
            if (customerID <= 0 || string.IsNullOrWhiteSpace(deliveryProvince) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                return Json(new ApiResult { Code = 0, Message = "Vui lòng chọn khách hàng và nhập đầy đủ địa chỉ giao hàng." });
            }
            int employeeID = 0;
            var userData = User.GetUserData();
            if (userData != null)
            {
                int.TryParse(userData.UserId, out employeeID);
            }
            var order = new Order()
            {
                CustomerID = customerID,
                EmployeeID = employeeID,
                OrderTime = DateTime.Now,
                DeliveryProvince = deliveryProvince,
                DeliveryAddress = deliveryAddress,
                Status = 1 
            };
            int orderID = await OrderDataService.OrderDB.AddAsync(order);
            if (orderID > 0)
            {
                foreach(var item in shoppingCart)
                {
                    bool orderDetail = await OrderDataService.OrderDB.SaveDetailAsync(orderID, item.ProductID, item.Quantity, item.SalePrice);
                    if (!orderDetail)
                    {
                       
                        return Json(new ApiResult { Code = 0, Message = "Đã xảy ra lỗi khi lưu chi tiết đơn hàng. Vui lòng thử lại." });
                    }
                }
                HttpContext.Session.Remove(CART);
                return Json(new ApiResult { Code=1 , Message = "Thêm thành công", data=orderID});
            }
            else
            {
                return Json(new ApiResult { Code = 0, Message = "Đã xảy ra lỗi khi khởi tạo đơn hàng. Vui lòng thử lại." });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            var details = await OrderDataService.OrderDB.ListDetailsAsync(id);
            var model = new OrderDetailModel()
            {
                Order = order,
                Details = details
            };

            return View(model);
        }

        // Duyệt đơn hàng
        public async Task<IActionResult> Accept(int id)
        {
            bool result = await OrderDataService.OrderDB.AcceptAsync(id);
            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Đơn hàng đã được duyệt thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể duyệt đơn hàng này (Chỉ duyệt được đơn hàng ở trạng thái 'Mới').";
            }
            return RedirectToAction("Details", new { id = id });
        }

        // Hoàn tất đơn hàng
        public async Task<IActionResult> Finish(int id)
        {
            bool result = await OrderDataService.OrderDB.FinishAsync(id);
            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Ghi nhận đơn hàng đã hoàn tất thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hoàn tất đơn hàng này (Đơn hàng phải đang ở trạng thái 'Đang giao hàng').";
            }
            return RedirectToAction("Details", new { id = id });
        }

        // Hủy đơn hàng
        public async Task<IActionResult> Cancel(int id)
        {
            bool result = await OrderDataService.OrderDB.CancelAsync(id);
            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Đã hủy đơn hàng thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể hủy đơn hàng này (Không thể hủy đơn hàng đã hoàn tất).";
            }
            return RedirectToAction("Details", new { id = id });
        }

        // Từ chối đơn hàng
        public async Task<IActionResult> Reject(int id)
        {
            bool result = await OrderDataService.OrderDB.RejectAsync(id);
            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Đã từ chối đơn hàng.";
            }
            else
            {
                TempData["Error"] = "Không thể từ chối đơn hàng này (Chỉ từ chối được đơn hàng Mới hoặc Đã duyệt).";
            }
            return RedirectToAction("Details", new { id = id });
        }

        // Xóa đơn hàng (chỉ xóa được đơn ở trạng thái vừa khởi tạo, bị hủy hoặc bị từ chối)
        public async Task<IActionResult> Delete(int id)
        {
            bool result = await OrderDataService.OrderDB.DeleteAsync(id);

            if (result)
            {
                int employeeID = 0;
                var userData = User.GetUserData();
                if (userData != null)
                {
                    int.TryParse(userData.UserId, out employeeID);
                }
                bool up = await OrderDataService.OrderDB.UpdateEmployee(employeeID, id);
                TempData["Message"] = "Đã xóa đơn hàng thành công.";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "Không thể xóa đơn hàng này. Chỉ được xóa đơn hàng ở trạng thái: Mới, Đã hủy hoặc Bị từ chối.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // Xóa chi tiết đơn hàng
        public async Task<IActionResult> DeleteDetail(int id, int productId)
        {
            var order = await OrderDataService.OrderDB.GetAsync(id);
            if (order == null) return RedirectToAction("Index");

            // Kiểm tra trạng thái trước khi xóa
            if (order.Status == Constants.ORDER_INIT || order.Status == Constants.ORDER_ACCEPTED)
            {
                var result = await OrderDataService.OrderDB.DeleteDetailAsync(id, productId);
                if (result)
                {
                    TempData["Message"] = "Đã xóa mặt hàng khỏi đơn hàng.";
                }
                else
                {
                    TempData["Error"] = "Không xóa được mặt hàng này.";
                }
            }
            else
            {
                TempData["Error"] = "Đơn hàng đang xử lý hoặc đã hoàn tất, không được phép xóa chi tiết.";
            }

            return RedirectToAction("Details", new { id = id });
        }
        [HttpGet]
        public async Task<IActionResult> UpdateDetail(int id, int productId)
        {
           
            if (id <= 0 || productId <= 0)
            {
                return RedirectToAction("Details", new { id = id });
            }

            var model = await OrderDataService.OrderDB.GetDetailAsync(id, productId);

            if (model == null)
            {
                TempData["Error"] = "Dữ liệu không tồn tại (Có thể ID sản phẩm bị sai).";
                return RedirectToAction("Details", new { id = id });
            }
            return View("EditDetail",model);
        }
        // Xử lý cập nhật chi tiết (POST)
        [HttpPost]
        public async Task<IActionResult> UpdateDetail(int id, int productID, int quantity, decimal salePrice)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng phải lớn hơn 0.";
                return RedirectToAction("Details", new { id = id });
            }
            if (salePrice < 0)
            {
                TempData["Error"] = "Giá bán không được âm.";
                return RedirectToAction("Details", new { id = id });
            }

            // Gọi hàm SaveDetailAsync để cập nhật
            bool result = await OrderDataService.OrderDB.SaveDetailAsync(id, productID, quantity, salePrice);

            if (result)
            {
                TempData["Message"] = "Đã cập nhật chi tiết đơn hàng.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật.";
            }

            return RedirectToAction("Details", new { id = id });
        }

    }
}
