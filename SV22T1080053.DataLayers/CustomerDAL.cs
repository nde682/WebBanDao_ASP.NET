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
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến khách hàng
    /// </summary>
    public class CustomerDAL : BaseDAL
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public CustomerDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng trên mỗi trang (nếu pageSize = 0 thì không phân trang)</param>
        /// <param name="searchValue">Tên khách hàng cần tìm (rỗng nếu lấy toàn bộ)</param>
        /// <returns></returns>
        public async Task<IEnumerable<Customer>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                            (
	                            SELECT	*,
			                            ROW_NUMBER() OVER(ORDER BY CustomerName) AS RowNumber
	                            FROM	Customers 
	                            WHERE	CustomerName LIKE @searchValue OR ContactName LIKE @searchValue
                            )
                            SELECT * FROM cte
                            WHERE	(@PageSize = 0)
	                            OR	(RowNumber BETWEEN (@page - 1)* @pageSize + 1 AND @page * @pageSize) 
                            ORDER BY RowNumber;";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue = searchValue
                };
                return await connection.QueryAsync<Customer>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Đếm số lượng khách hàng tìm được
        /// </summary>
        /// <param name="searchValue">Tên khách hàng cần tìm (rỗng nếu lấy toàn bộ)</param>
        /// <returns></returns>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Customers 
                            WHERE CustomerName LIKE @searchValue OR ContactName LIKE @searchValue;";
                var parameters = new { searchValue };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Lấy thông tin 1 khách hàng dựa vào mã khách hàng
        /// </summary>
        /// <param name="id">Mã khách hàng</param>
        /// <returns></returns>
        public async Task<Customer?> GetAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Customers WHERE CustomerID = @id";
                var parameters = new { id };
                return await connection.QueryFirstOrDefaultAsync<Customer>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Bổ sung một khách hàng mới. Hàm trả về ID của khách hàng vừa được bổ sung.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Customer data)
        {
            using (var connection = await OpenConnectionAsync())
            {

                var sql = @"INSERT INTO Customers
                            (
	                            CustomerName
	                            ,ContactName
	                            ,Province
	                            ,Address
	                            ,Phone
	                            ,Email
	                            ,IsLocked
                            )
                            VALUES
                            (
	                            @CustomerName
	                            ,@ContactName
	                            ,@Province
	                            ,@Address
	                            ,@Phone
	                            ,@Email
	                            ,@IsLocked
                            );
                            SELECT SCOPE_IDENTITY();";
                var parameters = data;
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Cập nhật thông tin của 1 khách hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Customer data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE	Customers
                            SET		CustomerName = @CustomerName
		                            ,ContactName = @ContactName
		                            ,Province = @Province
		                            ,Address = @Address
		                            ,Phone = @Phone
		                            ,Email = @Email
		                            ,IsLocked = @IsLocked
                            WHERE	CustomerID = @CustomerID";
                var parameters = data;
                return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text)) > 0;
            }
        }
        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        public async Task<bool> UpdateFullAsync(Customer data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"UPDATE Customers 
                        SET CustomerName = @CustomerName,
                            ContactName = @ContactName,
                            Province = @Province,
                            Address = @Address,
                            Phone = @Phone,
                            Photo = @Photo,
                            IsLocked = @IsLocked
                        WHERE CustomerID = @CustomerID";

            var parameters = data;

            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text)) > 0;
        }
        /// <summary>
        /// Xóa 1 khách hàng có mã là id
        /// </summary>
        /// <param name="id">Mã khách hàng cần xóa</param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM Customers WHERE CustomerID = @id";
                var parameters = new { id };
                return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text)) > 0;
            }
        }
        /// <summary>
        /// Kiểm tra xem một khách hàng hiện đang có dữ liệu liên quan hay không?
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<bool> InUsed(int id)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"IF EXISTS(SELECT 1 FROM Orders WHERE CustomerID = @id)
	                            SELECT 1
                            ELSE
	                            SELECT 0;";
                var parameters = new { id };
                return await connection.ExecuteScalarAsync<bool>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
    }
}
