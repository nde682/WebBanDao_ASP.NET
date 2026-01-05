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
            // 1. Lấy tất cả (Lấy ngẫu nhiên hoặc lấy mặc định 8 cái)
            var AllProducts = await ProductDataService.ProductDB.ListWithSortAsync(1, 8, "",0,0,0,0, "name_asc"); // Giả sử hàm này lấy list có phân trang, bạn có thể viết hàm lấy Top 8 riêng

            var NewestProducts = await ProductDataService.ProductDB.ListNewestAsync(8);

            var CheapestProducts = await ProductDataService.ProductDB.ListCheapestAsync(8);

            var BestSellerProducts = await ProductDataService.ProductDB.ListBestSellersAsync(8);

            var Bestseller = await ProductDataService.ProductDB.GetBestSellerAsync();
            var model = new HomeIndexModel
            {
                AllProducts = AllProducts,
                NewestProducts = NewestProducts,
                CheapestProducts = CheapestProducts,
                BestSellerProducts = BestSellerProducts,
                Bestseller = Bestseller?? new Product(),
            };
            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}