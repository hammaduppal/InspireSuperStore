using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Purchases.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Purchase + "," + UserRolesConstants.PowerUser)]
    [Area("Purchase")]
    [Route("[controller]/[action]")]
    public class PurchaseController : Controller
    {
        private readonly PagesViewModel vm;
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        private readonly AdminPanelRepository _admin;
        private readonly OneDb _one;
        public PurchaseController(IConfiguration config, OneDb one)
        {
            _one = one;

            vm = new PagesViewModel();
            _config = config;
            _product = new ProductRepository(_config);
            _admin = new AdminPanelRepository(_config, _one);
        }
        public async Task<IActionResult> Requisition()
        {
            vm.Suppliers = await _admin.GetSuppliers();
            return View(vm);
        }
        public IActionResult PurchaseOrder()
        {
            return View(vm);
        }
        public IActionResult GoodReceivedNote()
        {
            return View();
        }
        public async Task<IActionResult> GetVariantbyBarCode(ProductVariantVM model)
        {
            var result = await _product.GetProductVariant(model.BarCode);
            return Json(result);
        }
        public async Task<IActionResult> Suppliers()
        {
            vm.Suppliers = await _admin.GetSuppliers();
            return View(vm);
        }

        public async Task<IActionResult> AddSupplier()
        {
            vm.Countries = await _admin.Countries();
            return View(vm);
        }

        public async Task<IActionResult> AddSupplierForm(SupplierVM model)
        {
            var result = await _admin.AddSupplier(model);
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
