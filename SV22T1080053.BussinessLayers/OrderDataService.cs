using SV22T1080053.BussinessLayers;
using SV22T1080053.DataLayers.SQLServer;

namespace SV22T1080053.BusinessLayers
{
    /// <summary>
    /// Các chức năng tác nghiệp liên quan đến đơn hàng
    /// </summary>
    public class OrderDataService
    {
        private static readonly OrderDAL orderDB;
        /// <summary>
        /// 
        /// </summary>
        static OrderDataService()
        {
            orderDB = new OrderDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// 
        /// </summary>
        public static OrderDAL OrderDB => orderDB;
    }
}