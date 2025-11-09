using InspireSuperStore.Areas.Notification.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Newtonsoft.Json;
using static QRCoder.PayloadGenerator;

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
        private readonly NotificationRepository notificationRepository;
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
            notificationRepository = new NotificationRepository(_oneDb);
        }
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            var result = await _admin.GetOrganizations();

            if (result.Count == 0)
            {
                // Redirect to StartupSettings in AdminPanel controller
                return RedirectToAction("StartupSettings", "Account");
            }
            if (CheckEncryption(returnUrl))
            {
                string base64 = Uri.UnescapeDataString(returnUrl);
                string decrypted = EncryptionPasses.RandomDecrypt(base64);
                LoginUserVM user = Newtonsoft.Json.JsonConvert.DeserializeObject<LoginUserVM>(decrypted);
                var res = await _login.ValidateLogin(user);
                if (res != null)
                {
                    await _login.SigninAsync(res, HttpContext);
                    await LoginHandler(res);

                    //AppDataUtility.SessionUser = res;
                    //string[] rolesnames = res.Roles.Select(x => x.Name).ToArray();
                    //bool isSuperAdmin = res.Roles.Any(r => r.Id == 1 && r.Name.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
                    //if (!isSuperAdmin)
                    //{
                    //    AppDataUtility.UserNotifications = await notificationRepository.GetGroupNotification(rolesnames);
                    //}
                    //else
                    //{
                    //    AppDataUtility.UserNotifications = new List<NotificationsDTO>(); // empty list for superadmin
                    //}

                }
            }
            TempData["ReturnURL"] = returnUrl;
            return View();
        }
        private bool CheckEncryption(string returnUrl)
        {
            bool isEncrypted = false;
            string decrypted = null;

            if (!string.IsNullOrEmpty(returnUrl))
            {
                try
                {
                    string unescaped = Uri.UnescapeDataString(returnUrl);
                    decrypted = EncryptionPasses.RandomDecrypt(unescaped);

                    // if decryption didn't throw, assume it's valid
                    if (!string.IsNullOrEmpty(decrypted))
                        isEncrypted = true;
                }
                catch
                {
                    isEncrypted = false; // not encrypted
                }
            }

            return isEncrypted;
        }
        private async Task<int> LoginHandler(LoginUserVM res)
        {
            res.WelcomeMessage = GreetingHelper.GetGreeting();
            AppDataUtility.SessionUser = res;
            string[] rolesnames = res.Roles.Select(x => x.Name).ToArray();
            bool isSuperAdmin = res.Roles.Any(r => r.Id == 1 && r.Name.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
            if (!isSuperAdmin)
            {
                AppDataUtility.UserNotifications = await notificationRepository.GetGroupNotification(rolesnames);
            }
            else
            {
                AppDataUtility.UserNotifications = new List<NotificationsDTO>(); // empty list for superadmin
            }
            return 1;
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
                    await LoginHandler(res);
                    // res.Passwords = "";
                    //AppDataUtility.SessionUser = res;
                    //string[] rolesnames = res.Roles.Select(x => x.Name).ToArray();
                    //bool isSuperAdmin = res.Roles.Any(r => r.Id == 1 && r.Name.Equals("superadmin", StringComparison.OrdinalIgnoreCase));
                    //if (!isSuperAdmin)
                    //{
                    //    AppDataUtility.UserNotifications = await notificationRepository.GetGroupNotification(rolesnames);
                    //}
                    //else
                    //{
                    //    AppDataUtility.UserNotifications = new List<NotificationsDTO>(); // empty list for superadmin
                    //}
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

        public async Task<IActionResult> _AddCustomerForm()
        {
            vm.Countries = await _admin.Countries();
            return View(vm);
        }
        public async Task<IActionResult> AddNewCustomer(PersonVM model)
        {
            var result = await _login.AddCustomer(model);
            return Json(new { statusCode = "200", CustomerId = result });
        }



        public async Task<IActionResult> StartupSettings()
        {
            //var result = await _adminPanel.GetOrganizations();
            //if (result.Count > 0)
            //{
            //    return Redirect("/");
            //}
            vm.BusinessCategories = await _admin.BusinessCategories();
            vm.BusinessEntities = await _admin.BusinessEntityType();

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> SeedData(OrganizationRegistrationDto model)
        {
            var result = await _admin.SeedinData(model);
            if (result)
            {
                return Json(new { statusCode = "200", Message = "New Record Updated" });
            }

            else
            {
                return Json(new { statusCode = "300", Message = "Unable to Setup Business" });
            }
        }

        [HttpGet]
        [Route("/Account/SSO/{key}")]
        public async Task<IActionResult> SSO(string key)
        {
            string base64 = Uri.UnescapeDataString(key);
            string decrypted = EncryptionPasses.RandomDecrypt(base64);
            LoginUserVM user = JsonConvert.DeserializeObject<LoginUserVM>(decrypted);
            var res = await _login.SSOValidateLogin(user);
            if (res != null)
            {
                await _login.SigninAsync(res, HttpContext);
                AppDataUtility.SessionUser = res;
                //if (res.RoleName=="SuperAdmin")
                //{
                //    url = "/adminPanel";

                //}
                return Redirect("/");

            }
            else
            {
                return View();
            }
        }



    }
}
