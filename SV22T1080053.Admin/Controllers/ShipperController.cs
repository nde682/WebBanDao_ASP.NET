using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using System.Reflection;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {
        /*public IActionResult Index()
        {
            return View();
        }*/
        private const int PAGESIZE = 20;
        private const string SHIPPER_SEARCH_CONDITION = "ShipperSearchCondition";
        /// <summary>
        /// Index hiển thị danh sách nhà vận chuyển dưới dạng phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index()
        {

            var condition = ApplicationContext.GetSessionData<paginationSearchCondition>(SHIPPER_SEARCH_CONDITION);
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

            var data = await BussinessLayers.CommonDataService.ShipperDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await BussinessLayers.CommonDataService.ShipperDB.CountAsync(condition.SearchValue);
            var model = new Models.PaginationSearchResult<DomainModels.Shipper>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(SHIPPER_SEARCH_CONDITION,condition);
            return View(model);
        }
        /// <summary>
        /// Thêm mới người giao hàng 
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhà vận chuyển";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Chỉnh sửa thông tin người giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhà vận chuyển";
            var model = await CommonDataService.ShipperDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// SavaData lưu thông tin người giao hàng (thêm mới hoặc cập nhật)
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Shipper data)
        {
            //TODO : kiểm tra tính hợp lệ của dữ liệu
            try
            {
                if (string.IsNullOrWhiteSpace(data.ShipperName))
                {
                    ModelState.AddModelError(nameof(data.ShipperName), "Tên người giao hàng không được để trống");
                }
                if (string.IsNullOrWhiteSpace(data.Phone))
                {
                    ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không được để trống");
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
            if (data.ShipperID == 0)
            {
                await CommonDataService.ShipperDB.AddAsync(data);
            }
            else
            {
                await CommonDataService.ShipperDB.UpdateAsync(data);
            }
            return RedirectToAction("Index");
        }
        /// <summary>
        /// Xóa người giao hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.ShipperDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.ShipperDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);                                             // truyền model để hiển thị thông tin người giao hàng cần xóa
            }
        }
    }
}
