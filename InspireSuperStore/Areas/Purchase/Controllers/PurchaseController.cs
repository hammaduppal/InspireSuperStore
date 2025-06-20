using MainModels.DTOModels;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Purchases.Controllers
{
    [Area("Purchase")]
    [Route("[controller]/[action]")]
    public class PurchaseController : Controller
    {
        private readonly PagesViewModel vm;
        public PurchaseController()
        {
            vm = new PagesViewModel();
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
    }
}
