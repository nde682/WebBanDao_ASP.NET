using Dapper;
using SV22T1080053.DomainModels;
using System.Data;

namespace SV22T1080053.DataLayers
{
    public class CustomerUserAccountDAL : BaseDAL
    {
        public CustomerUserAccountDAL(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Kiểm tra tên đăng nhập (Email) và mật khẩu của khách hàng
        /// </summary>
        public async Task<UserAccount?> AuthenticateAsync(string userName, string password)
        {
            using var connection = await OpenConnectionAsync();
            // UserID lấy từ CustomerID, UserName lấy từ Email
            // RoleNames gán cứng là 'Customer' hoặc để trống
            var sql = @"SELECT  CustomerID AS UserID, 
                                Email AS UserName, 
                                CustomerName AS FullName, 
                                Email, 
                                Photo, 
                                'Customer' AS RoleNames
                        FROM    Customers
                        WHERE   Email = @username AND Password = @password";

            var parameters = new { userName, password };

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql: sql,
                param: parameters,
                commandType: CommandType.Text);
        }

        /// <summary>
        /// Thay đổi mật khẩu khách hàng
        /// </summary>
        public async Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            using var connection = await OpenConnectionAsync();
            var sql = @"UPDATE Customers 
                        SET    Password = @newPassword 
                        WHERE  Email = @userName AND Password = @oldPassword";

            var parameters = new { userName, oldPassword, newPassword };

            return (await connection.ExecuteAsync(sql: sql, param: parameters, commandType: CommandType.Text)) > 0;
        }
        public async Task<int> RegisterAsync(Customer data , string Password, string Photo)
        {
            using var connection = await OpenConnectionAsync();

            var sqlCheck = "SELECT COUNT(*) FROM Customers WHERE Email = @Email";
            var count = await connection.ExecuteScalarAsync<int>(sqlCheck, new { data.Email });

            if (count > 0) return -1;

            // Cập nhật: Thêm cột Photo vào câu lệnh INSERT
            var sql = @"INSERT INTO Customers(CustomerName, ContactName, Province, Address, Phone, Email, Password,IsLocked, Photo)
                        VALUES(@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked, @Photo);
                        
                        SELECT SCOPE_IDENTITY();";

            var parameters = new
            {
                CustomerName = data.CustomerName ?? "",
                ContactName = data.ContactName ?? "",
                Province = data.Province,
                Address = data.Address ?? "",
                Phone = data.Phone ?? "",
                Email = data.Email,
                Password = Password??"123",
                Photo = Photo ?? "",
                IsLocked = false
            };

            var id = await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: CommandType.Text);
            return id;
        }
    }
}