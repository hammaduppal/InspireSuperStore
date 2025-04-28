using MainModels.DTOModels;
using MainModels.Util;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Account.Controllers
{
    [Area("Account")]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AccountRepository _login;
        public AccountController(IConfiguration config)
        {
            _config = config;
            _login = new AccountRepository(_config);
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
    }
}
