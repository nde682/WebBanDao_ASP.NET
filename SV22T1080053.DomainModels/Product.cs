using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DomainModels
{
    /// <summary>
    /// Sản phẩm
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên mặt hàng
        /// </summary>
        public string ProductName { get; set; } = "";
        /// <summary>
        /// Mô tả mặt hàng
        /// </summary>
        public string ProductDescription { get; set; } = "";
        /// <summary>
        /// Mã nhà cung cấp
        /// </summary>
        public int SupplierID { get; set; }
        /// <summary>
        /// Mã giỏ hàng
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// Đơn vị
        /// </summary>
        public string Unit { get; set; } = "";
        /// <summary>
        /// Giá
        /// </summary>
        public decimal Price { get; set; }
        /// <summary>
        /// Ảnh
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Mặt hàng có đang bán không
        /// </summary>
        public bool IsSelling { get; set; }
    }
}
