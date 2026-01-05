using SV22T1080053.DataLayers;
using SV22T1080053.DomainModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SV22T1080053.BussinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng nghiệp vụ liên quan đến báo cáo
    /// </summary>
    public static class ReportDataService
    {
        private static readonly ReportDAL reportDB;

        /// <summary>
        /// Static Constructor để khởi tạo DAL
        /// </summary>
        static ReportDataService()
        {
            // Sử dụng Configuration.ConnectionString giống CommonDataService
            reportDB = new ReportDAL(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy dữ liệu báo cáo Dashboard
        /// </summary>
        public static async Task<ReportData> GetDashboardReportAsync()
        {
            return await reportDB.GetReportAsync();
        }
    }
}