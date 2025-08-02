using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;

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
       
        public async Task<IActionResult> LoginUser()
        {
            vm.LoginUsers = await _adminPanel.GetLoginUser();

            return View(vm);
        }
        [AllowAnonymous]
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




        public async Task<IActionResult> GetBranchesByOrganizationId(OrganizationVM model)
        {
            var result = await _adminPanel.GetBranches(model.OrganizationId);
            return Json(result);
        }
        [AllowAnonymous]
        public async Task<IActionResult> StartupSettings()
        {
            var result = await _adminPanel.GetOrganizations();
            if (result.Count > 0)
            {
                return Redirect("/");
            }
            vm.BusinessCategories = await _adminPanel.BusinessCategories();
            vm.BusinessEntities = await _adminPanel.BusinessEntityType();

            return View(vm);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> SeedData(OrganizationRegistrationDto model)
        {
            var result = await _adminPanel.SeedinData(model);
            if (result)
            {
                return Json(new { statusCode = "200", Message = "New Record Updated" });
            }

            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Setup Business" });
            }
        }
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
    }
}
