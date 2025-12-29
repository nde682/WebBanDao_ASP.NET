using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BussinessLayers;
using SV22T1080053.Shop.Models;

namespace SV22T1080053.Shop.Controllers
{
    public class ShopController : Controller
    {
        const int pageSize = 9;
        /// <summary>
        /// Trang khởi tạo (Chứa khung giao diện, Sidebar bộ lọc...)
        /// </summary>
        public async Task<IActionResult> Index(int page = 1, string searchValue = "", int categoryID = 0)
        {
            var products = await ProductDataService.ProductDB.ListAsync(page, pageSize, searchValue, categoryID);
            int rowCount = await ProductDataService.ProductDB.CountAsync(searchValue, categoryID);
            var condition = new ShopSearchModel
            {
                Page = page,
                PageSize = pageSize,
                CategoryID = categoryID,
                SearchValue = searchValue,
                MaxPrice = 0,
                MinPrice = 0,
                SortBy = "",
                RowCount = rowCount,
                PageCount = rowCount / pageSize,
                Products = (List<DomainModels.Product>)products
            };
            // 2. Lấy tổng số dòng để tính phân trang
            var categories = await CommonDataService.CategoryDB.ListAsync();
            ViewBag.Categories = categories;

            // Trả về danh sách sản phẩm cho View
            return View(condition);
        }

        /// <summary>
        /// Action xử lý tìm kiếm Ajax, trả về PartialView danh sách sản phẩm
        /// </summary>
        public async Task<IActionResult> Search(ShopSearchModel condition)
        {
            int pageSize = 9; // Số sản phẩm trên 1 trang
            condition.PageSize = pageSize;

            // 1. Đếm số lượng dòng dữ liệu tìm được (để tính phân trang
            condition.RowCount = await ProductDataService.ProductDB.CountAsync(
                condition.SearchValue ?? "",
                condition.CategoryID,
                0, // SupplierID (Shop không lọc theo NCC)
                condition.MinPrice,
                condition.MaxPrice
            );

            // 2. Lấy danh sách sản phẩm theo trang và tiêu chí sắp xếp
            var data = await ProductDataService.ProductDB.ListWithSortAsync(
                condition.Page,
                pageSize,
                condition.SearchValue ?? "",
                condition.CategoryID,
                0, // SupplierID
                condition.MinPrice,
                condition.MaxPrice,
                condition.SortBy ?? "" 
            );

            condition.Products = data.ToList();

            // 3. Tính tổng số trang
            condition.PageCount = condition.RowCount / pageSize;
            if (condition.RowCount % pageSize > 0)
                condition.PageCount++;

            // 4. Trả về Partial View chứa danh sách sản phẩm
            return View("Search", condition);
        }
    }
}