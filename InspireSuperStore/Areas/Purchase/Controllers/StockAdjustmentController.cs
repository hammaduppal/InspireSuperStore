using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.PurchaseRP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InspireSuperStore.Areas.Purchase.Controllers
{
    [Area("Purchase")]
    [Route("[controller]/[action]")]
    public class StockAdjustmentController : Controller
    {
        private readonly IConfiguration _config;
        private readonly OneDb _oneDb;
        private readonly ISessionService _sessionService;
        PagesViewModel vm = new PagesViewModel();
        private readonly PurchaseRepository purchaseRepo;
        public StockAdjustmentController(IConfiguration config, OneDb onedb, ISessionService sessionService)
        {
            _config = config;
            _oneDb = onedb;
            _sessionService = sessionService;
            purchaseRepo = new PurchaseRepository(_config, _oneDb, _sessionService);
        }
        public async Task<IActionResult> OpeningStock()
        {

            try
            {
                var res = await purchaseRepo.GetPurchaseRequisition(AppConstants.PurchaseType.OpeningStock);
                vm.PurchaseMasters = res;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while loading stock adjustments.";
            }

            return View(vm);
        }
        public async Task<IActionResult> CreateVoucher()
        {
            vm.PurchaseTypes = await purchaseRepo.GetStockAdjustTypes();
            return View(vm);
        }
        public async Task<IActionResult> CreateStockVoucherSubmit(PurchaseDataDto model)
        {
            var resp = await purchaseRepo.SaveOpeningStock(model);
            return Json(new { statusCode = "200", Message = "Voucher Created Successfully" });
        }






    }
}
