using Microsoft.AspNetCore.Mvc.Rendering;
using SV22T1080053.BussinessLayers;

namespace SV22T1080053.Admin
{
    public class selectListHelper
    {
        /// <summary>
        /// danh sách các tỉnh thành thẻ select
        /// </summary>
        /// <returns></returns>
        public static async Task<IEnumerable<SelectListItem>> Province()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value="",Text="--Chọn tỉnh thành--"});
            foreach (var item in await CommonDataService.ProvinceDB.ListAsync())
            {
                list.Add(new SelectListItem() {Value = item.ProvinceName, Text = item.ProvinceName });
            }
            return list;
        }

        public static async Task<IEnumerable<SelectListItem>> Category()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "0", Text = "--Chọn loại hàng--" });
            foreach (var item in await CommonDataService.CategoryDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.CategoryID.ToString(), Text = item.CategoryName });
            }
            return list;
        }
        public static async Task<IEnumerable<SelectListItem>> Supplier()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "0", Text = "--Chọn loại hàng--" });
            foreach (var item in await CommonDataService.SupplierDB.ListAsync())
            {
                list.Add(new SelectListItem() { Value = item.SupplierID.ToString(), Text = item.SupplierName });
            }
            return list;
        }
        /// <summary>
        /// Lấy danh sách khách hàng đưa vào Dropdown
        /// </summary>
        public static async Task<IEnumerable<SelectListItem>> Customers()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem() { Value = "0", Text = "-- Chọn khách hàng --" });
            var data = await CommonDataService.CustomerDB.ListAsync(1, 0, "");
            foreach (var item in data)
            {
                list.Add(new SelectListItem()
                {
                    Value = item.CustomerID.ToString(),
                    Text = $"{item.CustomerName} ({item.ContactName})" 
                });
            }
            return list;
        }
    }
}
