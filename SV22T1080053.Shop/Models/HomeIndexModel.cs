using SV22T1080053.DomainModels;

namespace SV22T1080053.Shop.Models
{
    public class HomeIndexModel
    {
        public IEnumerable<Product> AllProducts { get; set; } = new List<Product>();      // Tab 1: Tất cả
        public IEnumerable<Product> NewestProducts { get; set; } = new List<Product>();     // Tab 2: Mới nhất
        public IEnumerable<Product> CheapestProducts { get; set; } = new List<Product>();   // Tab 3: Rẻ nhất (Thay cho Featured)
        public IEnumerable<Product> BestSellerProducts { get; set; } = new List<Product>(); // Tab 4: Bán chạy
        public Product Bestseller { get; set; }= new Product();

    }
}