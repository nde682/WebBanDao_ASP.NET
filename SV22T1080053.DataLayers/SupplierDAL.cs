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
    public class SupplierDAL : BaseDAL
    {
        /// <summary>
        /// Ctor lớp truy cập dữ liệu bảng Nhà cung cấp
        /// </summary>
        /// <param name="connectionString"></param>
        public SupplierDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách nhà cung cấp dưới dạng phân trang
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Supplier>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                                (
                                    SELECT    *,
                                            ROW_NUMBER() OVER(ORDER BY SupplierName) AS RowNumber
                                    FROM    Suppliers
                                    WHERE   SupplierName LIKE @searchValue OR ContactName LIKE @searchValue
                                )
                                SELECT * FROM cte
                                WHERE   (@PageSize = 0) OR
                                        (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                                ORDER BY RowNumber;";
                var parameters = new
                {
                    page = page,
                    pageSize = pageSize,
                    searchValue
                };
                return await connection.QueryAsync<Supplier>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
            }
        }
        /// <summary>
        /// Đếm số nhà cung cấp tìm được
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(String searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Suppliers
                            WHERE   SupplierName LIKE @searchValue OR ContactName LIKE @searchValue";
                var parameters = new
                {
                    searchValue
                };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
            }
        }
        /// <summary>
        /// Lấy thông tin của nhà cung cấp dựa vào mã nhà cung cấp 
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public async Task<Supplier> GetAsync(int supplierID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "SELECT * FROM Suppliers WHERE SupplierID = @supplierID";
                var parameters = new
                {
                    supplierID
                };
                var data = await connection.QueryAsync<Supplier>(sql: sql, param: parameters, commandType: System.Data.CommandType.Text);
                return await connection.QueryFirstOrDefaultAsync<Supplier>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Thêm một nhà cung cấp mới, hàm trả về mã của nhà cung cấp mới được tạo
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Suppliers(SupplierName, ContactName, Address, Province, Phone, Email)
                            VALUES(@SupplierName, @ContactName, @Address, @Province, @Phone, @Email);
                            SELECT SCOPE_IDENTITY();";
                //var parameters = new
                //{
                //    data.SupplierName,
                //    data.ContactName,
                //    data.Address,
                //    data.Province,
                //    data.Phone,
                //    data.Email
                //};
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: data, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Cập nhật thông tin của nhà cung cấp
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Suppliers
                            SET SupplierName = @SupplierName,
                                ContactName = @ContactName,
                                Address = @Address,
                                Provice = @Province,
                                Phone = @Phone,
                                Email = @Email
                            WHERE SupplierID = @SupplierID";
                //var parameters = new
                //{
                //    data.SupplierID,
                //    data.SupplierName,
                //    data.ContactName,
                //    data.Address,
                //    data.Province,
                //    data.Phone,
                //    data.Email
                //};
                int rowsAffected = await connection.ExecuteAsync(sql: sql, param: data, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        /// <summary>
        /// Xóa một nhà cung cấp dựa vào mã nhà cung cấp
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int supplierID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "DELETE FROM Suppliers WHERE SupplierID = @supplierID";
                var parameters = new
                {
                    supplierID
                };
                int rowsAffected = await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
        }
        /// <summary>
        /// Kiểm tra xem nhà cung cấp đang có dữ liệu liên quan hay không 
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public async Task<bool> InUsedAsync(int supplierID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = "SELECT CASE WHEN EXISTS(SELECT * FROM Products WHERE SupplierID = @supplierID) THEN 1 ELSE 0 END";
                var parameters = new
                {
                    supplierID
                };
                int result = await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
                return result > 0;
            }
        }
    }
}
