using MainModels.DTOModels;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.POS.Controllers
{
    [Area("POS")]
    [Route("[controller]/[action]")]
    
    public class POSManagerController : Controller
    {
        PagesViewModel vm = new PagesViewModel();
        private readonly AttributeRepository _attrib;
        private readonly IConfiguration _config;
        public POSManagerController(IConfiguration config)
        {
            _config = config;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<IActionResult> CreateInvoice()
        {
            vm.Departments = await _attrib.GetDepartment();
            return View(vm);
        }
    }
}
