using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin;
using SV22T1080053.Admin.Models;
using SV22T1080053.BussinessLayers; // Giả định em dùng ProductDataService
using SV22T1080053.DomainModels;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace SV22T1080053.Web.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private const int PAGESIZE = 20;
        private const string PRODUCT_SEARCH_CONDITION = "ProductSearchCondition";

        // --- 1. TÌM KIẾM VÀ HIỂN THỊ (INDEX) ---
        public IActionResult Index()
        {
            var condition = ApplicationContext.GetSessionData<ProductSearchCondition>(PRODUCT_SEARCH_CONDITION);
            if (condition == null)
            {
                condition = new ProductSearchCondition()
                {
                    Page = 1,
                    PageSize = PAGESIZE,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0
                };
            }
            return View(condition);
        }

        public async Task<IActionResult> Search(ProductSearchCondition condition)
        {
            var data = await ProductDataService.ProductDB.ListAsync(condition.Page, condition.PageSize, condition.SearchValue,
                                                                   condition.CategoryID, condition.SupplierID, condition.MinPrice, condition.MaxPrice);
            var rowCount = await ProductDataService.ProductDB.CountAsync(condition.SearchValue,
                                                                        condition.CategoryID, condition.SupplierID, condition.MinPrice, condition.MaxPrice);

            var model = new PaginationSearchResult<Product>()
            {
                Page = condition.Page,
                PageSize = condition.PageSize,
                SearchValue = condition.SearchValue,
                RowCount = rowCount,
                Data = data
            };

            ApplicationContext.SetSessionData(PRODUCT_SEARCH_CONDITION, condition);
            return View(model);
        }

        // --- 2. THÊM / SỬA MẶT HÀNG (EDIT) ---
        public async Task<IActionResult> Create()
        {
            ViewBag.Title = "Bổ sung mặt hàng";
            var model = new ProductEditModel()
            {
                ProductID = 0,
                Photo = "nophoto.png", // Ảnh mặc định
                IsSelling = true
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin mặt hàng";
            var product = await ProductDataService.ProductDB.GetAsync(id);
            if (product == null)
                return RedirectToAction("Index");
            var model = new ProductEditModel()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ProductDescription = product.ProductDescription,
                CategoryID = product.CategoryID,
                SupplierID = product.SupplierID,
                IsSelling = product.IsSelling,
                Photo = product.Photo,
                Price = product.Price,
                Unit = product.Unit
            };
            // Lấy danh sách ảnh và thuộc tính để hiển thị ở View Edit
            ViewBag.Photos = await ProductDataService.ProductDB.ListPhotosAsync(id);
            ViewBag.Attributes = await ProductDataService.ProductDB.ListAttributesAsync(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(ProductEditModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.ProductName))
                {
                    ModelState.AddModelError(nameof(model.ProductName), "Tên mặt hàng không được để trống");
                }
                if (model.CategoryID == 0)
                {
                    ModelState.AddModelError(nameof(model.CategoryID), "Hãy chọn loại hàng");
                }
                if (model.SupplierID == 0)
                {
                    ModelState.AddModelError(nameof(model.SupplierID), "Hãy chọn nhà cung cấp");
                }
                if (string.IsNullOrWhiteSpace(model.Unit))
                {
                    ModelState.AddModelError(nameof(model.Unit), "Đơn vị không được để trống");
                }
                if (string.IsNullOrWhiteSpace(model.Price.ToString()))
                {
                    ModelState.AddModelError(nameof(model.Price), "Giá cả không được để trống");
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
            // Xử lý upload ảnh chính
            if (model.UploadPhoto != null)
            {
                string fileName = $"{DateTime.Now.Ticks}_{model.UploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.UploadPhoto.CopyToAsync(stream);
                }
                model.Photo = fileName;
            }

            if (model.ProductID == 0)
            {
                model.ProductID = await ProductDataService.ProductDB.AddAsync(model);
                ViewBag.Photos = await ProductDataService.ProductDB.ListPhotosAsync(model.ProductID);
                ViewBag.Attributes = await ProductDataService.ProductDB.ListAttributesAsync(model.ProductID);
                return View("Edit",model);
            }
            else
            {
                await ProductDataService.ProductDB.UpdateAsync(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await ProductDataService.ProductDB.DeleteAsync(id);
                return RedirectToAction("Index");
            }

            var model = await ProductDataService.ProductDB.GetAsync(id);
            if (model == null) return RedirectToAction("Index");
            return View(model);
        }

        // --- 3. XỬ LÝ ẢNH PHỤ (PHOTO) ---
        public async Task<IActionResult> Photo(string id, string method, int photoId = 0)
        {
            // id ở đây là ProductID (chuỗi query string từ view truyền lên có thể là string, cần parse)
            int productID = 0;
            int.TryParse(id, out productID);

            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung ảnh cho mặt hàng";
                    var model = new ProductPhoto() { ProductID = productID, PhotoID = 0 };
                    return View(model);
                case "edit":
                    ViewBag.Title = "Thay đổi ảnh của mặt hàng";
                    var photo = await ProductDataService.ProductDB.GetPhotoAsync(photoId);
                    return View(photo);
                case "delete":
                    // Xóa trực tiếp rồi quay lại trang Edit
                    await ProductDataService.ProductDB.DeletePhotoAsync(photoId);
                    return RedirectToAction("Edit", new { id = productID });
                default:
                    return RedirectToAction("Edit", new { id = productID });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SavePhoto(ProductPhoto model, IFormFile? uploadPhoto)
        {
            
            if (uploadPhoto != null)
            {
                
                string fileName = $"{DateTime.Now.Ticks}_{uploadPhoto.FileName}";
                string filePath = Path.Combine(ApplicationContext.WWWRootPath, @"images\products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }
                model.Photo = fileName;
            }
            try
            {
                if (string.IsNullOrWhiteSpace(model.Photo))
                {
                    ModelState.AddModelError(nameof(model.Photo), "Vui lòng chọn ảnh");
                }

                if (!ModelState.IsValid)
                {
                    return View("Photo", model);
                }
            }
            catch (Exception ex)
            {
                return View("Photo", model);
            }
            if (model.PhotoID == 0)
                await ProductDataService.ProductDB.AddPhotoAsync(model);
            else
                await ProductDataService.ProductDB.UpdatePhotoAsync(model);

            return RedirectToAction("Edit", new { id = model.ProductID });
        }

        // --- 4. XỬ LÝ THUỘC TÍNH (ATTRIBUTE) ---
        public async Task<IActionResult> Attribute(string id, string method, int attributeId = 0)
        {
            int productID = 0;
            int.TryParse(id, out productID);

            switch (method)
            {
                case "add":
                    ViewBag.Title = "Bổ sung thuộc tính cho mặt hàng";
                    var model = new ProductAttribute() { ProductID = productID, AttributeID = 0 };
                    return View(model);
                case "edit":
                    ViewBag.Title = "Thay đổi thuộc tính của mặt hàng";
                    var attr = await ProductDataService.ProductDB.GetAttributeAsync(attributeId);
                    return View(attr);
                case "delete":
                    await ProductDataService.ProductDB.DeleteAttributeAsync(attributeId);
                    return RedirectToAction("Edit", new { id = productID });
                default:
                    return RedirectToAction("Edit", new { id = productID });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttribute(ProductAttribute model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.AttributeName))
                {
                    ModelState.AddModelError(nameof(model.AttributeName), "Tên mặt hàng không được để trống");
                }
                // Thông báo lỗi và yêu cầu nhập lại dữ liệu
                if (!ModelState.IsValid)
                {
                    return View("Attribute", model);
                }
            }
            catch (Exception ex)
            {
                return View("Attribute", model);
            }
            if (model.AttributeID == 0)
                await ProductDataService.ProductDB.AddAttributeAsync(model);
            else
                await ProductDataService.ProductDB.UpdateAttributeAsync(model);

            return RedirectToAction("Edit", new { id = model.ProductID });
        }
    }
}