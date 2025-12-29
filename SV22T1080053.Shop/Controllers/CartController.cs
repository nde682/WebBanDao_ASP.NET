using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SV22T1080053.BusinessLayers;
using SV22T1080053.BussinessLayers;
using SV22T1080053.Shop.AppCodes;
using SV22T1080053.Shop.Models;

namespace SV22T1080053.Shop.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private const string SHOPPING_CART_SESSION_KEY = "SHOPPING_CART";

        // 1. Lấy danh sách giỏ hàng từ Session
        private List<CartItem> GetShoppingCart()
        {
            var sessionData = ApplicationContext.GetSessionData<List<CartItem>>(SHOPPING_CART_SESSION_KEY);
            if(sessionData == null)
            {
                return new List<CartItem>();
            }
            return sessionData;
        }

        // 2. Lưu danh sách giỏ hàng vào Session
        private void SetShoppingCart(List<CartItem> cart)
        {
            ApplicationContext.SetSessionData(SHOPPING_CART_SESSION_KEY, cart);
        }

        // ACTION: Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetShoppingCart();
            return View(cart);
        }

        // ACTION: Thêm vào giỏ hàng (Gọi bằng Ajax từ trang Detail/Index)
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            try
            {
                var cart = GetShoppingCart();
                var item = cart.FirstOrDefault(x => x.ProductID == id);
                if (quantity < 0) quantity = 1;
                if (item != null)
                {
                    // Nếu đã có -> Tăng số lượng
                    item.Quantity += quantity;
                }
                else
                {
                    // Nếu chưa có -> Lấy thông tin từ DB và thêm mới
                    var product = await ProductDataService.ProductDB.GetAsync(id);
                    if (product == null)
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                    }

                    item = new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo,
                        Unit = product.Unit,
                        Price = product.Price,
                        Quantity = quantity
                    };
                    cart.Add(item);
                }

                SetShoppingCart(cart);

                // Trả về tổng số lượng để cập nhật icon giỏ hàng
                return Json(new { success = true, message = "", cartCount = cart.Sum(x => x.Quantity) });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ACTION: Xóa 1 sản phẩm khỏi giỏ
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                SetShoppingCart(cart);
            }
            return RedirectToAction("Index");
        }

        // ACTION: Xóa tất cả
        public IActionResult ClearCart()
        {
            var cart = new List<CartItem>();
            SetShoppingCart(cart);
            return RedirectToAction("Index");
        }

        // ACTION: Cập nhật số lượng (Dùng Ajax khi bấm nút +/- trong giỏ)
        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = GetShoppingCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item != null)
            {
                item.Quantity = quantity;

                // Nếu số lượng <= 0, xóa sản phẩm và báo hiệu cho Client biết để reload trang hoặc xóa dòng
                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                    SetShoppingCart(cart);
                    // Trả về tín hiệu yêu cầu reload trang vì đã xóa dòng
                    return Json(new { success = true, isRemoved = true });
                }

                SetShoppingCart(cart);

                // Trả về dữ liệu để update giao diện
                return Json(new
                {
                    success = true,
                    isRemoved = false,
                    newItemTotal = item.TotalPrice, // Thành tiền mới của sản phẩm này
                    newGrandTotal = cart.Sum(x => x.TotalPrice) // Tổng tiền mới của cả giỏ
                });
            }

            return Json(new { success = false, message = "Sản phẩm không tìm thấy" });
        }
        [HttpGet]
        public IActionResult GetCartInfo()
        {
            var cart = GetShoppingCart();
            return Json(new
            {
                count = cart.Sum(x => x.Quantity),
                total = cart.Sum(x => x.TotalPrice)
            });
        }
    }
}