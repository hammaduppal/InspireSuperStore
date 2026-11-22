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

        public string GetServerFingerprint()
        {
            string cpu = HardwareInfo.GetCpuId();
            string board = HardwareInfo.GetMotherBoardId();
            string disk = HardwareInfo.GetDiskId();
            string mac = HardwareInfo.GetMacAddress();

            string raw = $"{cpu}|{board}|{disk}|{mac}";
            return raw;
        }

        public static (bool IsValid, string Message) ValidateLicenseRecord(BranchLicenseVM? license, string serverInfo)
        {
            if (license == null)
                return (false, "No active license found for this branch.");

            if (license.IsActive != true) // adjust if IsActive is non-nullable bool
                return (false, "License is inactive.");

            if (string.IsNullOrWhiteSpace(serverInfo) ||
                !string.Equals(license.ServerFingerprint?.Trim(), serverInfo.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Server fingerprint does not match this machine.");
            }

            if (!license.ExpiryDate.HasValue || DateTime.UtcNow > license.ExpiryDate.Value)
                return (false, "License has expired.");

            return (true, $"License valid until {license.ExpiryDate.Value:yyyy-MM-dd}.");
        }

       


    }

}
