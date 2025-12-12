using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.DCS;
using MarketBal.Repository.Products;
using MarketBal.Repository.SystemRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using PuppeteerSharp;
using static MainModels.DTOModels.AppConstants;

namespace InspireSuperStore.Areas.Product.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("Product")]
    [Route("Products/[action]")]
    public partial class ProductsController : Controller
    {
        private readonly ProductRepository _product;
        private readonly IConfiguration _config;
        private readonly DCSRepository _dcs;
        private readonly AttributeRepository _attrib;
        private readonly FileRepository _file;
        private readonly PagesViewModel vm = new PagesViewModel();
        private readonly OneDb oneDb;
        private readonly AdminPanelRepository _admin;
        private readonly SystemRepository _system;
        private readonly IDataRepository _datarepo;
        public ProductsController(IConfiguration config, OneDb oneDb,IDataRepository data)
        {
            this.oneDb = oneDb;

            _config = config;
            _product = new ProductRepository(_config, oneDb);
            _dcs = new DCSRepository(_config);
            _attrib = new AttributeRepository(_config, oneDb);
            _file = new FileRepository();
            _admin = new AdminPanelRepository(_config, oneDb);
            _system = new SystemRepository(_config, oneDb);
            _datarepo = data;
        }
        public async Task<IActionResult> Products()
        {
            vm.Permission = PermissionHelper.Permissions().Where(x => x.URL == "/Product/Products" && x.Module == ModuleList.Product.ToString() && x.Role == "Admin").FirstOrDefault(); ;
            vm.Branches = await _admin.GetBranches(AppDataUtility.SessionUser.Person.Branch.Organization.OrganizationId);

            return View(vm);
        }
        public async Task<IActionResult> GetProducts(DataTableRequest request)
        {
            var res = await _product.GetProducts(request);
            return Json(res);
        }
        [HttpPost]
        public async Task<IActionResult> ActiveUnactive(RequestModel model)
        {
            var res = await _product.ActiveUnActiveProduct(model.ProductId, model.IsActive ? 1 : 0);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });

        }
        public async Task<IActionResult> _AddProductForm()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.UOMs = await _attrib.GetUOM();
            vm.Brands = await _attrib.GetBrands();

            return PartialView(vm);
        }
        public async Task<IActionResult> _AddServiceForm()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.UOMs = await _attrib.GetUOM();
            vm.Brands = await _attrib.GetBrands();

            return PartialView(vm);
        }
        public async Task<IActionResult> AddProduct(ProductVM product)
        {

            if (product.BrandId == null || product.SubCategoryId == null)
            {
                return Json(new { statusCode = "300" });

            }
            product.ProductDescription = await _datarepo.AnalyzeTextAsync(product.ProductName);
            var result = await _product.AddProduct(product,ProductType.PhysicalInventory);

            return Json(new { statusCode = "200", ProductId = result });
        }
        public async Task<IActionResult> AddService(ProductVM product)
        {

            if (product.BrandId == null || product.SubCategoryId == null)
            {
                return Json(new { statusCode = "300" });

            }
            var result = await _product.AddProduct(product,ProductType.ServiceInventory);

            return Json(new { statusCode = "200", ProductId = result });
        }



        public async Task<IActionResult> CreateDescriptionPrompt(ProductVM product)
        {
            var resulttext = await _datarepo.AnalyzeTextAsync(product.ProductName);

            return Json(new { statusCode = 200, Prompt =resulttext});
        }
        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid productId)
        {
            var product = await _product.GetProduct(productId);
            vm.Departments = await _attrib.GetDepartment();
            vm.Brands = await _attrib.GetBrands();
            vm.UOMs = await _attrib.GetUOM();

            vm.Product = product;
            vm.Permission = PermissionHelper.Permissions().Where(x => x.URL == "/Product/Products" && x.Module == ModuleList.Product.ToString() && x.Feature == ModuleList.Category.ToString() && x.UserId == 2).FirstOrDefault(); ;


            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateDescriptionSection(ProductVM product)
        {
            try
            {
                var result = await _product.UpdateDescriptionSection(product);
                if (result == 1)
                {
                    return Json(new { statusCode = "200", Message = "Record Updated Successful" });

                }
                else
                {
                    return Json(new { statusCode = "300", Message = "Unable to Update Record Null or Empty Values are not Allowed" });

                }
            }
            catch (Exception e)
            {

                return Json(new { statusCode = "300", Message = e.Message });
            }

        }
        //[HttpPost]
        //public async Task<IActionResult> _ProductDescriptionPage(RequestModel model)
        //{
        //    var product = await _product.GetProduct(model.ProductId);
        //    vm.Departments = await _attrib.GetDepartment();
        //    vm.Brands = await _attrib.GetBrands();
        //    vm.Product = product;
        //    return PartialView(vm);
        //}
        [HttpPost]
        public async Task<IActionResult> _GetProductImages([FromForm] RequestModel model)
        {
            var res = await _product.GetProductImages(model.ProductId);
            return PartialView(res);
        }
        [HttpPost]
        public async Task<IActionResult> AddProductImages(UploadImage model)
        {
            var uploadResult = await _file.SaveFile(model.File, "Products", "Products");
            if (uploadResult.StatusCode == "200")
            {
                var resu = await _product.SaveProductImage(Guid.Parse(model.Id), uploadResult.ImageUrl);

            }
            return Json(new { statusCode = uploadResult.StatusCode, ProductImageId = model.Id, ImageUrl = uploadResult.ImageUrl, Message = uploadResult.Message });

            //var res = await _product.GetProductImages(Guid.Parse(model.Id));

            //return PartialView("_GetProductImages", res);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateProductDefaultImage(RequestModel model)
        {

            var resu = _product.SetProductDefaultImage(model.ProductImageId, model.ProductId);
            return Json(new { });

        }

        [HttpPost]
        public async Task<IActionResult> _GetProductVariants(RequestModel model)
        {
            vm.SubUOMs = await _attrib.GetSubUOMs(model.UOMId);
            vm.Sizes = await _attrib.GetSizes();
            vm.Colors = await _attrib.GetColors();
            vm.Materials = await _attrib.GetMaterials();
            vm.ProductVariants = await _product.GetProductVariants(model.ProductId);
            vm.TaxSlabs = await _system.TaxSlabs();
            vm.Branches = await _admin.GetBranches(AppDataUtility.SessionUser.Person.Branch.Organization.OrganizationId);
            vm.ProductImages = await _product.GetProductImages(model.ProductId);

            return PartialView(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SaveVariantBranches(ProductVariantVM model)
        {
            var res = await _product.AddProductVariantBranch(model);
            return Json(new { Message = "Branch Stock Updated Successfully!" });
        }
        [HttpPost]
        public async Task<IActionResult> AddProductVariant(ProductVariantVM model)

        {
            var result = await _product.AddProductVariant(model);
            if (result == -1)
            {
                return Json(new { id = result, statusCode = "300" });

            }
            else
            {
                return Json(new { id = result, statusCode = "200" });

            }
        }
        [HttpPost]
        public async Task<IActionResult> SetVariantImage(RequestModel model)
        {
            var result = await _product.SetVariantImage(model.ProductImageId, model.VariantId);
            return Json(new { id = result, statusCode = "200" });

        }

        [HttpPost]
        public async Task<IActionResult> SetPriceFormat(RequestModel model)
        {
            if (Enum.TryParse<EnumPriceFormat>(model.PriceFormat, out var enumValue))
            {
                int intValue = (int)enumValue;
                var result = await _product.SetPriceFormat(intValue, model.VariantId);
                return Json(new { priceFormatInt = intValue });
            }
            else
            {
                return BadRequest("Invalid price format value.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateVariant(UpdateVariantModel model)
        {

            var status = await _product.UpdateVariant(model);
            if (status == 1)
            {
                return Json(new { Message = $"Data Updated for {model.DataType} is Updated", statusCode = "200" });

            }
            else
            {
                return Json(new { Message = $"Unable to Update Data for {model.DataType}", statusCode = "300" });

            }

        }



    }
}
