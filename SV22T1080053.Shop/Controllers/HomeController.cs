using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Shop.Models;        // Nơi chứa HomeViewModel
using SV22T1080053.BussinessLayers;    // Nơi chứa ProductDataService (Chú ý namespace của bạn là BussinessLayers có 2 chữ s)
using System.Diagnostics;
using SV22T1080053.DomainModels;
using Microsoft.AspNetCore.Authorization;

namespace SV22T1080053.Shop.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var bannerData = await ProductDataService.ProductDB.ListAsync(page: 1, pageSize: 3, searchValue: "");
            var featuredData = await ProductDataService.ProductDB.ListAsync(page: 1, pageSize: 12, searchValue: "");
            var bestseller = await ProductDataService.ProductDB.GetAsync(1);
            // 3. Đưa dữ liệu vào ViewModel
            var model = new HomeViewModel
            {
                BannerProducts = bannerData,
                BestSeller = bestseller,
                FeaturedProducts = featuredData
            };

            // 4. Trả về View kèm theo Model
            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}