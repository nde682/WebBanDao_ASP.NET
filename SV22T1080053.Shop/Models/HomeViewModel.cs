using SV22T1080053.DomainModels;

namespace SV22T1080053.Shop.Models
{
    public class HomeViewModel
    {
        // Danh sách sản phẩm chạy trên Slider (Carousel)
        public IEnumerable<Product> BannerProducts { get; set; } = new List<Product>();
        public Product BestSeller {  get; set; } = new Product();
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
    }
}