using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DomainModels
{
    /// <summary>
    /// Hình ảnh mặt hàng
    /// </summary>
    public class ProductPhoto
    {
        /// <summary>
        /// Mã hình ảnh
        /// </summary>
        public long PhotoID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// hình ảnh
        /// </summary>
        public string Photo { get; set; } = "";
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; } = "";
        /// <summary>
        /// thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
        /// <summary>
        /// Hình ảnh sản phẩm có bị ẩn không
        /// </summary>
        public bool IsHidden { get; set; }
    }
}
