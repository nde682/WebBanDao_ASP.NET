using SV22T1080053.DomainModels;
using System.Collections.Generic;

namespace SV22T1080053.Admin.Models
{
    // Dùng để chứa thông tin đầu vào tìm kiếm
    public class OrderSearchInput
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int Status { get; set; } = 0;
        public string DateRange { get; set; } = ""; 
        public string SearchValue { get; set; } = "";
    }

    // Dùng để hiển thị kết quả tìm kiếm ra Index
    public class OrderSearchResult
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string SearchValue { get; set; } = "";
        public int Status { get; set; }
        public string DateRange { get; set; } = "";
        public int RowCount { get; set; }
        public int PageCount { get; set; }
        public IEnumerable<Order> Data { get; set; }
    }

    // Dùng để hiển thị trang Details (gồm thông tin đơn và danh sách mặt hàng)
    public class OrderDetailModel
    {
        public Order Order { get; set; }
        public IEnumerable<OrderDetail> Details { get; set; }
    }
}