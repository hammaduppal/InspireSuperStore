using System.Security.Cryptography.Pkcs;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace InspireSuperStore.Areas.AdminArea.Controllers
{
    [Authorize(Roles = UserRolesConstants.SuperAdmin + "," + UserRolesConstants.Admin)]
    [Area("AdminArea")]
    [Route("[controller]/[action]")]
    public class AdminPanelController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AdminPanelRepository _adminPanel;
        private readonly AccountRepository _account;

        private readonly PagesViewModel vm = new PagesViewModel();
        private readonly OneDb _oneDb;
        public AdminPanelController(IConfiguration config, OneDb oneDb)
        {
            _oneDb = oneDb;
            _config = config;
            _adminPanel = new AdminPanelRepository(_config, _oneDb);
            _account = new AccountRepository(_config);
        }
        #region Users
        public async Task<IActionResult> LoginUser()
        {
            vm.LoginUsers = await _adminPanel.GetLoginUser();

            return View(vm);
        }
        [HttpGet]
        [Route("/AdminPanel/EditUser/{Id}")]
        public async Task<IActionResult> EditUser(int Id)
        {
            vm.LoginUser = await _adminPanel.GetLoginUser(Id);
            vm.Countries = await _adminPanel.Countries();
            vm.Roles = await _adminPanel.GetRoles();
            return View(vm);
        }
        public async Task<IActionResult> AssignUserRoles(AssignRolesVM formData)
        {
            var result = await _adminPanel.UpdateRoles(formData);
            if (result == 1)
            {
                return Json(new { statusCode = "200", Message = "Roles Updated" });
            }
            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Update Roles" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateLoginUser(LoginUserVM formData)
        {
            var result = await _adminPanel.UpdateLoginUser(formData);

            if (result == 1)
            {
                return Json(new { statusCode = "200", Message = "Roles Updated" });
            }
            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Update Roles" });
            }
        }
        public async Task<IActionResult> AddUser()
        {
            vm.Organizations = await _adminPanel.GetOrganizations();
            vm.Countries = await _adminPanel.Countries();
            return View(vm);
        }
        public async Task<IActionResult> AddNewUser(LoginUserVM formData)
        {
            var result = await _adminPanel.AddNewUser(formData);

            if (result > 0)
            {
                return Json(new { statusCode = "200", Message = "New User Added" });
            }
            else if (result == -3)
            {
                return Json(new { statusCode = "201", Message = "User Already Exisits Email Already Taken" });

            }
            else if (result == -4)
            {
                return Json(new { statusCode = "202", Message = "Person is Already Taken, CNIC / Mobile Already Saved" });

            }
            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Add User" });
            }
        }

        public async Task<IActionResult> ActiveUnActiveUser(LoginUserVM formData)
        {
            var result = await _adminPanel.ActiveDeactiveUser(formData);

            if (result > 0)
            {
                return Json(new { statusCode = "200", Message = "User Updated" });
            }

            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Update User" });
            }
        }

        public async Task<IActionResult> UserRoles()
        {
            vm.Roles = await _adminPanel.Roles();
            return View(vm);
        }

        public async Task<IActionResult> AddUserRoles(RolesVM formData)
        {
            var result = await _adminPanel.AddRole(formData);

            if (result > 0)
            {
                return Json(new { statusCode = "200", Message = "New Role Added" });
            }

            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Add New Role" });
            }
        }


        #endregion
        
        #region RegionManagement
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetStatesByCountryId(StateProvinceVM vm)
        {
            var result = await _adminPanel.GetStatesByCountryId(vm.CountryId);
            return Json(result);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetCityByStateId(CityVM vm)
        {
            var result = await _adminPanel.GetCityByStateId(vm.StateProvinceId);
            return Json(result);
        }
        public async Task<IActionResult> UpdateAddress(LaneAddressVM formData)
        {
            var result = await _adminPanel.UpdateAddress(formData);

            if (result > 0)
            {
                return Json(new { statusCode = "200", Message = "Address  Updated" });
            }
            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Update Address" });
            }


        }

        #endregion
     
        #region OrganizationRegion
        public IActionResult Organizations()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetOrganizations(DataTableRequest request)
        {
            var res = await _adminPanel.GetOrganizations(request);
            return Json(res);
        }
        public IActionResult AddOrganizationView(int Id)
        {

            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> AddOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.AddOrganization(model);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });
        }

        public async Task<IActionResult> GetOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.GetOrganization(model.OrganizationId);

            return PartialView(res);
        }
        public async Task<IActionResult> EditOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.GetOrganization(model.OrganizationId);

            return PartialView(res);
        }
        public async Task<IActionResult> UpdateOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.EditOrganization(model);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            else
            {
                return Json(new { statusCode = "300" });

            }
        }
        public async Task<IActionResult> RemoveOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.RemoveOrganization(model.OrganizationId);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });

        }
        public async Task<IActionResult> ActiveUnactiveOrganization(OrganizationVM model)
        {
            var res = await _adminPanel.UpdateOrganization(model.OrganizationId, model.IsActive ? 1 : 0);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });

        }
        #endregion
        #region BranchRegion
        public async Task<IActionResult> GetBranchesByOrganizationId(OrganizationVM model)
        {
            var result = await _adminPanel.GetBranches(model.OrganizationId);
            return Json(result);
        }
        public async Task<IActionResult> Branches()
        {
            vm.Branches = await _adminPanel.GetBranches();
            return View(vm);
        }
     
        public async Task<IActionResult> AddBranchView()
        {
            vm.Organizations = await _adminPanel.GetOrganizations();
            vm.BusinessEntities = await _adminPanel.GetEntities();
            vm.BusinessCategories = await _adminPanel.BusinessCategories();
            return PartialView(vm);
        }
        [HttpPost]
        public async Task<IActionResult> AddBranch(BranchVM model)
        {
            var res = await _adminPanel.AddBranch(model);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });
        }

        public async Task<IActionResult> GetBranch(OrganizationVM model)
        {
            var res = await _adminPanel.GetOrganization(model.OrganizationId);

            return PartialView(res);
        }
        public async Task<IActionResult> EditBranch(OrganizationVM model)
        {
            var res = await _adminPanel.GetOrganization(model.OrganizationId);

            return PartialView(res);
        }
        public async Task<IActionResult> UpdateBranch(OrganizationVM model)
        {
            var res = await _adminPanel.EditOrganization(model);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            else
            {
                return Json(new { statusCode = "300" });

            }
        }
        public async Task<IActionResult> RemoveBranch(OrganizationVM model)
        {
            var res = await _adminPanel.RemoveOrganization(model.OrganizationId);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });

        }
        public async Task<IActionResult> ActiveUnactiveBranch(OrganizationVM model)
        {
            var res = await _adminPanel.UpdateOrganization(model.OrganizationId, model.IsActive ? 1 : 0);
            if (res)
            {
                return Json(new { statusCode = "200" });
            }
            return Json(new { statusCode = "300" });

        }
        #endregion
        #region BusinessFeatures
        public async Task<IActionResult> BusinessEntities()
        {
            vm.BusinessEntities = await _adminPanel.GetEntities();
            vm.BusinessCategories = await _adminPanel.GetBusinessCategories();
            return View(vm);
        }
        public async Task<IActionResult> AddBusinessEntity(BusinessEntityTypeVM model)
        {
            var result = await _adminPanel.AddBusinessEntity(model);
            if (result == -1)
            {
                return Json(new { statusCode = "210" });

            }
            else if (result == 1)
            {
                return Json(new { statusCode = "200" });

            }
            else
            {
                return Json(new { statusCode = "300" });

            }
        }
        public async Task<IActionResult> AddBusinessCategory(BusinessCategoryVM model)
        {
            var result = await _adminPanel.AddBusinessCategory(model);
            if (result == -1)
            {
                return Json(new { statusCode = "210" });

            }
            else if (result == 1)
            {
                return Json(new { statusCode = "200" });

            }
            else
            {
                return Json(new { statusCode = "300" });

            }
        }



        #endregion


        
        public async Task<IActionResult> BulkUpload()
        {
            return View();
        }
        public async Task<IActionResult> GetMasterData()
        {
            var result = await _adminPanel.MasterDataExcel();
            return File(result,
                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                 $"MasterExport_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx");
        }
        public async Task<IActionResult> UploadBulkFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            ExcelPackage.License.SetNonCommercialPersonal("Inspire");
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0]; 

            var rowCount = worksheet.Dimension.Rows;
            var currentTime = DateTime.UtcNow;
            var products = new List<MainModels.Models.Product>();
            var variants = new List<ProductVariant>();
            Guid? currentProductId = null;
            for (int row = 2; row <= rowCount; row++) 
            {
                string isVariant = worksheet.Cells[row, 1].Text.Trim().ToLower();

                if (isVariant == "no")
                {
                    var product = new MainModels.Models.Product();

                    product.ProductId = Guid.NewGuid();
                    product.ProductName = worksheet.Cells[row, 2].Text.Trim();
                    Guid.TryParse(worksheet.Cells[row, 3].Text?.Trim(), out Guid subcatId);
                    product.SubCategoryId = subcatId == Guid.Empty ? null : subcatId;
                    product.ProductDescription = worksheet.Cells[row, 4].Text.Trim();

                    Guid.TryParse(worksheet.Cells[row, 5].Text?.Trim(), out Guid uomId);
                    product.Uomid = uomId == Guid.Empty ? null : uomId;
                    Guid.TryParse(worksheet.Cells[row, 6].Text?.Trim(), out Guid brandId);
                    product.BrandId = brandId == Guid.Empty ? null : brandId;

                    product.IsActive = true;
                    product.IsDeleted = false;
                    product.CreatedOn = currentTime;
                    product.Createdby = AppDataUtility.SessionUser.Id;
                    product.ProductSlug = HelperClass.CreateSlug(product.ProductName);
                    product.BranchId = AppDataUtility.SessionUser.PersonVM.Branch.BranchId;
                    product.OrganizationId = AppDataUtility.SessionUser.PersonVM.Branch.Organization.OrganizationId;

                    currentProductId = product.ProductId;
                    products.Add(product);
                }
                else if (isVariant == "yes" && currentProductId != null)
                {
                    var variant = new ProductVariant();
                    variant.VariantId = Guid.NewGuid();
                    variant.ProductId = currentProductId.Value;
                    Guid.TryParse(worksheet.Cells[row, 7].Text?.Trim(), out Guid subuomId);
                    variant.SubUomid = subuomId == Guid.Empty ? null : subuomId;
                    Guid.TryParse(worksheet.Cells[row, 8].Text?.Trim(), out Guid materialId);
                    variant.MaterialId = materialId == Guid.Empty ? null : materialId;
                    Guid.TryParse(worksheet.Cells[row, 9].Text?.Trim(), out Guid colorId);
                    variant.ColorId = colorId == Guid.Empty ? null : colorId;
                    Guid.TryParse(worksheet.Cells[row, 10].Text?.Trim(), out Guid sizeId);
                    variant.SizeId = sizeId == Guid.Empty ? null : sizeId;
                    variant.BarCode = worksheet.Cells[row, 11].Text?.Trim();
                    variant.Cost = decimal.TryParse(worksheet.Cells[row, 12].Text, out var cost) ? cost : 0;
                    variant.SalesPrice = decimal.TryParse(worksheet.Cells[row, 13].Text, out var sp) ? sp : 0;
                    variant.PromotionPrice = decimal.TryParse(worksheet.Cells[row, 14].Text, out var pp) ? pp : 0;
                    variant.RetailPrice = decimal.TryParse(worksheet.Cells[row, 15].Text, out var rp) ? rp : 0;
                    Guid.TryParse(worksheet.Cells[row, 16].Text?.Trim(), out Guid subUomId);
                    variant.SubUomid = subUomId == Guid.Empty ? null : subUomId;
                    variant.QuantityPerUnit = int.TryParse(worksheet.Cells[row, 17].Text, out var qpu) ? qpu : 0;
                    var isSerialText = worksheet.Cells[row, 18].Text?.Trim();
                    variant.IsSerial = isSerialText == "1";
                    variant.MinQty = int.TryParse(worksheet.Cells[row, 19].Text, out var minQty) ? minQty : 0;
                    variant.MaxQty = int.TryParse(worksheet.Cells[row, 20].Text, out var maxQty) ? maxQty : 0;
                    variant.PriceFormat = int.TryParse(worksheet.Cells[row, 21].Text, out var pf) ? pf : 0;
                    variant.IsActive = true;
                    variant.IsDeleted = false;
                    variant.BranchId = AppDataUtility.SessionUser.PersonVM.Branch.BranchId;
                    variant.OrganizationId = AppDataUtility.SessionUser.PersonVM.Branch.Organization.OrganizationId;
                    variant.CreatedOn = currentTime;
                    variant.Createdby = AppDataUtility.SessionUser.Id;
                    variants.Add(variant);
                }
            }
            var allProducts = JsonConvert.SerializeObject(products);
            var allvariants = JsonConvert.SerializeObject(variants);
            HttpContext.Session.SetString("products", allProducts);
            HttpContext.Session.SetString("variants", allvariants);
            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var streamtwo = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            await _oneDb.Products.AddRangeAsync(products);
            await _oneDb.ProductVariants.AddRangeAsync(variants);
            await _oneDb.SaveChangesAsync();
            // TODO: Read and process Excel here (e.g., using EPPlus or ClosedXML)

            return Json(new { statusCode = "200" });
        }
        //[AllowAnonymous]
        //public async Task<IActionResult> ResetData()
        //{
        //    var result = await _adminPanel.removeData();
        //    if (result > 0)
        //    {
        //        return Json(new { statusCode = "200", Message = "New Record Updated" });
        //    }

        //    else
        //    {
        //        return Json(new { statusCode = "300", Message = "Unable to Setup Business" });
        //    }
        //}
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/invn.xlsx");

            if (!System.IO.File.Exists(filePath))
                return NotFound("File not found");

            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = "DownloadedFile.xlsx"; // This is the name the browser will use

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, contentType, fileName);
        }
        public async Task<IActionResult> GetCustomers()
        {
            return Ok(await _adminPanel.Customers());
        }
        public async Task<IActionResult> GetCustomerById(CustomerVM model)
        {
            return Ok(await _adminPanel.Customers(model.CustomerId));
        }
        
        public async Task<IActionResult> SystemPrefrences(CustomerVM model)
        {
           vm.SystemPreferences = await _adminPanel.GetSystemPreferences();
            vm.Organizations = await _adminPanel.GetOrganizations();
            vm.Branches= await _adminPanel.GetBranches();
            return View(vm);
        }
        public async Task<IActionResult> GetSystemPrefrences(CustomerVM model)
        {
            vm.SystemPreferences = await _adminPanel.GetSystemPreferences(model.BranchId.Value);
            return PartialView(vm);
        }
        public async Task<IActionResult> AccountPrefrences(CustomerVM model)
        {
            vm.SystemPreferences = await _adminPanel.GetSystemPreferences();
            vm.Organizations = await _adminPanel.GetOrganizations();
            vm.Branches = await _adminPanel.GetBranches();
            return View(vm);
        }
        public async Task<IActionResult> GetAccountPrefrences(CustomerVM model)
        {
            vm.AccountingPreferences = await _adminPanel.GetAccountPrefrences(model.BranchId.Value);
            return PartialView(vm);
        }
        [HttpPost]
        public async Task<IActionResult> SaveSystemPrefrences(SystemPreferencesVM model, IFormFile? CompanyLogoFile)
        {
            var result = await _adminPanel.SavePrefrences(model);
            return Json(new { });
        }
        [HttpPost]
        public async Task<IActionResult> SaveAccountingPreferences(AccountingPreferencesVM model)
        {
            var result = await _adminPanel.SaveAccountingPreferences(model);
            return Json(new { });
        }

    }
}
