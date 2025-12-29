using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1080053.BussinessLayers;
using SV22T1080053.DomainModels;
using System.Threading.Tasks;

namespace SV22T1080053.Admin.Controllers
{
    [Authorize]
    public class ApiController : Controller
    {

        public async Task<IActionResult> Customer(int id)
        {
            var data = await CommonDataService.CustomerDB.GetAsync(id);
            if (data == null)
            {
                return Json(new Customer());
            }
            return Json(data);
        }
    }
}
