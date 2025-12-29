using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SV22T1080053.DataLayers
{
    /// <summary>
    /// Lớp cơ sở cho các lớp xử lý dữ liệu trên CSDL SQL sever
    /// </summary>
    public abstract class BaseDAL
    {
        protected string connectionString;
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="connectionString">Chuỗi tham số kết nối đến csdl</param>
        public BaseDAL(string connectionString)
        {
            this.connectionString = connectionString;
        }
        /// <summary>
        /// Mở kết nối đến cơ sở dữ liệu
        /// </summary>
        /// <returns></returns>
        protected SqlConnection OpenConnection() 
        { 
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString=connectionString;
            connection.Open();
            return connection;
        }
        /// <summary>
        /// mở kết nối đến csdl bất đồng bộ
        /// </summary>
        /// <returns></returns>
        protected async Task<SqlConnection> OpenConnectionAsync()
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString=connectionString;
            await connection.OpenAsync();
            return connection;
        }

    }
}
