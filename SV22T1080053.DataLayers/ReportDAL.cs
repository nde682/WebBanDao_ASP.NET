using Dapper;
using SV22T1080053.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1080053.DataLayers
{
    public class ReportDAL : BaseDAL
    {
        public ReportDAL(string connectionString) : base(connectionString)
        {
        }

        public async Task<ReportData> GetReportAsync()
        {
            var data = new ReportData();

            using (var connection = await OpenConnectionAsync())
            {
                // 1. Tổng số khách hàng
                var sqlCustomer = "SELECT COUNT(*) FROM Customers";
                data.CustomerCount = await connection.ExecuteScalarAsync<int>(sqlCustomer);

                // 2. Tổng số đơn hàng (Bao gồm tất cả: chờ, hủy, thành công...)
                var sqlTotalOrders = "SELECT COUNT(*) FROM Orders";
                data.TotalOrderCount = await connection.ExecuteScalarAsync<int>(sqlTotalOrders);

                // 3. Số đơn hàng thành công (Status = 4)
                var sqlSuccessOrders = "SELECT COUNT(*) FROM Orders WHERE Status = 4";
                data.SuccessfulOrderCount = await connection.ExecuteScalarAsync<int>(sqlSuccessOrders);

                // 4. Tổng doanh thu toàn thời gian (Chỉ tính đơn thành công Status = 4)
                // Phải JOIN OrderDetails để lấy giá bán * số lượng
                var sqlTotalRevenue = @"
                    SELECT ISNULL(SUM(d.Quantity * d.SalePrice), 0)
                    FROM Orders o
                    JOIN OrderDetails d ON o.OrderID = d.OrderID
                    WHERE o.Status = 4";
                data.TotalRevenue = await connection.ExecuteScalarAsync<decimal>(sqlTotalRevenue);

                // 5. Đơn hàng chờ xử lý (Status = 1) - Để hiện thông báo nếu cần
                var sqlWaiting = "SELECT COUNT(*) FROM Orders WHERE Status = 1";
                data.WaitingOrderCount = await connection.ExecuteScalarAsync<int>(sqlWaiting);

                // 6. Dữ liệu biểu đồ (30 ngày gần nhất)
                var sqlChart = @"
                    SELECT 
                        FORMAT(o.OrderTime, 'yyyy-MM-dd') as Date, 
                        SUM(d.Quantity * d.SalePrice) as Revenue
                    FROM Orders o
                    JOIN OrderDetails d ON o.OrderID = d.OrderID
                    WHERE o.OrderTime >= DATEADD(day, -30, GETDATE()) 
                      AND o.Status = 4 
                    GROUP BY FORMAT(o.OrderTime, 'yyyy-MM-dd')
                    ORDER BY Date ASC";

                var chartData = await connection.QueryAsync<RevenueDataPoint>(sqlChart);
                data.RevenueChartData = chartData.ToList();
            }

            return data;
        }
    }
}