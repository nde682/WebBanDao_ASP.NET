using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.BussinessLayers
{
    /// <summary>
    /// Khởi tạo và lưu thông tin cấu hình cho tầng tác nghiệp - Business Layers
    /// </summary>
    public class Configuration            // public
    {
        private static string connectionString = "";
        /// <summary>
        /// Khởi tạo cấu hình cho tầng tác nghiệp
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            Configuration.connectionString = connectionString;
        }
        /// <summary>
        /// Chuỗi tham số kết nối CSDL 
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }
    }
}
