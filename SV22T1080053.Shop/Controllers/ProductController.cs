using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BussinessLayers;
using System.Threading.Tasks;

namespace SV22T1080053.Shop.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        // Lưu ý: Phải dùng async Task<IActionResult>
        public async Task<IActionResult> Detail(int id = 0)
        {
            if (id <= 0) return RedirectToAction("Index");

            // 1. SỬA LỖI: Thêm 'await' để lấy kết quả từ Task
            var product = await ProductDataService.ProductDB.GetAsync(id);

            if (product == null) return RedirectToAction("Index");

            ViewBag.Categories = await CommonDataService.CategoryDB.ListAsync();

            ViewBag.Photos = await ProductDataService.ProductDB.ListPhotosAsync(id);
            ViewBag.Attributes = await ProductDataService.ProductDB.ListAttributesAsync(id);

            return View(product);
        }
    }
}
