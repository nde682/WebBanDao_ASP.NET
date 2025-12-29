using SV22T1080053.DomainModels;

public class ShopSearchModel
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public string SearchValue { get; set; } = "";
    public int CategoryID { get; set; } = 0;
    public decimal MinPrice { get; set; } = 0;
    public decimal MaxPrice { get; set; } = 0;
    public string SortBy { get; set; } = ""; // <--- Thêm mới

    public int RowCount { get; set; }
    public int PageCount { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}