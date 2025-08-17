using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Account.Controllers
{
    [Area("Account")]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AccountRepository _login;
        private readonly AESEncryption _aes;
        private readonly PagesViewModel vm = new PagesViewModel();
        private readonly AdminPanelRepository _admin;
        private readonly OneDb _oneDb;
        private readonly HumanRespourceRepository _hrm;
        public AccountController(IConfiguration config, OneDb onedb)
        {
            _config = config;
            _oneDb = onedb;
            _login = new AccountRepository(_config);
            _aes = new AESEncryption();
            _admin = new AdminPanelRepository(_config, _oneDb);
            _hrm = new HumanRespourceRepository(_config, _oneDb);
        }
        public async Task<IActionResult> Login(string? ReturnUrl = null)
        {
            var result = await _admin.GetOrganizations();
            if (result.Count == 0)
            {
                return Redirect("/AdminPanel/StartupSettings");
            }
            TempData["ReturnURL"] = ReturnUrl;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ValidateLogin(LoginUserVM formData)
        {
            try
            {
                string url = (string)TempData["ReturnURL"];
                var res = await _login.ValidateLogin(formData);
                if (res != null)
                {
                    await _login.SigninAsync(res, HttpContext);
                    res.Passwords = "";
                    AppDataUtility.SessionUser = res;
                    //if (res.RoleName=="SuperAdmin")
                    //{
                    //    url = "/adminPanel";

                    //}
                    return Json(new { statusCode = "200", Message = "LoginSuccessfull", returnUrl = url });
                }
                else
                {
                    return Json(new { statusCode = "300", Message = "LoginFailure" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Invalid JWT", error = ex.Message });
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _login.LogoutAsync(HttpContext);
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Failure()
        {
            return View();
        }

        public IActionResult DecryptSomeThing(SecretLock model)
        {
            string aesresult = _aes.Decrypt(model.UnlockKey);
            if (aesresult == "ERP")
            {
                return Json(new { statusCode = "200", Message = "Special Feature Unlocked" });

            }
            else
            {
                return Json(new { statusCode = "300", Message = "Wrong Password" });

            }
        }

        public async Task< IActionResult> _AddCustomerForm()
        {
            vm.Countries= await _admin.Countries();
            return View(vm);
        }
        public async Task<IActionResult> AddNewCustomer(PersonVM model)
        {
            var result = await _login.AddCustomer(model);
            return Json(new {statusCode="200", CustomerId=result });
        }

    }
}
