namespace SV22T1080053.Admin.Models
{
    /// <summary>
    /// đầu vàng sử dụng cho tìm kiếm và phân trang dữ liệu
    /// </summary>
    public class paginationSearchCondition
    {
        /// <summary>
        /// Trang cần hiển thị
        /// </summary>
        public int Page { get; set; }=1;
        /// <summary>
        /// Số phần tử trong trang
        /// </summary>
        public int PageSize {  get; set; }
        /// <summary>
        /// Giá trị tìm kiếm
        /// </summary>
        public string SearchValue { get; set; } = "";

    }
    /// <summary>
    /// Đầu vào tìm kiếm, phân trang đối với mặt hàng
    /// </summary>
    public class ProductSearchCondition : paginationSearchCondition
    {
        public int CategoryID { get; set; } = 0;
        public int SupplierID { get; set; } = 0;
        public decimal MinPrice { get; set; } = 0;
        public decimal MaxPrice { get; set; } = 0;
    }

}
