using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DomainModels
{
    /// <summary>
    /// Thuộc tính sản phẩm
    /// </summary>
    public class ProductAttribute
    {
        /// <summary>
        /// Mã thuộc tính mặt hàng
        /// </summary>
        public long AttributeID { get; set; }
        /// <summary>
        /// Mã mặt hàng
        /// </summary>
        public int ProductID { get; set; }
        /// <summary>
        /// Tên thuộc tính
        /// </summary>
        public string AttributeName { get; set; } = "";
        /// <summary>
        /// Giá trị thuộc tính
        /// </summary>
        public string AttributeValue { get; set; } = "";
        /// <summary>
        /// thứ tự hiển thị
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}
