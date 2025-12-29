using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BusinessLayers;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using SV22T1080053.Shop.AppCodes;
using SV22T1080053.Shop.Models; // Namespace chứa WebUserData

namespace SV22T1080053.Shop.Controllers
{
    [Authorize] // Mặc định yêu cầu đăng nhập cho tất cả Action, trừ [AllowAnonymous]
    public class AccountController : Controller
    {
        // ======================= ĐĂNG NHẬP & ĐĂNG XUẤT =======================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            // Nếu đã đăng nhập rồi thì đá về trang chủ, không cần đăng nhập lại
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.UserName = username;

            // 1. Kiểm tra dữ liệu nhập
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập Email và Mật khẩu.");
                return View();
            }

            // 2. Kiểm tra thông tin qua Service
            var userAccount = await UserAccountService.CustomerUserAccountDB.AuthenticateAsync(username, password);
            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại! Email hoặc mật khẩu không đúng.");
                return View();
            }

            // 3. Tạo Claims và Cookie
            var userData = new WebUserData()
            {
                UserId = userAccount.UserID,
                UserName = userAccount.UserName,
                DisplayName = userAccount.FullName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = new List<string>() { "Customer" }
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal(),
                new AuthenticationProperties
                {
                    IsPersistent = true, // Ghi nhớ đăng nhập
                    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                });

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View(); // Tạo View thông báo lỗi truy cập nếu cần
        }

        // ======================= ĐĂNG KÝ TÀI KHOẢN =======================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new Customer());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(Customer data, string confirmPassword, IFormFile? uploadPhoto,string Password)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập họ và tên.");

            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập địa chỉ Email.");

            if (string.IsNullOrWhiteSpace(Password))
                ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu.");

            if (Password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu nhập lại không khớp.");

            // Kiểm tra độ dài mật khẩu (tùy chọn)
            if (!string.IsNullOrEmpty(Password) && Password.Length < 6)
                ModelState.AddModelError("Password", "Mật khẩu phải có ít nhất 6 ký tự.");
            if (!ModelState.IsValid) return View(data);
            string Photo = "";
            // Xử lý ảnh đại diện
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                // Đặt tên file duy nhất để tránh trùng lặp: TimeStamp + Tên gốc
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";

                // Đường dẫn lưu file: wwwroot/images/customers
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), ApplicationContext.CustomerImagePath);

                // Tạo thư mục nếu chưa có
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                Photo = fileName;
            }
            else
            {
                Photo = ""; // Hoặc đặt ảnh mặc định "default-user.png" nếu muốn
            }

            // Các dữ liệu mặc định khác
            data.ContactName = data.ContactName ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";
            data.Province = data.Province;

            int result = await UserAccountService.CustomerUserAccountDB.RegisterAsync(data,Password,Photo);

            if (result > 0)
            {
                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else if (result == -1)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(data);
            }

            ModelState.AddModelError("Error", "Đăng ký thất bại.");
            return View(data);
        }

        // ======================= ĐỔI MẬT KHẨU =======================

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            // 1. Kiểm tra dữ liệu nhập vào (Validate)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 2. Lấy thông tin user đang đăng nhập
            var userData = User.GetUserData();
            string currentUserId = userData?.Email ?? "";
            if (currentUserId == "") return RedirectToAction("Login");

            // 3. Gọi DAL để đổi mật khẩu
            bool result = await UserAccountService.CustomerUserAccountDB.ChangePasswordAsync(currentUserId, model.OldPassword, model.NewPassword);

            if (result)
            {
                // Thành công: Thông báo và reset form
                TempData["Message"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("ChangePassword"); // Load lại trang sạch sẽ
            }
            else
            {
                // Thất bại: Thường là do mật khẩu cũ không đúng
                ModelState.AddModelError("OldPassword", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }
        }
        // ======================= THÔNG TIN CÁ NHÂN =======================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            // Lấy ID khách hàng từ Cookie
            var userData = User.GetUserData();
            int customerId = 0;
            int.TryParse(userData?.UserId, out customerId);

            if (customerId == 0) return RedirectToAction("Login");

            // Lấy thông tin chi tiết từ DB
            var data = await CommonDataService.CustomerDB.GetAsync(customerId);
            if (data == null) return RedirectToAction("Login");

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer data, IFormFile? uploadPhoto)
        {
            // 1. Lấy thông tin người dùng đang đăng nhập từ Cookie
            var userData = User.GetUserData();
            int currentUserId = 0;
            int.TryParse(userData?.UserId, out currentUserId);

            // 2. Kiểm tra bảo mật: Nếu chưa đăng nhập hoặc ID gửi lên khác ID đang đăng nhập
            if (currentUserId == 0 || data.CustomerID != currentUserId)
            {
                return RedirectToAction("Login");
            }

            // 3. Validate dữ liệu nhập
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên không được để trống");

            // (Thêm các validate khác nếu cần)

            if (!ModelState.IsValid)
            {
                return View("Profile", data);
            }

            // 4. Xử lý logic dữ liệu
            // Lấy dữ liệu cũ từ DB để giữ lại những thông tin không có trong Form (như Password, Email, IsLocked)
            var oldData = await CommonDataService.CustomerDB.GetAsync(currentUserId);
            if (oldData == null) return RedirectToAction("Login");

            // Xử lý ảnh đại diện
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                // -- Upload ảnh mới --
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), ApplicationContext.CustomerImagePath);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string filePath = Path.Combine(folderPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                data.Photo = fileName;

            }
            else
            {
                // -- Giữ nguyên ảnh cũ --
                data.Photo = oldData.Photo;
            }

            // Giữ nguyên các thông tin nhạy cảm không được sửa từ Form
            data.Email = oldData.Email;  
            data.IsLocked = oldData.IsLocked;

            // Xử lý null cho các trường phụ
            data.ContactName = data.ContactName ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";

            // 5. Gọi DAL cập nhật
            bool result = await CommonDataService.CustomerDB.UpdateFullAsync(data);

            if (result)
            {
                var newAuthData = new WebUserData()
                {
                    UserId = userData.UserId,
                    UserName = data.Email,
                    DisplayName = data.CustomerName, // Cập nhật tên mới
                    Email = data.Email,
                    Photo = data.Photo,             // Cập nhật ảnh mới
                    Roles = userData.Roles
                };

                // Hủy cookie cũ và tạo cookie mới
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                              newAuthData.CreatePrincipal(),
                                              new AuthenticationProperties { IsPersistent = true });

                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile"); // Load lại trang Profile (Get)
            }
            else
            {
                ModelState.AddModelError("Error", "Cập nhật thất bại. Vui lòng thử lại.");
                ViewBag.Provinces = await CommonDataService.ProvinceDB.ListAsync();
                return View("Profile", data);
            }
        }
        [Authorize]
        public async Task<IActionResult> Statistics()
        {
            // 1. Lấy UserID
            var user = User.GetUserData();
            int customerID = int.Parse(user.UserId);

            // 2. Lấy số liệu thống kê
            var model = await OrderDataService.OrderDB.GetCustomerStatsAsync(customerID);

            return View(model);
        }
    }
}