using SV22T1080053.DomainModels;

namespace SV22T1080053.Shop.Models
{
    // Class đại diện cho 1 dòng trong giỏ hàng
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public string Unit { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Tính thành tiền của từng dòng
        public decimal TotalPrice => Price * Quantity;
    }
}