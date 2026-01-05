using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DataLayers;
using SV22T1080053.DomainModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Gọi Service lấy dữ liệu (Async)
            var data = await ReportDataService.GetDashboardReportAsync();

            // 2. Đưa vào ViewModel
            var model = new DashboardViewModel
            {
                ReportData = data
            };

            return View(model);
        }
    }
}
