using MainModels.DTOModels;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Product.Controllers
{
    public partial class ProductsController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> GetVariantbyBarCode(ProductVariantVM model)
        {
            var result = await _product.GetProductVariant(model.BarCode);

            return Json(result);
        }
        public async Task<IActionResult> SearchProducts(ProductSearchVM model)
        {
            var result = await _product.SearchProducts(model);
            return Json(result);
        }
        public async Task<IActionResult> GetProductBySubCategoryId(SubCategoryVM model)
        {
            var result = await _product.ProductsBySubCategories(model.SubCategoryId);
            return Json(result);
        }

    }
}
