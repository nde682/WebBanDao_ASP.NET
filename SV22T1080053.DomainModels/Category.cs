using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SV22T1080053.DomainModels
{
    /// <summary>
    /// Giỏ hàng
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Mã giỏ hàng
        /// </summary>
        public int CategoryID { get; set; }
        /// <summary>
        /// Tên giỏ hàng
        /// </summary>
        public string CategoryName { get; set; } = "";
        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; } = "";

    }
}
