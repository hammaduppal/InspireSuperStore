using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Products;
using MarketBal.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarketBal.Repository.SuppliersRP;

namespace InspireSuperStore.Areas.Purchase.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.Purchase + "," + UserRolesConstants.PowerUser)]
    [Area("Purchase")]
    [Route("[controller]/[action]")]
    public class SupplierController : Controller
    {
        private readonly PagesViewModel vm;
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        private readonly SupplierRepository repo;
        private readonly AdminPanelRepository _admin;
        private readonly ISessionService _sessionService;
        private readonly OneDb _one;
        public SupplierController(IConfiguration config, OneDb one, ISessionService sessionService)
        {
            _one = one;
            _sessionService = sessionService;

            vm = new PagesViewModel();
            _config = config;
            _product = new ProductRepository(_config, _one, _sessionService);
            repo = new SupplierRepository(_config, _one, _sessionService);
            _admin = new AdminPanelRepository(_config, _one, _sessionService);

        }

        public async Task<IActionResult> Suppliers()
        {
            vm.Suppliers = await repo.GetSuppliers();
            return View(vm);
        }

        public async Task<IActionResult> AddSupplier()
        {
            vm.Countries = await _admin.Countries();
            return View(vm);
        }
        public async Task<IActionResult> AddSupplierForm(SupplierVM model)
        {
            var result = await repo.AddSupplier(model);
            if (result > 0)
            {
                return Json(new { statusCode = "200", Message = "New Supplier Added" });
            }

            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Add New Supplier" });
            }
        }
    }
}
