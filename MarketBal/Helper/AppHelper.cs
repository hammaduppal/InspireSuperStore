using System.ComponentModel.DataAnnotations;
using System.Web;
using MainModels.DTOModels;
using MainModels.Util;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace MarketBal.Helper
{
    public static class AppHelper
    {
        private static IServiceProvider _serviceProvider;

        // Call this once in Program.cs at startup
        public static void Configure(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static string CreateSSOURL(int appId)
        {
            // 1. Resolve HttpContext and the scoped session safely for the current thread
            var httpContextAccessor = _serviceProvider?.GetService<IHttpContextAccessor>();
            var currentContext = httpContextAccessor?.HttpContext;
            var sessionService = currentContext?.RequestServices?.GetService<ISessionService>();

            var currentUser = sessionService?.SessionUser;
            if (currentUser == null)
            {
                throw new InvalidOperationException("Cannot create SSO URL because no user session exists.");
            }

            // 2. Run your exact matching logic from PagesViewModel
            var setting = PagesViewModel.Settings.FirstOrDefault(x => x.ApplicationId == appId);
            if (setting == null)
            {
                throw new Exception($"Application setting not found for App ID: {appId}");
            }

            var ssoLogin = new SSOLogin
            {
                UserName = currentUser.UserName,
                Password = EncryptionPasses.Decrypt(
                    currentUser.Password,
                    PassesCore.INIT_VECTOR,
                    PassesCore.PASS_PHRASE,
                    PassesCore.KEY_SIZE
                )
            };

            string encrypted = EncryptionPasses.RandomEncrypt(JsonConvert.SerializeObject(ssoLogin));
            string safeEncrypted = Uri.EscapeDataString(encrypted);

            string baseUrl = setting.ApplicationUrl.TrimEnd('/');
            return $"{baseUrl}/Account/Login?returnUrl={safeEncrypted}";
        }
    }
    public class SSOLogin
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
