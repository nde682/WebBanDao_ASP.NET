using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1080053.Shop;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký HttpContextAccessor (Cần thiết cho ApplicationContext)
builder.Services.AddHttpContextAccessor();

// Đăng ký Controllers và Views
builder.Services.AddControllersWithViews()
    .AddMvcOptions(option =>
    {
        // Cho phép không bắt buộc validate các trường non-nullable (tránh lỗi validate ngầm định)
        option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    });

// Đăng ký Authentication (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        // ĐỔI TÊN COOKIE: Để không bị trùng với trang Admin nếu chạy cùng lúc
        option.Cookie.Name = "SV22T1080053.Shop.Auth";
        option.LoginPath = "/Account/Login";            // Đường dẫn trang đăng nhập
        option.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn khi không có quyền
        option.ExpireTimeSpan = TimeSpan.FromDays(30);  // Thời gian nhớ đăng nhập
        option.SlidingExpiration = true;                // Gia hạn thời gian nếu người dùng còn thao tác
    });

// Đăng ký Session
builder.Services.AddDistributedMemoryCache(); // Cần thiết để lưu Session trong RAM
builder.Services.AddSession(option =>
{
    // LƯU Ý: Session không nên để quá lâu (30 ngày) vì tốn RAM server.
    // Chỉ nên để khoảng 30-60 phút. Cookie Auth mới là cái giữ trạng thái "Nhớ đăng nhập".
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

// =========================================================
// 2. BUILD APP & KHỞI TẠO CẤU HÌNH (INITIALIZATION)
// =========================================================
var app = builder.Build();

// Khởi tạo Business Layer (Database connection)
string connectionString = builder.Configuration.GetConnectionString("LiteCommerceDB")
                          ?? throw new Exception("Không tìm thấy chuỗi kết nối 'LiteCommerceDB' trong appsettings.json");
SV22T1080053.BussinessLayers.Configuration.Initialize(connectionString);

// Khởi tạo ApplicationContext (Biến toàn cục)
ApplicationContext.Configure
(
    httpContextAccessor: app.Services.GetRequiredService<IHttpContextAccessor>(),
    webHostEnvironment: app.Services.GetRequiredService<IWebHostEnvironment>(),
    configuration: app.Configuration
);

// Cấu hình định dạng văn hóa (Tiếng Việt) cho toàn bộ ứng dụng
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Môi trường không phải Development thì hiện trang lỗi thân thiện
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

// Cho phép dùng file tĩnh (css, js, images) trong wwwroot
app.UseStaticFiles();

// Kích hoạt Session (Nên đặt trước Routing để Session sẵn sàng sớm)
app.UseSession();

// Xác định Route (Controller/Action)
app.UseRouting();

// Xác thực (Ai đang đăng nhập?)
app.UseAuthentication();

// Phân quyền (Được phép làm gì?)
app.UseAuthorization();

// Định nghĩa Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Chạy ứng dụng
app.Run();