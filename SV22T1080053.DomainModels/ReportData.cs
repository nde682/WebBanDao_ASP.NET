using System.Collections.Generic;

namespace SV22T1080053.DomainModels
{
    public class ReportData
    {
        public int CustomerCount { get; set; }          // Tổng số khách hàng

        public int TotalOrderCount { get; set; }        // Tổng số đơn hàng (Tất cả trạng thái)
        public int SuccessfulOrderCount { get; set; }   // Số đơn hàng thành công
        public decimal TotalRevenue { get; set; }       // Tổng doanh thu toàn thời gian

        public int WaitingOrderCount { get; set; }      // Đơn hàng chờ xử lý (vẫn giữ để thông báo)

        // Dữ liệu biểu đồ (giữ nguyên)
        public List<RevenueDataPoint> RevenueChartData { get; set; } = new List<RevenueDataPoint>();
    }

    public class RevenueDataPoint
    {
        public string Date { get; set; }
        public decimal Revenue { get; set; }
    }
}