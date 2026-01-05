using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BussinessLayers;
using SV22T1080053.Shop.Models;

namespace SV22T1080053.Shop.Controllers
{
    public class ShopController : Controller
    {
        const int PAGE_SIZE = 9;
        const string SHOP_SEARCH_CONDITION = "ShopSearchCondition";

        /// <summary>
        /// Trang khởi tạo (Chứa khung giao diện, Sidebar bộ lọc...)
        /// Nhận tham số từ Header (searchValue) hoặc Navbar (categoryID)
        /// </summary>
        public async Task<IActionResult> Index(int? page = 1,string? searchValue = null, int? categoryID = null)
        {
            var condition = ApplicationContext.GetSessionData<ShopSearchModel>(SHOP_SEARCH_CONDITION);

            if (condition == null)
            {
                condition = new ShopSearchModel
                {
                    Page = 1,
                    PageSize = PAGE_SIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    MinPrice = 0,
                    MaxPrice = 0,
                    SortBy = ""
                };
            }

            if (categoryID.HasValue) // Trường hợp bấm Category trên Navbar
            {
                condition.CategoryID = categoryID.Value;
                condition.SearchValue = ""; // Reset tìm kiếm tên
                condition.Page = 1;         // Về trang 1
                condition.MinPrice = 0;     // Reset bộ lọc giá (tuỳ chọn)
                condition.MaxPrice = 0;
            }

            if (!string.IsNullOrEmpty(searchValue)) // Trường hợp tìm kiếm từ Header
            {
                condition.SearchValue = searchValue;
                condition.CategoryID = 0;   // Reset danh mục về tất cả
                condition.Page = 1;         // Về trang 1
                condition.MinPrice = 0;     // Reset bộ lọc giá (tuỳ chọn)
                condition.MaxPrice = 0;
            }

            ApplicationContext.SetSessionData(SHOP_SEARCH_CONDITION, condition);

            var categories = await CommonDataService.CategoryDB.ListAsync();
            ViewBag.Categories = categories;

            return await Search(condition);
            
        }

        /// <summary>
        /// Action xử lý tìm kiếm Ajax, trả về PartialView danh sách sản phẩm
        /// </summary>
        public async Task<IActionResult> Search(ShopSearchModel input)
        {
            // 1. Lấy Session cũ ra
            var condition = ApplicationContext.GetSessionData<ShopSearchModel>(SHOP_SEARCH_CONDITION);
            if (condition == null)
            {
                // Phòng trường hợp session hết hạn giữa chừng
                condition = new ShopSearchModel { PageSize = PAGE_SIZE };
            }

            // 2. Cập nhật các thay đổi từ Input vào Condition của Session
            // Chỉ cập nhật những gì client gửi lên (nếu input không null)
            if (input != null)
            {
                // Chỉ nhận các thay đổi logic filter, giữ nguyên các cái khác
                condition.Page = input.Page > 0 ? input.Page : 1;

                condition.SearchValue = input.SearchValue ?? "";

                condition.CategoryID = input.CategoryID;
                condition.SortBy = input.SortBy ?? "";
                condition.MinPrice = input.MinPrice;
                condition.MaxPrice = input.MaxPrice;
            }

            // 3. Lưu ngược lại Session
            ApplicationContext.SetSessionData(SHOP_SEARCH_CONDITION, condition);

            // 4. Thực hiện truy vấn dữ liệu
            int rowCount = await ProductDataService.ProductDB.Count2Async(
                condition.SearchValue ?? "",
                condition.CategoryID,
                0, // SupplierID
                condition.MinPrice,
                condition.MaxPrice
            );

            var data = await ProductDataService.ProductDB.ListWithSortAsync(
                condition.Page,
                PAGE_SIZE,
                condition.SearchValue ?? "",
                condition.CategoryID,
                0, // SupplierID
                condition.MinPrice,
                condition.MaxPrice,
                condition.SortBy ?? ""
            );

            // 5. Gán kết quả vào Model
            condition.RowCount = rowCount;
            condition.Products = data.ToList();

            condition.PageCount = condition.RowCount / PAGE_SIZE;
            if (condition.RowCount % PAGE_SIZE > 0)
                condition.PageCount++;

            // 6. Trả về
            // Kiểm tra xem Request này là Ajax hay là gọi trực tiếp từ Index
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Search", condition); // Trả về Partial cho Ajax
            }
            else
            {
                return View("Index", condition); // Trả về Full View cho lần load đầu tiên
            }
        }
    }
}