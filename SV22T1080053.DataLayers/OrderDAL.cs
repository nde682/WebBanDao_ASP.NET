using Dapper;
using SV22T1080053.DataLayers;
using SV22T1080053.DomainModels;

namespace SV22T1080053.DataLayers.SQLServer
{
    /// <summary>
    /// Các chức năng xử lý dữ liệu liên quan đến đơn hàng và nội dung của đơn hàng
    /// </summary>
    public class OrderDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public OrderDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="status"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Order>> ListAsync(int page = 1, int pageSize = 0, int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"with cte as
                        (
                            select  row_number() over(order by o.OrderTime desc) as RowNumber,
                                    o.*,
                                    c.CustomerName,
                                    c.ContactName as CustomerContactName,
                                    c.Address as CustomerAddress,
                                    c.Phone as CustomerPhone,
                                    c.Email as CustomerEmail,
                                    e.FullName as EmployeeName,
                                    s.ShipperName,
                                    s.Phone as ShipperPhone        
                            from    Orders as o
                                    left join Customers as c on o.CustomerID = c.CustomerID
                                    left join Employees as e on o.EmployeeID = e.EmployeeID
                                    left join Shippers as s on o.ShipperID = s.ShipperID
                            where   (@Status = 0 or o.Status = @Status)
                                and (@FromTime is null or o.OrderTime >= @FromTime)
                                and (@ToTime is null or o.OrderTime <= @ToTime)
                                and (c.CustomerName like @SearchValue or e.FullName like @SearchValue or s.ShipperName like @SearchValue)
                        )
                        select * from cte 
                        where (@PageSize = 0) or (RowNumber between (@Page - 1) * @PageSize + 1 and @Page * @PageSize)
                        order by RowNumber";
            var parameters = new
            {
                page,
                pageSize,
                status,
                fromTime,
                toTime,
                searchValue
            };
            return await connection.QueryAsync<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Đếm số đơn hàng từ kết quả tìm kiếm
        /// </summary>
        /// <param name="status"></param>
        /// <param name="fromTime"></param>
        /// <param name="toTime"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(int status = 0, DateTime? fromTime = null, DateTime? toTime = null, string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"select  count(*)       
                        from    Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                        where   (@Status = 0 or o.Status = @Status)
                            and (@FromTime is null or o.OrderTime >= @FromTime)
                            and (@ToTime is null or o.OrderTime <= @ToTime)
                            and (c.CustomerName like @SearchValue or e.FullName like @SearchValue or s.ShipperName like @SearchValue)";
            var parameters = new
            {
                status,
                fromTime,
                toTime,
                SearchValue = searchValue ?? ""
            };
            return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Lấy thông tin chi tiết của 1 đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public async Task<Order?> GetAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select o.*,
                                c.CustomerName,
                                c.ContactName as CustomerContactName,
                                c.Address as CustomerAddress,
                                c.Phone as CustomerPhone,
                                c.Email as CustomerEmail,
                                e.FullName as EmployeeName,
                                s.ShipperName,
                                s.Phone as ShipperPhone        
                        from    Orders as o
                                left join Customers as c on o.CustomerID = c.CustomerID
                                left join Employees as e on o.EmployeeID = e.EmployeeID
                                left join Shippers as s on o.ShipperID = s.ShipperID
                        where   o.OrderID = @OrderID";
            var parameters = new
            {
                orderID
            };
            return await connection.QueryFirstOrDefaultAsync<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Bổ sung 1 đơn hàng mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"insert into Orders(CustomerId, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                            values(@CustomerID, getdate(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                            SELECT CONVERT(INT, SCOPE_IDENTITY());";
            return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Bổ sung 1 đơn hàng mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> CustomerAddAsync(Order data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"insert into Orders(CustomerId, OrderTime, DeliveryProvince, DeliveryAddress, Status)
                            values(@CustomerID, getdate(), @DeliveryProvince, @DeliveryAddress, @Status);
                            SELECT CONVERT(INT, SCOPE_IDENTITY());";
            return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// Cập nhật đơn hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set CustomerID = @CustomerID,
                            OrderTime = @OrderTime,
                            DeliveryProvince = @DeliveryProvince,
                            DeliveryAddress = @DeliveryAddress,
                            EmployeeID = @EmployeeID,
                            AcceptTime = @AcceptTime,
                            ShipperID = @ShipperID,
                            ShippedTime = @ShippedTime,
                            FinishedTime = @FinishedTime,
                            Status = @Status
                        where OrderID = @OrderID";
            return (await connection.ExecuteAsync(sql: sql, param: data, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Xoá 1 đơn hàng
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        /// <summary>
        /// Xóa đơn hàng và chi tiết (Chỉ xóa được đơn hàng ở trạng thái: Mới, Hủy, hoặc Từ chối)
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"if exists(select * from Orders where OrderID = @OrderID and Status in (@StatusInit, @StatusCancel, @StatusRejected))
                        begin
                            delete from OrderDetails where OrderID = @OrderID;
                            delete from Orders where OrderID = @OrderID;
                        end";

            var parameters = new
            {
                OrderID = orderID,
                StatusInit = Constants.ORDER_INIT,         // 1
                StatusCancel = Constants.ORDER_CANCEL,     // -1
                StatusRejected = Constants.ORDER_REJECTED  // -2
            };

            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Lấy thông tin chi tiết cụ thể của 1 đơn hàng có orderID
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        public async Task<IEnumerable<OrderDetail>> ListDetailsAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select  od.*, p.ProductName, p.Photo, p.Unit
                        from    OrderDetails as od
                                join Products as p on od.ProductID = p.ProductID
                        where od.OrderID = @OrderID";
            var parameters = new { orderID };
            return await connection.QueryAsync<OrderDetail>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        public async Task<OrderDetail?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select  od.*, p.ProductName, p.Photo, p.Unit
                            from    OrderDetails as od
                                    join Products as p on od.ProductID = p.ProductID
                            where od.OrderID = @OrderID and od.ProductID = @ProductID";
            var parameters = new { orderID, productID };
            return await connection.QueryFirstOrDefaultAsync<OrderDetail>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <param name="quantity"></param>
        /// <param name="salePrice"></param>
        /// <returns></returns>
        public async Task<bool> SaveDetailAsync(int orderID, int productID, int quantity, decimal salePrice)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"if exists(select * from OrderDetails where OrderID = @OrderID and ProductID = @ProductID)
                                update OrderDetails 
                                set Quantity = @Quantity, SalePrice = @SalePrice 
                                where OrderID = @OrderID and ProductID = @ProductID
                            else
                                insert into OrderDetails(OrderID, ProductID, Quantity, SalePrice) 
                                values(@OrderID, @ProductID, @Quantity, @SalePrice)";
            var parameters = new
            {
                orderID,
                productID,
                quantity,
                salePrice
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderID"></param>
        /// <param name="productID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"delete from OrderDetails where OrderID = @OrderID and ProductID = @ProductID";
            var parameters = new
            {
                orderID,
                productID
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Duyệt đơn hàng (Chuyển sang trạng thái đã duyệt/chờ giao hàng)
        /// </summary>
        public async Task<bool> AcceptAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set Status = @NewStatus, AcceptTime = getdate()
                        where OrderID = @OrderID and Status = @ExpectedStatus";

            var parameters = new
            {
                OrderID = orderID,
                NewStatus = Constants.ORDER_ACCEPTED, // Chuyển thành 2
                ExpectedStatus = Constants.ORDER_INIT // Chỉ duyệt đơn đang ở trạng thái 1
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Xác nhận chuyển đơn hàng cho người giao hàng (Shipper)
        /// </summary>
        public async Task<bool> ShipAsync(int orderID, int shipperID)
        {
            using var connection = await OpenConnectionAsync();
            // Đơn hàng có thể chuyển đi giao nếu đang ở trạng thái:  Đã duyệt (2)
            var sql = @"update Orders
                        set Status = @NewStatus, ShippedTime = getdate(), ShipperID = @ShipperID
                        where OrderID = @OrderID and  Status = @StatusAccepted";

            var parameters = new
            {
                OrderID = orderID,
                ShipperID = shipperID,
                NewStatus = Constants.ORDER_SHIPPING,    
                StatusAccepted = Constants.ORDER_ACCEPTED 
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Ghi nhận đơn hàng đã hoàn tất thành công
        /// </summary>
        public async Task<bool> FinishAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set Status = @NewStatus, FinishedTime = getdate()
                        where OrderID = @OrderID and Status = @ExpectedStatus"; // Chỉ hoàn tất khi đang giao hàng

            var parameters = new
            {
                OrderID = orderID,
                NewStatus = Constants.ORDER_FINISHED, // Chuyển thành 4
                ExpectedStatus = Constants.ORDER_SHIPPING // 3
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Hủy bỏ đơn hàng
        /// </summary>
        public async Task<bool> CancelAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set Status = @NewStatus, FinishedTime = getdate()
                        where OrderID = @OrderID and Status <> @NotAllowedStatus"; // Không thể hủy đơn đã hoàn tất

            var parameters = new
            {
                OrderID = orderID,
                NewStatus = Constants.ORDER_CANCEL,        // -1
                NotAllowedStatus = Constants.ORDER_FINISHED // 4
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }

        /// <summary>
        /// Từ chối đơn hàng
        /// </summary>
        public async Task<bool> RejectAsync(int orderID)
        {
            using var connection = await OpenConnectionAsync();
            // Chỉ từ chối được đơn Mới hoặc Đã duyệt (chưa giao)
            var sql = @"update Orders
                        set Status = @NewStatus, FinishedTime = getdate()
                        where OrderID = @OrderID and (Status = @StatusInit or Status = @StatusAccepted)";

            var parameters = new
            {
                OrderID = orderID,
                NewStatus = Constants.ORDER_REJECTED,     // -2
                StatusInit = Constants.ORDER_INIT,        // 1
                StatusAccepted = Constants.ORDER_ACCEPTED // 2
            };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Lấy danh sách đơn hàng của riêng một khách hàng (Dùng cho Web Shop)
        /// </summary>
        public async Task<IEnumerable<Order>> GetByCustomerAsync(int customerID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"select  o.*,
                        s.ShipperName,
                        s.Phone as ShipperPhone        
                from    Orders as o
                        left join Shippers as s on o.ShipperID = s.ShipperID
                where   o.CustomerID = @CustomerID
                order by o.OrderTime desc"; // Mới nhất lên đầu
            var parameters = new { CustomerID = customerID };
            return await connection.QueryAsync<Order>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
        }
        public async Task<bool> UpdateEmployee(int employeeID,int orderID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"update Orders
                        set EmployeeID = @EmployeeID
                        where OrderID = @OrderID";
            var parameters = new {EmployeeID = employeeID, OrderID = orderID };
            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: System.Data.CommandType.Text)) > 0;
        }
        /// <summary>
        /// Lấy thống kê mua hàng của một khách hàng
        /// </summary>
        public async Task<CustomerStatistics> GetCustomerStatsAsync(int customerID)
        {
            using var connection = await OpenConnectionAsync();

            // Câu lệnh SQL thực hiện 2 việc:
            // 1. Đếm số lượng đơn theo trạng thái
            // 2. Tính tổng tiền đã chi (chỉ tính đơn Status = 4: Hoàn tất)
            var sql = @"
                SELECT 
                    COUNT(*) as TotalOrders,
                    SUM(CASE WHEN Status IN (1, 2, 3) THEN 1 ELSE 0 END) as WaitingOrders,
                    SUM(CASE WHEN Status = 4 THEN 1 ELSE 0 END) as FinishedOrders,
                    SUM(CASE WHEN Status IN (-1, -2) THEN 1 ELSE 0 END) as CancelledOrders,
                    
                    ISNULL((
                        SELECT SUM(od.Quantity * od.SalePrice)
                        FROM OrderDetails od 
                        JOIN Orders o2 ON od.OrderID = o2.OrderID
                        WHERE o2.CustomerID = @CustomerID AND o2.Status = 4
                    ), 0) as TotalSpent

                FROM Orders
                WHERE CustomerID = @CustomerID";

            var parameters = new { CustomerID = customerID };

            // Dapper sẽ tự động map các cột SQL vào thuộc tính của class CustomerStatistics
            var result = await connection.QueryFirstOrDefaultAsync<CustomerStatistics>(
                sql: sql,
                param: parameters,
                commandType: System.Data.CommandType.Text
            );

            // Nếu chưa có đơn nào, trả về object rỗng (all 0) để không bị null
            return result ?? new CustomerStatistics();
        }
    }
}