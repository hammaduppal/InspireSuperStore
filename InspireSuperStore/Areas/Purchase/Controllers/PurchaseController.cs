using MainModels.DTOModels;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Purchases.Controllers
{
    [Area("Purchase")]
    [Route("[controller]/[action]")]
    public class PurchaseController : Controller
    {
        private readonly PagesViewModel vm;
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        public PurchaseController(IConfiguration config)
        {

            vm = new PagesViewModel();
            _config = config;
            _product = new ProductRepository(_config);
        }
        public IActionResult Requisition()
        {
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
    }
}
