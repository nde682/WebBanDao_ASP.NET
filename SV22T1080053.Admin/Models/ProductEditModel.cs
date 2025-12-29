using SV22T1080053.DomainModels;

namespace SV22T1080053.Admin.Models
{
    /// <summary>
    /// ViewModel dùng cho chức năng bổ sung/cập nhật Product
    /// </summary>
    public class ProductEditModel : Product
    {
        public IFormFile? UploadPhoto { get; set; }
    }
}
