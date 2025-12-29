namespace SV22T1080053.Shop.Models
{
    public class CustomerStatistics
    {
        public int TotalOrders { get; set; }        // Tổng số đơn
        public int WaitingOrders { get; set; }      // Đơn đang chờ (Status 1, 2, 3)
        public int FinishedOrders { get; set; }     // Đơn thành công (Status 4)
        public int CancelledOrders { get; set; }    // Đơn hủy (Status -1, -2)
        public decimal TotalSpent { get; set; }     // Tổng tiền đã chi tiêu (cho đơn thành công)
    }
}