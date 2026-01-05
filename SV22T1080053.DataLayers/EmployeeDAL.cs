using Dapper;
using SV22T1080053.DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DataLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến nhân viên
    /// </summary>
    public class EmployeeDAL : BaseDAL
    {
        public EmployeeDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// // Lấy danh sách nhân viên dưới dạng phân trang và có thể tìm kiếm
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Employee>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 0) page = 0;
            searchValue = $"%{searchValue}%"; // viet chuoi bang dau $ de noi chuoi va 2 dau % de tim kiem gan dung 
            using var connection = await OpenConnectionAsync();
            var sql = @"
                    WITH cte AS
                    (
                        SELECT *,
                               ROW_NUMBER() OVER(ORDER BY FullName) AS RowNumber
                        FROM Employees
                        WHERE FullName LIKE @searchValue
                    )
                    SELECT * FROM cte
                    WHERE (@pageSize = 0)
                       OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                    ORDER BY RowNumber;";
            var parameters = new
            {
                page, // ten tham so truyen vao phai giong ten bien trong cau lenh SQL
                pageSize,
                searchValue
            };
            // Thuc thi cau lenh SQL
            return await connection.QueryAsync<Employee>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// // Đếm số lượng
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<int> CountAsync(string searchValue = "")
        {
            searchValue = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"Select Count(*) From Employees
                    Where FullName like @searchValue or Address like @searchValue;";
            var parameters = new
            {
                searchValue
            };
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// // Lấy thông tin của một nhân viên dựa vào mã nhân viên
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public async Task<Employee?> GetAsync(int employeeID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Select * From Employees
                    Where EmployeeID = @employeeID;";
            var parameters = new
            {
                employeeID
            };
            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// // Thêm một nhân viên. Hàm trả về mã nhân viên được tạo mới.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Insert Into Employees(
                    FullName, 
                    BirthDate, 
                    Address, 
                    Phone, 
                    Email, 
                    Photo, 
                    IsWorking,
                    RoleNames)
                    
                    Values(@FullName, 
                    @BirthDate, 
                    @Address, 
                    @Phone, 
                    @Email, 
                    @Photo, 
                    @IsWorking,
                    @RoleNames);
                    Select SCOPE_IDENTITY();";
            var parameters = new
            {
                data.FullName,
                data.BirthDate,
                data.Address,
                data.Phone,
                data.Email,
                data.Photo,
                data.IsWorking,
                RoleNames="Employee"
            };
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// // Cập nhật thông tin một nhân viên. Hàm trả về true nếu cập nhật thành công.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Update Employees
                    Set FullName = @FullName,
                        BirthDate = @BirthDate,
                        Address = @Address,
                        Phone = @Phone,
                        Email = @Email,
                        Photo = @Photo,
                        IsWorking = @IsWorking
                    Where EmployeeID = @EmployeeID;";
            var parameters = new
            {
                data.EmployeeID,
                data.FullName,
                data.BirthDate,
                data.Address,
                data.Phone,
                data.Email,
                data.Photo,
                data.IsWorking
            };
            return await connection.ExecuteAsync(sql, parameters, commandType: System.Data.CommandType.Text) > 0;
        }
        /// <summary>
        /// // Xóa một nhân viên dựa vào mã nhân viên. Hàm trả về true nếu xóa thành công.
        /// </summary>
        /// <param name="employeeID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int employeeID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Delete From Employees
                    Where EmployeeID = @employeeID;";
            var parameters = new
            {
                employeeID
            };
            return await connection.ExecuteAsync(sql, parameters, commandType: System.Data.CommandType.Text) > 0;
        }
        /// <summary>
        /// 
        /// Kiểm tra xem nhân viên có đang được sử dụng ở bảng Orders hay không
        /// </summary>
        public async Task<bool> InUsedAsync(int employeeID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"If Exists(Select * From Orders Where EmployeeID = @employeeID) select 1
                    Else select 0;";
            var parameters = new
            {
                employeeID
            };
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text) > 0;
        }
        /// <summary>
        /// // Cập nhật ảnh đại diện của nhân viên
        /// </summary>
        /// <param name="employeeID"></param>
        /// <param name="photo"></param>
        /// <returns></returns>
        public async Task<bool> ChangePhotoAsync(int employeeID, string photo)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Update Employees
                    Set Photo = @photo
                    Where EmployeeID = @employeeID;";
            var parameters = new
            {
                employeeID,
                photo
            };
            return await connection.ExecuteAsync(sql, parameters, commandType: System.Data.CommandType.Text) > 0;
        }

    }
}
