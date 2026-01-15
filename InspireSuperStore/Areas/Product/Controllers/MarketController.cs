using System.Threading.Tasks;
using InspireSuperStore.Areas.Product.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("Product")]
    [Route("Market/[action]")]
    public class MarketController : Controller
    {
        private readonly OneDb _oneDb;
        private readonly MarketRepository repository;
        PagesViewModel vm = new PagesViewModel();
        public MarketController(OneDb oneDb)
        {

            _oneDb = oneDb;
            repository = new MarketRepository(_oneDb);
        }
        public async Task<IActionResult> Coupons()
        {

            vm.Coupons = await repository.GetAllCoupons();
            return View(vm);
        }
        public async Task<IActionResult> _AddCouponView()
        {
            return PartialView();
        }
        public async Task<IActionResult> CreateCoupon(CouponVM model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors) });
            }
            var result = await repository.CreateCoupon(model);
            if (result)
            {
                return Json(new { statusCode = "200", success = result });

            }
            else
            {
                return Json(new { statusCode = "300", success = result });

            }
        }
        public async Task<IActionResult> _EditCoupon(CouponVM model)
        {
            var coupon = await repository.GetCoupon(model.CouponId);
            vm.Coupon = coupon;
            return PartialView(vm);
        }
        public async Task<IActionResult> EditCoupon([FromForm] CouponVM model)
        {
            var result = await repository.EditCoupon(model);
            if (result)
            {
                return Json(new { statusCode = "200", success = result });

            }
            else
            {
                return Json(new { statusCode = "300", success = result });

            }
        }
        [HttpGet("{CouponId}")]
        public async Task<IActionResult> CouponProduct(Guid CouponId)
        {
            vm.Coupon = await repository.GetCoupon(CouponId);
            return View(vm);
        }

    }
}
