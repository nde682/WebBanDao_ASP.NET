using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.Admin.Models;
using SV22T1080053.DataLayers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> TestData()
        {
            string connectionString = "Server=WINDOWS-PC;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;";
            var dal = new ProvinceDAL(connectionString);
            var dal2 = new CustomerDAL(connectionString);
            return Json((await dal2.ListAsync()).ToList());
        }
    }
}
