using MainModels.DTOModels;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("[controller]/[action]")]
    public class AttributeManagerController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AttributeRepository _attrib;
        public AttributeManagerController(IConfiguration config)
        {
            _config = config;
            _attrib = new AttributeRepository(_config);
        }
        public IActionResult DCSManager()
        {
            return View();
        }
        public async Task<IActionResult> AddDepartment(DepartmentVM model)
        {
         
            var result = await _attrib.AddDepartment(model);
            if (result==1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result==-1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }
    }
}
