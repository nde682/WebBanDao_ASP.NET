using SV22T1080053.DomainModels;

namespace SV22T1080053.Admin.Models
{
    /// <summary>
    /// ViewModel dùng cho chức năng bổ sung/cập nhật Employee
    /// </summary>
    public class EmployeeEditModel : Employee
    {
        public IFormFile? UploadPhoto { get; set; }
    }
}
