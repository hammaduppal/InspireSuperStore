using MainModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Account;
using MarketBal.Repository.SystemRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace InspireSuperStore.Areas.AdminArea.Controllers
{
    [Area("AdminArea")]
    [Route("[controller]/[action]")]
    public class LicenseController : Controller
    {
        private readonly LicenseService _license;
        private readonly OneDb _context;
        private readonly AccountRepository _login;
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly DBManager _db;

        public LicenseController(LicenseService license,OneDb oneDb, ISessionService sessionService, IConfiguration config)
        {

            _license = license;
            _context = oneDb;
            _sessionService = sessionService;
            _config = config;
            _login = new AccountRepository(_config,_sessionService);
            _db = new DBManager(_config);


        }
        public async Task<IActionResult> LicenseActivation(Guid? branchId = null)
        {
            Guid? resolvedBranchId = branchId ?? TempData.Peek("LicenseBranchId") as Guid?;
            
            string serverFingerprint = _license.GetServerFingerprint();
            string requestCode = string.Empty;

            if (resolvedBranchId.HasValue && resolvedBranchId.Value != Guid.Empty)
            {
                var existingLicense = await _login.GetActiveBranchLicense(resolvedBranchId.Value);
                requestCode = existingLicense?.RequestCode ?? $"REQ-{resolvedBranchId.Value.ToString("N")[..8].ToUpperInvariant()}";
            }

            ViewBag.BranchId = resolvedBranchId;
            ViewBag.RequestCode = requestCode;
            ViewBag.ServerFingerprint = serverFingerprint;

            return View();
        }
        [HttpGet]
        public string GenerateKey(string requestCode, string serverFingerprint, DateTime expiryDate)
        {
            if (string.IsNullOrWhiteSpace(requestCode) || string.IsNullOrWhiteSpace(serverFingerprint))
            {
                return "RequestCode and ServerFingerprint are required.";
            }

            string licenseKey = SimpleLicenseHelper.GenerateLicenseKey(
                requestCode: requestCode,
                serverFingerprint: serverFingerprint,
                expiryDateUtc: expiryDate.ToUniversalTime());


            return licenseKey;
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateLicense(Guid? branchId, string? requestCode, string? serverFingerprint, string? licenseKey)
        {
            string currentFingerprint = string.IsNullOrWhiteSpace(serverFingerprint)
                ? _license.GetServerFingerprint()
                : serverFingerprint.Trim();

            if (!branchId.HasValue || branchId.Value == Guid.Empty)
            {
                ModelState.AddModelError("", "Branch information is missing.");
            }

            if (string.IsNullOrWhiteSpace(requestCode))
            {
                ModelState.AddModelError("", "Request code is missing.");
            }

            if (string.IsNullOrWhiteSpace(licenseKey))
            {
                ModelState.AddModelError("", "Please enter a valid license key.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BranchId = branchId;
                ViewBag.RequestCode = requestCode ?? "";
                ViewBag.ServerFingerprint = currentFingerprint;
                return View("LicenseActivation");
            }

            var validation = SimpleLicenseHelper.ValidateLicenseKey(
                licenseKey: licenseKey,
                expectedRequestCode: requestCode,
                currentServerFingerprint: currentFingerprint);

            if (!validation.IsValid)
            {
                ModelState.AddModelError("", validation.Message);
                ViewBag.BranchId = branchId;
                ViewBag.RequestCode = requestCode;
                ViewBag.ServerFingerprint = currentFingerprint;
                return View("LicenseActivation");
            }

            // Deactivate previous active licenses for this branch before saving the new one
            const string deactivateSql = @"
        UPDATE Business.BranchLicenses
        SET IsActive = 0,
            UpdatedOn = GETUTCDATE()
        WHERE BranchId = @BranchId
          AND IsActive = 1;";

            await _db.ExecuteInsertQueryandParam(deactivateSql, new { BranchId = branchId.Value });

            const string insertSql = @"
        INSERT INTO Business.BranchLicenses
        (
            BranchId,
            RequestCode,
            ServerFingerprint,
            LicenseKey,
            ExpiryDate,
            IsActive,
            ActivatedOn,
            CreatedOn
        )
        VALUES
        (
            @BranchId,
            @RequestCode,
            @ServerFingerprint,
            @LicenseKey,
            @ExpiryDate,
            1,
            GETUTCDATE(),
            GETUTCDATE()
        );";

            await _db.ExecuteInsertQueryandParam(insertSql, new
            {
                BranchId = branchId.Value,
                RequestCode = requestCode!.Trim().ToUpperInvariant(),
                ServerFingerprint = currentFingerprint,
                LicenseKey = licenseKey!.Trim(),
                ExpiryDate = validation.ExpiryUtc
            });

            TempData["LicenseSuccess"] = $"License activated successfully ({validation.Message}).";
            return Json(new { statusCode = "200" }); // or wherever you want them to proceed after activation
        }











    }
}
