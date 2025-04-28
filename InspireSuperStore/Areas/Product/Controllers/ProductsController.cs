using MainModels.Util;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("[controller]/[action]")]
    public class ProductsController : Controller
    {
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        public ProductsController(IConfiguration config)
        {

            _config = config;
            _product = new ProductRepository(_config);

        }
        public IActionResult Products()
        {
            return View();
        }
        public async Task<IActionResult> GetProducts(DataTableRequest request)
        {
            var res = await _product.GetProducts(request);
            return Json(res);
        }
    }
}
