using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using MarketBal.Repository.POSManager;
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
        private readonly AssetRepository _assets;
        private readonly HumanRespourceRepository _hrm;
        private readonly POSRepository _posRepo;
        public POSManagerController(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _oneDb = oneDb;
            _attrib = new AttributeRepository(_config);
            _account = new AccountRepository(_config);
            _admin = new AdminPanelRepository(_config,_oneDb);
            _assets = new AssetRepository(_config, _oneDb);
            _hrm = new HumanRespourceRepository(_config, _oneDb);
            _posRepo = new POSRepository(_config, _oneDb);
        }
        public async Task<IActionResult> CreateInvoice()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.Countries = await _admin.Countries();
            vm.Customers = await _admin.Customers();
            vm.ServingTables = await _assets.ServingTables();
            vm.Employees = await _hrm.GetSaleStaff();
            vm.PaymentMethods = await _assets.PaymentMethods();
            return View(vm);
        }
        public async Task<IActionResult> SaveInvoice([FromForm]InvoiceMasterVM model)
        {
            var result = await _posRepo.SaveInvoice(model);
            return Json(new { success = true, message = "Invoice saved successfully!" });
        }
    }
}
