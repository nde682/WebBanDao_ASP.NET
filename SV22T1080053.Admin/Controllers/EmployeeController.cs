using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private const int PAGESIZE = 12;
        private const string EMPLOYEE_SEARCH_CONDITION = "EmployeeSearchCondition";
        public async Task<IActionResult> Index()
        {
            var condition = ApplicationContext.GetSessionData<paginationSearchCondition>(EMPLOYEE_SEARCH_CONDITION);
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
            var data = await CommonDataService.EmployeeDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue);
            var rowCount = await CommonDataService.EmployeeDB.CountAsync(condition.SearchValue);
            var model = new PaginationSearchResult<Employee>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };
            ApplicationContext.SetSessionData(EMPLOYEE_SEARCH_CONDITION, condition);
            return View(model);
        }
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await CommonDataService.EmployeeDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }
            else
            {
                var model = await CommonDataService.EmployeeDB.GetAsync(id);
                if (model == null)
                {
                    return RedirectToAction("Index");
                }
                return View(model);
            }
        }
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new EmployeeEditModel()
            {
                EmployeeID = 0,
                Photo = "nophoto.png"
            };
            return View("Edit", model);
        }
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var employee = await CommonDataService.EmployeeDB.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            var model = new EmployeeEditModel()
            {
                EmployeeID = employee.EmployeeID,
                FullName = employee.FullName,
                BirthDate = employee.BirthDate,
                Address = employee.Address,
                Email = employee.Email,
                Phone = employee.Phone,
                Photo = employee.Photo,
                IsWorking = employee.IsWorking
            };
            return View(model);
        }
        /// <summary>
        /// / Xử lý lưu dữ liệu từ form (Create, Edit)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SaveData(EmployeeEditModel model)
        {
            //TODO: Kiểm tra dữ liệu đầu vào
            ViewBag.Title = model.EmployeeID == 0 ? "Bổ sung nhân viên mới" : "Cập nhật thông tin nhân viên";

            try
            {
                if (string.IsNullOrWhiteSpace(model.FullName))
                {
                    ModelState.AddModelError(nameof(model.FullName), "Tên nhân viên không được để trống");
                }
            

                // Thông báo lỗi và yêu cầu nhập lại dữ liệu
                if (!ModelState.IsValid)
                {
                    return View("Edit", model);
                }
            }
            catch (Exception ex)
            {
                return View("Edit", model);
            }

            //Nếu có ảnh thì upload ảnh lên và lấy tên file ảnh mới upload cho Photo
            if (model.UploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\employees", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadPhoto.CopyToAsync(stream);
                }
                model.Photo = fileName;
            }

            Employee data = new Employee()
            {
                EmployeeID = model.EmployeeID,
                FullName = model.FullName,
                BirthDate = model.BirthDate,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone,
                Photo = model.Photo,
                IsWorking = model.IsWorking
            };

            if (data.EmployeeID == 0)
            {
                await CommonDataService.EmployeeDB.AddAsync(data);
            }
            else
            {
                await CommonDataService.EmployeeDB.UpdateAsync(data);
            }

            return RedirectToAction("Index");
        }
    }
}
