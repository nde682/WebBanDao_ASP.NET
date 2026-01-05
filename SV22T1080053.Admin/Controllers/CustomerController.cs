using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using System.Buffers;
using System.Text.RegularExpressions;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize(Roles ="Admin")]
    public class CustomerController : Controller
    {
        private const int PAGESIZE = 20;
        private const string CUSTOMER_SEARCH_CONDITION = "CustomerSearchCondition";
        public async Task<IActionResult> Index()
        {
            var condition = ApplicationContext.GetSessionData<paginationSearchCondition>(CUSTOMER_SEARCH_CONDITION);
            if(condition == null)
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
            var data = await CommonDataService.CustomerDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.CustomerDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Customer>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            //Lưu điều kiện tìm kiếm vào session
            ApplicationContext.SetSessionData(CUSTOMER_SEARCH_CONDITION, condition);
            return View(model);
        }
        /// <summary>
        /// Thêm mới khách hàng
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Thêm khách hàng";
            var model = new Customer()
            {
                CustomerID = 0
            };
            return View("Edit", model);
        }
        /// <summary>
        /// Chỉnh sửa thông tin khách hàng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            var model = await CommonDataService.CustomerDB.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");
            return View(model);
        }
        /// <summary>
        /// dùng để lưu dữ liệu khách hàng (thêm mới hoặc cập nhật)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<IActionResult> SaveData(Customer data)
        {
            ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng mới" : "Cập nhật thông tin khách hàng";

            try
            {
                if (string.IsNullOrWhiteSpace(data.CustomerName))
                {
                    ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");
                }
                if (string.IsNullOrWhiteSpace(data.ContactName))
                {
                    ModelState.AddModelError(nameof(data.ContactName), "Tên liên lạc không được để trống");
                }
                if (string.IsNullOrWhiteSpace(data.Email))
                {
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập địa chỉ Email.");
                }
                else
                {
                    // Pattern kiểm tra email cơ bản
                    string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    if (!Regex.IsMatch(data.Email, emailPattern))
                    {
                        ModelState.AddModelError(nameof(data.Email), "Địa chỉ Email không đúng định dạng.");
                    }
                }

                // 3. Validate Số điện thoại (Kiểm tra rỗng + Định dạng VN 10 số)
                if (string.IsNullOrWhiteSpace(data.Phone))
                {
                    ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại.");
                }
                else
                {
                    // Pattern: Bắt đầu bằng số 0, theo sau là 9 chữ số bất kỳ (Tổng 10 số)
                    string phonePattern = @"^0\d{9}$";
                    if (!Regex.IsMatch(data.Phone, phonePattern))
                    {
                        ModelState.AddModelError(nameof(data.Phone), "Số điện thoại không hợp lệ (phải bắt đầu bằng số 0 và gồm 10 chữ số).");
                    }
                }
                if (string.IsNullOrWhiteSpace(data.Address))
                {
                    ModelState.AddModelError(nameof(data.Address), "Địa chỉ không được để trống");
                }
                if (string.IsNullOrWhiteSpace(data.Province))
                {
                    ModelState.AddModelError(nameof(data.Province), "Chọn tỉnh thành");
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
            //TODO : Kiếm tra tính đúng đắn của dữ liệu nhập vào
            if (data.CustomerID == 0)
            {
                await CommonDataService.CustomerDB.AddAsync(data);
            }
            else
            {
                await CommonDataService.CustomerDB.UpdateAsync(data);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            //ViewBag.Title = "Xóa khách hàng";         không cần thiết vì chỉ dùng duy nhất 1 view 
            if (Request.Method == "POST")               // kiểm tra nếu phương thức gửi lên là POST
            {
                // Thực hiện xóa
                await CommonDataService.CustomerDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.CustomerDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);                     // trả về view xóa với dữ liệu là model
            }
            //return View();
        }
    }

}
