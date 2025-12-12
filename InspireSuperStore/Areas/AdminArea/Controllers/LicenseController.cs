using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.SystemRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Route("[controller]/[action]")]
    public class LicenseController : Controller
    {
        private readonly LicenseService _licenseService;
        private readonly OneDb _context;

        public LicenseController(LicenseService license,OneDb oneDb)
        {

            _licenseService = license;
            _context = oneDb;
        }

        //public IActionResult Index()
        //{
        //    var licenses = _licenseService.GetAllLicenses(); // We'll add this method
        //    return View(licenses);
        //}

        //// GET: /License/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}

        //// POST: /License/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(string customerName, string productName, DateTime validTill)
        //{
        //    string deviceId = _licenseService.GetServerFingerprint();
        //    var license = _licenseService.CreateLicense(customerName, deviceId, productName, validTill);
        //    TempData["Success"] = $"License created successfully. Key: {license.LicenseKey}";
        //    return RedirectToAction(nameof(Index));
        //}

        //// GET: /License/Validate
        //public IActionResult Validate()
        //{
        //    return View();
        //}

        //// POST: /License/Validate
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Validate(string licenseKey)
        //{
        //    string deviceId = _licenseService.GetServerFingerprint();
        //    bool isValid = _licenseService.ValidateLicense(licenseKey, deviceId);

        //    ViewBag.Message = isValid ? "License is valid." : "License is invalid or expired.";
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult Activate(string licenseKey)
        //{
        //    string deviceId = _licenseService.GetServerFingerprint();
        //    bool isValid = _licenseService.ValidateLicense(licenseKey, deviceId);

        //    if (!isValid)
        //        return Json(new { success = false, message = "License invalid or expired." });

        //    // Bind license to device if not already bound
        //    var license = _context.Licenses.First(l => l.LicenseKey == licenseKey);
        //    if (string.IsNullOrEmpty(license.DeviceId))
        //    {
        //        license.DeviceId = deviceId;
        //        _context.SaveChanges();
        //    }

        //    return Json(new { success = true, message = "License activated successfully." });
        //}

    }
}
