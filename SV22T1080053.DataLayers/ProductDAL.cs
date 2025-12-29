using Dapper;
using SV22T1080053.DomainModels;
using System.Data;

namespace SV22T1080053.DataLayers
{
    public class ProductDAL : BaseDAL
    {
        public ProductDAL(string connectionString) : base(connectionString) { }

        // --- PHẦN 1: XỬ LÝ MẶT HÀNG (PRODUCT) ---
        /// <summary>
        /// Tìm kiếm và lấy danh sách mặt hàng dưới dạng phân trang
        /// </summary>
        /// <param name="page">Trang cần hiển thị</param>
        /// <param name="pageSize">Số dòng mỗi trang</param>
        /// <param name="searchValue">Giá trị cần tìm kiếm</param>
        /// <param name="categoryID">Mã loại hàng từ bộ lọc</param>
        /// <param name="supplierID">Mã nhà cung cấp từ bộ lọc</param>
        /// <param name="minPrice">GIá nhỏ nhất cần tìm</param>
        /// <param name="maxPrice">Giá lớn nhất cần tìm</param>
        /// <returns></returns>
        public async Task<IEnumerable<Product>> ListAsync(int page = 1, int pageSize = 0,
                                                          string searchValue = "", int categoryID = 0, int supplierID = 0,
                                                          decimal minPrice = 0, decimal maxPrice = 0)
        {
            if (page < 1) page = 1;
            if (pageSize < 0) pageSize = 0;
            searchValue = $"%{searchValue}%";

            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"WITH cte AS
                            (
                                SELECT  *,
                                        ROW_NUMBER() OVER(ORDER BY ProductName) AS RowNumber
                                FROM    Products 
                                WHERE   (@searchValue = N'%%' OR ProductName LIKE @searchValue)
                                AND     (@categoryID = 0 OR CategoryID = @categoryID)
                                AND     (@supplierID = 0 OR SupplierID = @supplierID)
                                AND     (@minPrice = 0 OR Price >= @minPrice)
                                AND     (@maxPrice = 0 OR Price <= @maxPrice)
                            )
                            SELECT * FROM cte
                            WHERE   (@PageSize = 0)
                            OR      (RowNumber BETWEEN (@page - 1)* @pageSize + 1 AND @page * @pageSize) 
                            ORDER BY RowNumber;";

                var parameters = new { page, pageSize, searchValue, categoryID, supplierID, minPrice, maxPrice };
                return await connection.QueryAsync<Product>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Tìm kiếm, lọc và sắp xếp danh sách sản phẩm (Dùng cho trang Shop)
        /// </summary>
        public async Task<IEnumerable<Product>> ListWithSortAsync(
            int page = 1,
            int pageSize = 0,
            string searchValue = "",
            int categoryID = 0,
            int supplierID = 0,
            decimal minPrice = 0,
            decimal maxPrice = 0,
            string sortBy = "")
        {
            // Xử lý từ khóa tìm kiếm
            searchValue = !string.IsNullOrEmpty(searchValue) ? $"%{searchValue}%" : "";

            // Xử lý logic sắp xếp (Mapping từ string sang SQL)
            // Việc này giúp tránh lỗi SQL Injection thay vì nối chuỗi trực tiếp từ người dùng
            string orderBy = "ProductID DESC"; // Mặc định: Sản phẩm mới nhất lên đầu

            switch (sortBy)
            {
                case "price_asc":
                    orderBy = "Price ASC"; // Giá thấp -> cao
                    break;
                case "price_desc":
                    orderBy = "Price DESC"; // Giá cao -> thấp
                    break;
                case "name_asc":
                    orderBy = "ProductName ASC"; // Tên A -> Z
                    break;
                case "name_desc":
                    orderBy = "ProductName DESC"; // Tên Z -> A
                    break;
                case "oldest":
                    orderBy = "ProductID ASC"; // Cũ nhất (ID nhỏ)
                    break;
                case "newest":
                    orderBy = "ProductID DESC"; // Mới nhất (ID lớn)
                    break;
                default:
                    orderBy = "ProductID DESC";
                    break;
            }

            using (var connection = await OpenConnectionAsync())
            {
                // Sử dụng CTE và ROW_NUMBER để phân trang kết hợp sắp xếp
                // {orderBy} được nối chuỗi an toàn vì ta đã kiểm soát giá trị ở switch-case trên
                var sql = $@"
                    WITH cte AS
                    (
                        SELECT  *,
                                ROW_NUMBER() OVER(ORDER BY {orderBy}) AS RowNumber
                        FROM    Products 
                        WHERE   (@searchValue = N'' OR ProductName LIKE @searchValue)
                            AND (@categoryID = 0 OR CategoryID = @categoryID)
                            AND (@supplierID = 0 OR SupplierID = @supplierID)
                            AND (@minPrice = 0 OR Price >= @minPrice)
                            AND (@maxPrice = 0 OR Price <= @maxPrice)
                    )
                    SELECT * FROM cte
                    WHERE  (@pageSize = 0) 
                        OR (RowNumber BETWEEN (@page - 1) * @pageSize + 1 AND @page * @pageSize)
                    ORDER BY RowNumber";

                var parameters = new
                {
                    page,
                    pageSize,
                    searchValue,
                    categoryID,
                    supplierID,
                    minPrice,
                    maxPrice
                };

                return await connection.QueryAsync<Product>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }

        public async Task<int> CountAsync(string searchValue = "", int categoryID = 0, int supplierID = 0,
                                          decimal minPrice = 0, decimal maxPrice = 0)
        {
            searchValue = $"%{searchValue}%";
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT COUNT(*) FROM Products 
                            WHERE   (@searchValue = N'%%' OR ProductName LIKE @searchValue)
                            AND     (@categoryID = 0 OR CategoryID = @categoryID)
                            AND     (@supplierID = 0 OR SupplierID = @supplierID)
                            AND     (@minPrice = 0 OR Price >= @minPrice)
                            AND     (@maxPrice = 0 OR Price <= @maxPrice)";
                var parameters = new { searchValue, categoryID, supplierID, minPrice, maxPrice };
                return await connection.ExecuteScalarAsync<int>(sql: sql, param: parameters, commandType: CommandType.Text);
            }
        }
        /// <summary>
        /// Lấy thông tin 1 mặt hàng qua id
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public async Task<Product?> GetAsync(int productID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM Products WHERE ProductID = @productID";
                return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
            }
        }
        /// <summary>
        /// Bổ sung 1 mặt hàng mới
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Product data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"INSERT INTO Products(ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                            VALUES(@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                            SELECT SCOPE_IDENTITY();";
                return await connection.ExecuteScalarAsync<int>(sql, data);
            }
        }
        /// <summary>
        /// Cập nhật mặt hàng
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<bool> UpdateAsync(Product data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE Products 
                            SET ProductName = @ProductName, ProductDescription = @ProductDescription,
                                SupplierID = @SupplierID, CategoryID = @CategoryID,
                                Unit = @Unit, Price = @Price, Photo = @Photo, IsSelling = @IsSelling
                            WHERE ProductID = @ProductID";
                return await connection.ExecuteAsync(sql, data) > 0;
            }
        }
        /// <summary>
        /// Xoá mặt hàng qua id
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(int productID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                // Xóa ảnh và thuộc tính trước (hoặc dùng Cascade Delete trong DB)
                await connection.ExecuteAsync("DELETE FROM ProductPhotos WHERE ProductID = @productID", new { productID });
                await connection.ExecuteAsync("DELETE FROM ProductAttributes WHERE ProductID = @productID", new { productID });

                var sql = @"DELETE FROM Products WHERE ProductID = @productID";
                return await connection.ExecuteAsync(sql, new { productID }) > 0;
            }
        }
        /// <summary>
        /// Kiểm tra tình trạng qua id
        /// </summary>
        /// <param name="productID"></param>
        /// <returns></returns>
        public async Task<bool> InUsed(int productID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"IF EXISTS(SELECT 1 FROM OrderDetails WHERE ProductID = @productID) SELECT 1 ELSE SELECT 0";
                return await connection.ExecuteScalarAsync<bool>(sql, new { productID });
            }
        }

        // --- PHẦN 2: XỬ LÝ ẢNH (PRODUCT PHOTOS) ---

        public async Task<IEnumerable<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM ProductPhotos WHERE ProductID = @productID ORDER BY DisplayOrder";
                return await connection.QueryAsync<ProductPhoto>(sql, new { productID });
            }
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM ProductPhotos WHERE PhotoID = @photoID";
                return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
            }
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
                    DECLARE @InputOrder INT = ISNULL(@DisplayOrder, 0); 
                    IF @InputOrder < 0 SET @InputOrder = 0;
                    DECLARE @FinalOrder INT;
                    IF EXISTS (SELECT 1 FROM ProductPhotos WHERE ProductID = @ProductID AND DisplayOrder = @InputOrder)
                    BEGIN
                        SELECT @FinalOrder = ISNULL(MAX(DisplayOrder), 0) + 1 
                        FROM ProductPhotos 
                        WHERE ProductID = @ProductID;
                    END
                    ELSE
                    BEGIN
                        SET @FinalOrder = @InputOrder;
                    END

                    INSERT INTO ProductPhotos(ProductID, Photo, Description, DisplayOrder, IsHidden)
                    VALUES(@ProductID, @Photo, @Description, @FinalOrder, @IsHidden);

                    SELECT SCOPE_IDENTITY();";
                return await connection.ExecuteScalarAsync<long>(sql, data);
            }
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE ProductPhotos 
                            SET Photo = @Photo, Description = @Description, DisplayOrder = @DisplayOrder, IsHidden = @IsHidden
                            WHERE PhotoID = @PhotoID";
                return await connection.ExecuteAsync(sql, data) > 0;
            }
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @photoID";
                return await connection.ExecuteAsync(sql, new { photoID }) > 0;
            }
        }

        // --- PHẦN 3: XỬ LÝ THUỘC TÍNH (PRODUCT ATTRIBUTES) ---

        public async Task<IEnumerable<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @productID ORDER BY DisplayOrder";
                return await connection.QueryAsync<ProductAttribute>(sql, new { productID });
            }
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @attributeID";
                return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
            }
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"
                    DECLARE @InputOrder INT = ISNULL(@DisplayOrder, 0); 
                    IF @InputOrder < 0 SET @InputOrder = 0;
                    DECLARE @FinalOrder INT;
                    IF EXISTS (SELECT 1 FROM ProductAttributes WHERE ProductID = @ProductID AND DisplayOrder = @InputOrder)
                    BEGIN
                        SELECT @FinalOrder = ISNULL(MAX(DisplayOrder), 0) + 1 
                        FROM ProductAttributes 
                        WHERE ProductID = @ProductID;
                    END
                    ELSE
                    BEGIN
                        SET @FinalOrder = @InputOrder;
                    END
                    INSERT INTO ProductAttributes(ProductID, AttributeName, AttributeValue, DisplayOrder)
                    VALUES(@ProductID, @AttributeName, @AttributeValue, @FinalOrder);

                    SELECT SCOPE_IDENTITY();";
                return await connection.ExecuteScalarAsync<long>(sql, data);
            }
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"UPDATE ProductAttributes 
                            SET AttributeName = @AttributeName, AttributeValue = @AttributeValue, DisplayOrder = @DisplayOrder
                            WHERE AttributeID = @AttributeID";
                return await connection.ExecuteAsync(sql, data) > 0;
            }
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using (var connection = await OpenConnectionAsync())
            {
                var sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @attributeID";
                return await connection.ExecuteAsync(sql, new { attributeID }) > 0;
            }
        }
    }
}