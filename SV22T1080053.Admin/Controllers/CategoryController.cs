using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        public const int PAGE_SIZE = 20;
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var Data = await CommonDataService.CategoryDB.ListAsync(page, PAGE_SIZE, searchValue);
            var rowCount = await CommonDataService.CategoryDB.CountAsync(searchValue);
            var model = new PaginationSearchResult<Category>()
            {
                Page = page,
                PageSize = PAGE_SIZE,
                SearchValue = searchValue,
                RowCount = rowCount,
                Data = Data
            };
            return View(model);
        }
        public IActionResult Create()
        {
            @ViewBag.Title = "Thêm loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id = 0)
        {
            @ViewBag.Title = "Chỉnh sửa loại hàng";
            var model = await CommonDataService.CategoryDB.GetAsync(id);
            if (model == null)
            {
                return RedirectToAction("Index");
            }
            return View(model);
        }
        /// <summary>
        /// // Lưu thông tin loại hàng (Thêm mới hoặc Cập nhật)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            ViewBag.Title = data.CategoryID == 0 ? "Bổ sung danh mục mới" : "Cập nhật thông tin danh mục";

            try
            {
                if (string.IsNullOrWhiteSpace(data.CategoryName))
                {
                    ModelState.AddModelError(nameof(data.CategoryName), "Tên danh mục không được để trống");
                }
                if (string.IsNullOrWhiteSpace(data.Description))
                {
                    ModelState.AddModelError(nameof(data.Description), "Tên danh mục không được để trống");
                }
              
                // Thông báo lỗi và yêu cầu nhập lại dữ liệu
                if (!ModelState.IsValid)
                {
                    return View("Edit", data);
                }
            }
            catch (Exception ex)
            {
                return View("Edit", data);
            }
            if (data.CategoryID == 0)
            {
                await CommonDataService.CategoryDB.AddAsync(data);
            }
            else
            {
                await CommonDataService.CategoryDB.UpdateAsync(data);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                // Xóa dữ liệu
                await CommonDataService.CategoryDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                // Hiển thị thông tin để xác nhận việc xóa
                var model = await CommonDataService.CategoryDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }
    }
}
