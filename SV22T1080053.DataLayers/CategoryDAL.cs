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
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến danh mục
    /// </summary>
    public class CategoryDAL : BaseDAL
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="connectionString"></param>
        public CategoryDAL(string connectionString) : base(connectionString)
        {
        }
        /// <summary>
        /// // Lấy danh sách danh mục dưới dạng phân trang và có thể tìm kiếm
        /// /
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Category>> ListAsync(int page = 1, int pageSize = 0, string searchValue = "")
        {
            string searchPattern = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"WITH cte AS
                (
                SELECT *, 
                ROW_NUMBER() OVER(ORDER BY CategoryName) AS RowNumber
                    FROM Categories
                WHERE CategoryName LIKE @searchValue
                )
                    SELECT * FROM cte
                    WHERE (@pageSize = 0)
                    OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                    ORDER BY RowNumber;";
            var parameters = new
            {
                page, // ten tham so truyen vao phai giong ten bien trong cau lenh SQL
                pageSize,
                searchValue = searchPattern
            };
            // Thuc thi cau lenh SQL
            return await connection.QueryAsync<Category>(sql, parameters, commandType: System.Data.CommandType.Text);
        }

        /// <summary>
        /// // Đếm số lượng
        /// /// </summary>
        public async Task<int> CountAsync(string searchValue = "")
        {
            string searchPattern = $"%{searchValue}%";
            using var connection = await OpenConnectionAsync();
            var sql = @"
                 Select Count(*) From Categories
                 Where CategoryName like @searchValue or Description like @searchValue;";
            var parameters = new
            {
                searchValue = searchPattern
            };
            // Thuc thi cau lenh SQL
            return await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
        }
        /// <summary>
        /// // kiem tra danh muc có đang được sử dụng ở bảng khác hay không
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        /// </summary>
        public async Task<bool> InUsedAsync(int categoryID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"If exists(Select * From Products where CategoryID=@categoryID) 
                    select 1 else select 0";
            var parameters = new
            {
                categoryID
            };
            // Thuc thi cau lenh SQL
            int result = await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
            return result > 0;
        }

        /// <summary>
        /// // Thêm một danh mục, trả về mã danh mục được tạo mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Insert into Categories(CategoryName, Description)
                    Values(@CategoryName, @Description);
                    Select SCOPE_IDENTITY();";
            var parameters = new
            {
                data.CategoryName,
                data.Description
            };
            // Thuc thi cau lenh SQL
            int result = await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: System.Data.CommandType.Text);
            return result;
        }

        /// <summary>
        /// // Cập nhật thông tin một danh mục
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Update Categories
                    Set CategoryName=@CategoryName,
                        Description=@Description
                    Where CategoryID=@CategoryID";
            var parameters = new
            {
                data.CategoryID,
                data.CategoryName,
                data.Description
            };
            // Thuc thi cau lenh SQL
            int result = await connection.ExecuteAsync(sql, parameters, commandType: System.Data.CommandType.Text);
            return result > 0;
        }

        /// <summary>
        /// // Xóa một danh mục
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int categoryID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Delete from Categories
                    Where CategoryID=@categoryID";
            var parameters = new
            {
                categoryID
            };
            // Thuc thi cau lenh SQL
            int result = await connection.ExecuteAsync(sql, parameters, commandType: System.Data.CommandType.Text);
            return result > 0;
        }

        /// <summary>
        /// // Lấy thông tin của một danh mục dựa vào mã
        /// </summary>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public async Task<Category?> GetAsync(int categoryID)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"Select * from Categories
                    Where CategoryID=@categoryID";
            var parameters = new
            {
                categoryID
            };
            // Thuc thi cau lenh SQL
            return await connection.QueryFirstOrDefaultAsync<Category>(sql, parameters, commandType: System.Data.CommandType.Text);
        }

    }
}
