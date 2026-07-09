using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.ReportRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.ReportingArea.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.Product)]
    [Area("ReportingArea")]
    [Route("Reports/[action]")]
    public class ReportsController : Controller
    {
        private readonly IConfiguration _config;
        private readonly OneDb _context;
        private readonly ReportRepository _repo;
        private readonly ISessionService _sessionService;
        public ReportsController(IConfiguration config, OneDb context, ISessionService sessionService)
        {
            _config = config;
            _context = context;
            _sessionService = sessionService;
            _repo = new ReportRepository(config, context,_sessionService);
        }
        public IActionResult MonthlySaleReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceChart()
        {
            // Example: Fetch values from database

            // Fetching sales data asynchronously
            var result = await _repo.GetInvoiceChart();

            return Json(new
            {
                months = result.Months,
                sales = result.Sales
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetSalesProfitChart()
        {
            try
            {
                var result = await _repo.GetSalesProfitChart();
                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to load Sales/Profit chart data",
                    error = ex.Message
                });
            }
        }

    }
}
