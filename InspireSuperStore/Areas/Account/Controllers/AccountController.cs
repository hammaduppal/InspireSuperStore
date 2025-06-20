using MainModels.DTOModels;
using MainModels.Util;
using MarketBal.Repository.Account;
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
        public AccountController(IConfiguration config)
        {
            _config = config;
            _login = new AccountRepository(_config);
            _aes = new AESEncryption();
        }
        public IActionResult Login(string? ReturnUrl = null)
        {
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
            if (aesresult=="ERP")
            {
                return Json(new { statusCode = "200",Message="Special Feature Unlocked" });

            }
            else
            {
                return Json(new { statusCode = "300",Message="Wrong Password" });

            }
        }
    }
}
