namespace SV22T1080053.Admin.Models
{
    /// <summary>
    /// Biểu diễn dữ liệu đầu ra khi tìm kiếm có phân trang (ViewModel)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginationSearchResult<T> where T : class
    {
        /// <summary>
        /// Trang được hiển thị
        /// </summary>
        public int Page { get; set; }
        /// <summary>
        /// Số dòng trên mỗi trang 
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// Giá trị tìm kiếm 
        /// </summary>
        public string SearchValue { get; set; } = "";
        /// <summary>
        /// Số dòng dữ liệu tìm được
        /// </summary>
        public int RowCount { get; set; }
        public int PageCount
        {
            get
            {
                if (PageSize <= 0)
                    return 1;
                int p = RowCount / PageSize;            // 100 dòng, 20 trang thì = 5 
                if (RowCount % PageSize > 0)
                    p += 1;
                return p;
            }
        }
        /// <summary>
        /// Danh sách dữ liệu truy vấn được 
        /// </summary>
        public required IEnumerable<T> Data { get; set; }
    }
}
