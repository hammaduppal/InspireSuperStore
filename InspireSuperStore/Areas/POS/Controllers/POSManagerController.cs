using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository;
using MarketBal.Repository.Account;
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
        private readonly AccountRepository _account;
        private readonly AdminPanelRepository _admin;
        private readonly OneDb _oneDb;
        public POSManagerController(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _oneDb = oneDb;
            _attrib = new AttributeRepository(_config);
            _account = new AccountRepository(_config);
            _admin = new AdminPanelRepository(_config,_oneDb);
        }
        public async Task<IActionResult> CreateInvoice()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.Countries = await _admin.Countries();
            vm.Customers = await _admin.Customers();
            return View(vm);
        }
    }
}
