using MainModels.DTOModels;
using MarketBal.Repository.DCS;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("[controller]/[action]")]
    public class AttributeManagerController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AttributeRepository _attrib;
        private readonly PagesViewModel vm = new PagesViewModel();
        public AttributeManagerController(IConfiguration config)
        {
            _config = config;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<IActionResult> DCSManager()
        {

            return View(vm);
        }
        public async Task<IActionResult> _GetAddDepartmentForm()
        {
            return PartialView();
        }
        public async Task<IActionResult> AddDepartment(DepartmentVM model)
        {

            var result = await _attrib.AddDepartment(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }
       
        public async Task<IActionResult> _GetAddCategoryForm()
        {
            vm.Departments = await _attrib.GetDepartment();

            return PartialView(vm);
        }
        public async Task<IActionResult> AddCategory(CategoryVM model)
        {

            var result = await _attrib.AddCagtegory(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }
        
        public async Task<IActionResult> _GetAddSubCategoryForm()
        {
            vm.Departments = await _attrib.GetDepartment();

            return PartialView(vm);
        }
        public async Task<IActionResult> AddSubCategory(SubCategoryVM model)
        {

            var result = await _attrib.AddSubCategory(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }

      
        public async Task<IActionResult> _GetAddColorForm()
        {
            return PartialView();
        }
        public async Task<IActionResult> AddColor(ColorVM model)
        {

            var result = await _attrib.AddColor(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }

        public async Task<IActionResult> _GetAddSizeForm()
        {

            return PartialView();
        }
        public async Task<IActionResult> AddSize(SizeVM model)
        {

            var result = await _attrib.AddSize(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }
        
        public async Task<IActionResult> _GetAddUOMForm()
        {
            return PartialView();
        }
        public async Task<IActionResult> AddUOM(UomVM model)
        {

            var result = await _attrib.AddUOM(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }
        
        public async Task<IActionResult> _GetAddSubUOMForm()
        {
            vm.UOMs = await _attrib.GetUOM();

            return PartialView(vm);
        }
        public async Task<IActionResult> AddSubUOM(UomsubVM model)
        {

            var result = await _attrib.AddSubUOM(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }

        public async Task<IActionResult> _GetAddMaterialForm()
        {
            return PartialView();
        }
        public async Task<IActionResult> AddMaterial(MaterialVM model)
        {

            var result = await _attrib.AddMaterial(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }



        public async Task<IActionResult> _GetAddBrandForm()
        {
            return PartialView();
        }
        public async Task<IActionResult> AddBrand(BrandVM model)
        {

            var result = await _attrib.AddBrand(model);
            if (result == 1)
            {
                return Json(new { statusCode = "200" });
            }
            else if (result == -1)
            {
                return Json(new { statusCode = "300" });
            }
            else
            {
                return Json(new { statusCode = "400" });
            }
        }



















        #region GetDCSValues
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _attrib.GetDepartment();
            return Json(departments);
        }
        public async Task<IActionResult> GetCategorybyDepartment(CategoryVM model)
        {
            var categories = await _attrib.GetCategory(model.DepartmentId);
            return Json(categories);
        }
        public async Task<IActionResult> GetSubCategorybyCategory(SubCategoryVM model)
        {
            var subcategories = await _attrib.GetSubCategory(model.CategoryId);
            return Json(subcategories);
        }

        #endregion
    }
}
