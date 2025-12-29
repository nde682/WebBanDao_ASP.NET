using SV22T1080053.DataLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.BussinessLayers
{
    /// <summary>
    /// Cung cấp tính năng giao tiếp và xử lý dữ liệu chung cho các tầng dịch vụ khác ( Pro, Sup, Cus, Ship, Em, Cate)
    /// </summary>
    public static class CommonDataService
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly ProvinceDAL provinceDB;
        private static readonly SupplierDAL supplierDB;
        private static readonly CustomerDAL customerDB;
        private static readonly ShipperDAL shipperDB;
        private static readonly EmployeeDAL employeeDB;
        private static readonly CategoryDAL categoryDB;
        /// <summary>
        /// ctor
        /// </summary>
        static CommonDataService()
        {
            provinceDB = new ProvinceDAL(Configuration.ConnectionString);
            supplierDB = new SupplierDAL(Configuration.ConnectionString);
            customerDB = new CustomerDAL(Configuration.ConnectionString);
            shipperDB = new ShipperDAL(Configuration.ConnectionString);
            employeeDB = new EmployeeDAL(Configuration.ConnectionString);
            categoryDB = new CategoryDAL(Configuration.ConnectionString);
        }
        /// <summary>
        /// Dữ liệu tỉnh thành
        /// </summary>
        public static ProvinceDAL ProvinceDB => provinceDB;     // get ...
        /// <summary>
        /// Dữ liệu nhà cung cấp
        /// </summary>
        public static SupplierDAL SupplierDB => supplierDB;
        /// <summary>
        /// Dữ liệu khách hàng
        /// </summary>
        public static CustomerDAL CustomerDB => customerDB;
        /// <summary>
        /// Dữ liệu người giao hàng 
        /// </summary>
        public static ShipperDAL ShipperDB => shipperDB;
        /// <summary>
        /// Dữ liệu nhân viên
        /// </summary>
        public static EmployeeDAL EmployeeDB => employeeDB;
        /// <summary>
        /// Dữ liệu loại hàng
        /// </summary>
        public static CategoryDAL CategoryDB => categoryDB;

        public static object ProductDB { get; set; }
    }
}
