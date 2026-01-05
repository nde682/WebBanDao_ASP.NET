using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private const int PAGESIZE = 20;
        private const string SUPPLIER_SEARCH_CONDITION = "SupplierSearchCondition";
        /// <summary>
        /// Danh sách nhà cung cấp
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {
            var condition = ApplicationContext.GetSessionData<paginationSearchCondition>(SUPPLIER_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new paginationSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = ""
                };
            }
            return View(condition);
        }
        public async Task<IActionResult> Search(paginationSearchCondition condition)
        {
            var data = await CommonDataService.SupplierDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.SupplierDB.CountAsync(condition.SearchValue);
            var model = new Models.PaginationSearchResult<DomainModels.Supplier>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(SUPPLIER_SEARCH_CONDITION, condition);
            return View(model);
        }
        /// <summary>
        /// Thêm nhà cung cấp
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.title = "Bổ sung nhà cung cấp mới";
            var model = new Supplier
            {
                SupplierID = 0
            };
            return View("Edit",model);
        }
        /// <summary>
        /// Cập nhật nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.title = "Cập nhật thông tin nhà cung cấp";
            var model = await CommonDataService.SupplierDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// Xoá nhà cung cấp
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id = 0)
        {
            if(Request.Method == "POST")
            {
                await CommonDataService.SupplierDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.SupplierDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }
        /// <summary>
        /// Lưu dữ liệu khách hàng
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Supplier Data)
        {
            ViewBag.Title = Data.SupplierID == 0 ? "Bổ sung khách hàng mới" : "Cập nhật thông tin khách hàng";

            try
            {
                if (string.IsNullOrWhiteSpace(Data.SupplierName))
                {
                    ModelState.AddModelError(nameof(Data.SupplierName), "Tên khách hàng không được để trống");
                }
                if (string.IsNullOrWhiteSpace(Data.ContactName))
                {
                    ModelState.AddModelError(nameof(Data.ContactName), "Tên liên lạc không được để trống");
                }
                if (string.IsNullOrWhiteSpace(Data.Phone))
                {
                    ModelState.AddModelError(nameof(Data.Phone), "Số điện thoại không được để trống");
                }
                if (string.IsNullOrWhiteSpace(Data.Email))
                {
                    ModelState.AddModelError(nameof(Data.Email), "Email không được để trống");
                }
                if (string.IsNullOrWhiteSpace(Data.Address))
                {
                    ModelState.AddModelError(nameof(Data.Address), "Địa chỉ không được để trống");
                }
                if (string.IsNullOrWhiteSpace(  Data.Province))
                {
                    ModelState.AddModelError(nameof(Data.Province), "Chọn tỉnh thành");
                }
                // Thông báo lỗi và yêu cầu nhập lại dữ liệu
                if (!ModelState.IsValid)
                {
                    return View("Edit", Data);
                }
            }
            catch (Exception ex)
            {
                return View("Edit", Data);
            }
            if (Data.SupplierID == 0)
            {
                await CommonDataService.SupplierDB.AddAsync(Data);
            }
            else
            {
                await CommonDataService.SupplierDB.UpdateAsync(Data);
            }
            return RedirectToAction("Index");
        }
    }
}
