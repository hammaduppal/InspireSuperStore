using System.Threading.Tasks;
using InspireSuperStore.Areas.Product.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("Product")]
    [Route("Market/[action]")]
    public class MarketController : Controller
    {
        private readonly OneDb _oneDb;
        private readonly IConfiguration _config;
        private readonly MarketRepository repository;
        private readonly AttributeRepository _attributeRepository;
        PagesViewModel vm = new PagesViewModel();
        private readonly ISessionService _sessionService;
        public MarketController( OneDb oneDb, IConfiguration config, ISessionService sessionService)
        {
            _oneDb = oneDb;
            _config = config;
            _sessionService = sessionService;
            repository = new MarketRepository(_oneDb);
            _attributeRepository = new AttributeRepository(_config, _oneDb, _sessionService);

        }
        public async Task<IActionResult> Coupons()
        {

            vm.Coupons = await repository.GetAllCoupons();
            return View(vm);
        }
        public IActionResult _AddCouponView()
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
            vm.Departments = await _attributeRepository.GetDepartment();
            vm.Coupon = await repository.GetCoupon(CouponId);
            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AssignVariants(Guid couponId, List<Guid> variantIds)
        {
            var result = await repository.AssignVariants(couponId, variantIds);
            return Json(new { statusCode = "200" });
        }
        public async Task<IActionResult> GetAssignedVariantIds(CouponVM model)
        {
            var ids = await repository.GetAssignedVariantIds(model.CouponId);
            return Json(ids);
        }
        public async Task<IActionResult> GetAssignedVariants(CouponVM model)
        {
            var data = await repository.GetAssignedVariants(model.CouponId);
            return Json(data);
        }
        [HttpPost]
        public async Task<IActionResult> UnassignVariant(CouponProductVM model)
        {
            var record = await _oneDb.CouponProducts.FindAsync(model.ProductVariantId);

            if (record != null)
            {
                _oneDb.CouponProducts.Remove(record);
                await _oneDb.SaveChangesAsync();
            }

            return Json(new { statusCode = "200" });
        }
    }
}
