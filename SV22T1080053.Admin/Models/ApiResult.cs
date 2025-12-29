namespace SV22T1080053.Admin.Models
{   
    /// <summary>
    /// dữ liệu trả về cho các api dưới dạng json
    /// </summary>
    public class ApiResult
    {
        public int Code {  get; set; }
        public string Message { get; set; } = "";
        public object? data { get; set; } = null;
    }
}
