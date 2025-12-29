using Dapper;
using SV22T1080053.DomainModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DataLayers
{
    /// <summary>
    /// định nghĩa các phép xử lý dữ liệu liên quan đến người giao hàng
    /// </summary>
    public class ShipperDAL : BaseDAL
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public ShipperDAL(string connectionString) : base(connectionString) { }
        /// <summary>
        /// Tìm kiếm và lấy danh sách dưới dạng phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Shipper>> ListAsync(int page=1,int pageSize=0,string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                                (
                                    SELECT    *,
                                            ROW_NUMBER() OVER(ORDER BY ShipperName) AS RowNumber
                                    FROM    Shippers
                                    WHERE   ShipperName LIKE @searchValue OR Phone LIKE @searchValue
                                )
                                SELECT * FROM cte
                                WHERE   (@PageSize = 0) OR
                                        (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                                ORDER BY RowNumber;";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue
                };
                return await connection.QueryAsync<DomainModels.Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Đếm số lượng kết quả tìm kiếm
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Shippers
                            WHERE ShipperName LIKE @searchValue OR Phone LIKE @searchValue;";
                var parameters = new
                {
                    searchValue
                };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Lấy thông tin 1 người giao hàng bằng shipperID
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public async Task<DomainModels.Shipper> GetAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Shippers
                            WHERE ShipperID = @shipperID;";
                var parameters = new
                {
                    shipperID
                };
                return await connection.QueryFirstOrDefaultAsync<DomainModels.Shipper>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Bổ sung một người giao hàng mới. Hàm trả về ID của người giao hàng vừa được bổ sung.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(DomainModels.Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Shippers (ShipperName, Phone)
                            VALUES (@ShipperName, @Phone);
                            SELECT SCOPE_IDENTITY();";
                var parameters = new
                {
                    data.ShipperName,
                    data.Phone
                };
                var shipperID = await connection.ExecuteScalarAsync<decimal>(sql: sql, param: parameters, commandType: CommandType.Text);
                return (int)shipperID;
            }
        }
        /// <summary>
        /// Cập nhật thông tin một người giao hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(DomainModels.Shipper data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Shippers
                            SET ShipperName = @ShipperName,
                                Phone = @Phone
                            WHERE ShipperID = @ShipperID;";
                var parameters = new
                {
                    data.ShipperID,
                    data.ShipperName,
                    data.Phone
                };
                var rowsAffected = await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        /// <summary>
        /// Xoá một người giao hàng qua shipperID
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Shippers
                            WHERE ShipperID = @shipperID;";
                var parameters = new
                {
                    shipperID
                };
                var rowsAffected = await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        /// <summary>
        /// Kiểm tra xem người giao hàng có tồn tại qua shipperID không
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public async Task<bool> InUseAsync(int shipperID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT CASE WHEN EXISTS
                            (
                                SELECT * FROM Orders
                                WHERE ShipperID = @shipperID
                            )
                            THEN 1 ELSE 0 END;";
                var parameters = new
                {
                    shipperID
                };
                var inUse = await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                return inUse > 0;
            }
        }
    }
}
