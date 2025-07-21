using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Products;
using MarketBal.Repository.PurchaseRP;
using MarketBal.Repository.SuppliersRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static MarketBal.Helper.AppHelper;

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
        #region Requisitions



        #endregion
        
        public async Task<IActionResult> AddRequisitionForm()
        {
            vm.Suppliers = await _supplier.GetSuppliers();
            return View(vm);
        }
        public async Task<IActionResult> Requisitions()
        {
            vm.PurchaseMasters= await repository.GetPurchaseRequisition(AppConstants.PurchaseType.Requisition);
            return View(vm);
        }
        public async Task<IActionResult> AddPurchaseRequisation(PurchaseDataDto model)
        {
            var result = await _purchaseRepository.SavePurchase(model);
            return Json(new {statusCode="200" });
        }
       
        public async Task<IActionResult> EditRequisition(Guid Id)
        {
            var result = await repository.GetSingleRequisition(Id,AppConstants.PurchaseType.Requisition);
            vm.PurchaseMaster = result;
            return View(vm);
        }


        public async Task <IActionResult> PurchaseOrder()
        {
            vm.PurchaseMasters = await repository.GetPurchaseRequisition(AppConstants.PurchaseType.PurchaseOrder);

            return View(vm);
        }
        public async Task<IActionResult> EditPurchaseOrder(Guid Id)
        {
            vm.PurchaseMaster = await repository.GetSingleRequisition(Id, AppConstants.PurchaseType.PurchaseOrder);
            return View(vm);
        }
        public async Task<IActionResult> GoodReceivedNote()
            {
            vm.PurchaseMasters = await repository.GetPurchaseRequisition(AppConstants.PurchaseType.Receiving);

            return View(vm);
        }
        public async Task<IActionResult> EditGoodRecievedNote(Guid Id)
        {
            vm.PurchaseMaster = await repository.GetSingleRequisition(Id, AppConstants.PurchaseType.Receiving);
            return View(vm);
        }

        public async Task<IActionResult> POChangeStatus(PurchaseMasterVM model)
        {
            var result = await repository.UpdatePOSTATUS(model);
            if (result> (int)ErrorCodesForReturn.Success)
            {
                return Ok(new { statusCode = "", Message = "" });


            }
            else if(result== (int)ErrorCodesForReturn.Failure)
            {
                return Ok(new { statusCode="",Message=""});
            }
            else if (result == (int)ErrorCodesForReturn.CrashError)
            {
                return Ok(new { statusCode = "", Message = "" });

            }
            else if (result == (int)ErrorCodesForReturn.Duplicate)
            {
                return Ok(new { statusCode = "", Message = "" });

            }
            else if (result == (int)ErrorCodesForReturn.DbConnection)
            {
                return Ok(new { statusCode = "", Message = "" });

            }

            else
            {
                return BadRequest();
            }

        }
        public async Task<IActionResult> POChangeType(PurchaseMasterVM model)
        {
            var result = await repository.UpdatePOType(model);
            if (result >= (int)ErrorCodesForReturn.Success)
            {
                return Ok(new { statusCode = "200", Message = "Update SuccessFull" });
            }
            else
            {
                return Ok(new { statusCode = "300", Message = "Unable to Update" });
            }

        }
        public async Task<IActionResult> POChangeQty(PurchaseDetailVM model)
        {
            await repository.UpdatePoQTY(model);
            return Ok(new { statusCode="200"});
        }
        public async Task<IActionResult> RecieveGRN(PurchaseMasterVM model)
        {
            await repository.CreateGRNNote(model);
            return Ok(new { statusCode = "200",Message="GRN Completed Successfully Items have been added in Inventory" });
        }
    }
}
