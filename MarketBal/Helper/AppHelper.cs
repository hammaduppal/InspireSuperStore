using System.ComponentModel.DataAnnotations;
using MainModels.DTOModels;
using MainModels.Util;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace MarketBal.Helper
{
    public class AppHelper
    {
        public static string CreateSSOURL(int appId)
        {
            var setting = PagesViewModel.Settings.Where(x => x.ApplicationId == appId).FirstOrDefault();
            string encrypted = EncryptionPasses.RandomEncrypt(JsonConvert.SerializeObject(AppDataUtility.SessionUser));
            string safeEncrypted = Uri.EscapeDataString(encrypted);

            string url = $"{setting.ApplicationUrl}/sso?key={safeEncrypted}"; 
            return url;
        }
    }
}
