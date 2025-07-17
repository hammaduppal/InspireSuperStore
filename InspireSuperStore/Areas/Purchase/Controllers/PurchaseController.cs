using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Products;
using MarketBal.Repository.PurchaseRP;
using MarketBal.Repository.SuppliersRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Purchases.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Purchase + "," + UserRolesConstants.PowerUser)]
    [Area("Purchase")]
    [Route("[controller]/[action]")]


    public class PurchaseController : Controller
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly PagesViewModel vm;
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        private readonly SupplierRepository _supplier;
        private readonly PurchaseRepository repository;

        private readonly OneDb _one;
        public PurchaseController(IConfiguration config, OneDb one, IPurchaseRepository purchaseRepository)
        {
            _one = one;
            vm = new PagesViewModel();
            _config = config;
            _product = new ProductRepository(_config);
            _supplier = new SupplierRepository(_config, _one);
            _purchaseRepository = purchaseRepository;
            repository = new PurchaseRepository(_config, _one);
        }
        public async Task<IActionResult> AddRequisitionForm()
        {
            vm.Suppliers = await _supplier.GetSuppliers();
            return View(vm);
        }
        public async Task<IActionResult> Requisitions()
        {
            vm.PurchaseMasters= await _purchaseRepository.GetPurchaseRequisition();
            return View(vm);
        }
        public async Task<IActionResult> AddPurchaseRequisation(PurchaseDataDto model)
        {
            var result = await _purchaseRepository.SavePurchase(model);
            return Json(new { });
        }
       
        public async Task<IActionResult> EditPurchaseOrder(Guid Id)
        {
            var result = await repository.GetSingleRequisition(Id);
            vm.PurchaseMaster = result;
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




    }
}
