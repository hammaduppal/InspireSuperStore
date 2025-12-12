using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Newtonsoft.Json;

namespace MarketBal.Repository.SystemRP
{
    public class LicenseValidator
    {
        private readonly OneDb _context;
        public LicenseValidator(OneDb oneDb)
        {
            _context = oneDb;
        }

        //public LicenseModelVM Validate(string encryptedLicense, string currentDeviceId)
        //{
        //    try
        //    {
        //        // 1. Decrypt license
        //        string json = AESEncryption.DecryptWithPublicKey(encryptedLicense, _publicKey);

        //        // 2. Convert to object
        //        var model = JsonConvert.DeserializeObject<LicenseModelVM>(json);

        //        // 3. Validate device
        //        if (model.DeviceId != currentDeviceId)
        //            throw new Exception("License does not match this server.");

        //        // 4. Validate expiry
        //        if (DateTime.UtcNow > model.ValidTill)
        //            throw new Exception("License has expired.");

        //        return model;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Invalid License: " + ex.Message);
        //    }
        //}

        //public bool ValidateLicense()
        //{
        //    // 1. Get latest license
        //    var license = _context.ClientLicenses
        //                          .OrderByDescending(x => x.Id)
        //                          .FirstOrDefault();

        //    // No license found
        //    if (license == null)
        //        return false;

        //    // 2. Generate current machine fingerprint
        //    var fingerprint = HardwareInfo.GetServerFingerprint();

        //    // 3. Validate fingerprint
        //    if (license.MachineFingerprint != fingerprint)
        //        return false;

        //    // 4. Validate expiration date
        //    if (DateTime.Now > license.EndDate)
        //        return false;

        //    // 5. Validate license integrity (SHA256)
        //    var expectedKey = _licenseService.ConvertToSha256(
        //        fingerprint +
        //        license.StartDate.ToString("yyyyMMdd") +
        //        license.EndDate.ToString("yyyyMMdd")
        //    );

        //    // Compare stored key vs real expected key
        //    if (license.LicenseKey != expectedKey)
        //        return false;

        //    // Everything OK
        //    return true;
        //}

    }

}
