using System.Security.Cryptography;
using System.Text;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Newtonsoft.Json;

namespace MarketBal.Repository.SystemRP
{
    public class LicenseService
    {
        public static string PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
<your private key here>
-----END RSA PRIVATE KEY-----";
        public static string PublicKey = @"-----BEGIN PUBLIC KEY-----
<your public key here>
-----END PUBLIC KEY-----";
        private readonly OneDb _context;

        public LicenseService(OneDb context)
        {
            _context = context;
        }
        //public List<License> GetAllLicenses()
        //{
        //    return _context.Licenses.ToList();
        //}

        // Generate SHA256 hash
        public string ConvertToSha256(string raw)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(raw);
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public string GetServerFingerprint()
        {
            string cpu = HardwareInfo.GetCpuId();
            string board = HardwareInfo.GetMotherBoardId();
            string disk = HardwareInfo.GetDiskId();
            string mac = HardwareInfo.GetMacAddress();

            string raw = $"{cpu}|{board}|{disk}|{mac}";
            return raw;
        }
        // Generate a license key
        public string GenerateLicenseKey(string deviceId, string productName, DateTime validTill)
        {
            string raw = $"{deviceId}-{productName}-{validTill:yyyyMMddHHmmss}";
            return ConvertToSha256(raw);
        }

        // Create and save a license
        //public License CreateLicense(string customerName, string deviceId, string productName, DateTime validTill)
        //{
        //    var licenseKey = GenerateLicenseKey(deviceId, productName, validTill);

        //    var license = new License
        //    {
        //        CustomerName = customerName,
        //        DeviceId = deviceId,
        //        ProductName = productName,
        //        ValidTill = validTill,
        //        LicenseKey = licenseKey,
        //        IsActive = true
        //    };

        //    _context.Licenses.Add(license);
        //    _context.SaveChanges();

        //    return license;
        //}

        // Validate a license key
        //public bool ValidateLicense(string licenseKey, string deviceId)
        //{
        //    var license = _context.Licenses.FirstOrDefault(l => l.LicenseKey == licenseKey && l.IsActive);

        //    if (license == null)
        //        return false;

        //    if (license.ValidTill < DateTime.UtcNow)
        //        return false;

        //    if (license.DeviceId != deviceId)
        //        return false;

        //    return true;
        //}

        public static string GenerateLicense(string deviceId, string customerName, string productName, DateTime validTill)
        {
            var model = new LicenseModelVM
            {
                DeviceId = deviceId,
                CustomerName = customerName,
                ProductName = productName,
                ValidTill = validTill
            };

            string json = JsonConvert.SerializeObject(model);
            return AESEncryption.EncryptWithPrivateKey(json, PrivateKey);
        }

    }
}
