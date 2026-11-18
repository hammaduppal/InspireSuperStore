using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.InvoiceRP;
using MarketBal.Repository.ReportRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly InvoiceRepository _invoiceRepository;
        public ReportsController(IConfiguration config, OneDb context, ISessionService sessionService)
        {
            _config = config;
            _context = context;
            _sessionService = sessionService;
            _repo = new ReportRepository(config, context,_sessionService);
            _invoiceRepository = new InvoiceRepository(_config, _context, _sessionService);
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

        [HttpGet]
        public async Task<IActionResult> GetDailySalesChart()
        {
            var startDate = DateTime.Today.AddDays(-7);

            var data =  _context.InvoiceMasters // Ensure your context name matches (e.g., _onedb or _context)
                .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate != null)
                .GroupBy(x => x.InvoiceDate.Value.Date)
                .Select(g => new
                {
                    SaleDate = g.Key,
                    Total = g.Sum(x => x.TotalAmount ?? 0m)
                })
                .OrderBy(x => x.SaleDate)
                .ToList();

            var days = data.Select(x => x.SaleDate.ToString("dd MMM")).ToList();
            var sales = data.Select(x => x.Total).ToList();

            return Json(new
            {
                days = days,
                sales = sales
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSalesDonutChart()
        {
            var data =  _context.InvoiceDetails
                .GroupBy(x => x.Product.SubCategory.Category.CategoryName)
                .Select(g => new
                {
                    Category = g.Key ?? "Uncategorized",
                    Total = g.Sum(x => x.LineTotal)
                })
                .ToList();

            return Json(new
            {
                labels = data.Select(x => x.Category).ToList(),
                values = data.Select(x => x.Total).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetDailySalesAndProfitChart()
        {
            // Fetch recent invoices (e.g., last 7 days) to maintain high performance
            var startDate = DateTime.Today.AddDays(-7);

            var invoices = await _context.InvoiceMasters
                .Include(i => i.InvoiceDetails)
                .Where(i => i.InvoiceDate >= startDate)
                .AsNoTracking()
                .AsSplitQuery()
                .ToListAsync();

            // Group by exact Date (ignoring time)
            var grouped = invoices
                .Where(i => i.InvoiceDate.HasValue)
                .GroupBy(i => i.InvoiceDate.Value.Date)
                .OrderBy(g => g.Key)
                .ToList();

            List<string> daysList = new();
            List<decimal> salesList = new();
            List<decimal> profitList = new();

            foreach (var dayGroup in grouped)
            {
                string dayName = dayGroup.Key.ToString("dd MMM"); // e.g., "15 Nov"

                decimal dailySales = dayGroup.Sum(i => i.GrandTotal ?? 0);
                decimal dailyCost = 0;

                foreach (var invoice in dayGroup)
                {
                    var details = invoice.InvoiceDetails?.ToList() ?? new List<InvoiceDetail>();
                    dailyCost += await _invoiceRepository.GetCostofGoodsFromInvoice(details);
                }

                decimal dailyProfit = dailySales - dailyCost;

                daysList.Add(dayName);
                salesList.Add(dailySales);
                profitList.Add(dailyProfit);
            }

            return Json(new
            {
                days = daysList,
                sales = salesList,
                profit = profitList
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetLowStockAlerts()
        {
            try
            {
                // Get the current user's branch ID from your session service
                var branchId = _sessionService.SessionUser.Person.Branch.BranchId;

                // Query BranchStocks, join with ProductVariants and Products
                var lowStockItems = await _context.BranchStocks
                    .Include(bs => bs.ProductVariant)
                        .ThenInclude(pv => pv.Product) // Assuming ProductVariant has a navigation property to Product
                    .Where(bs => bs.BranchId == branchId &&
                                 bs.ProductVariant != null &&
                                 bs.ProductVariant.MinQty.HasValue &&
                                 bs.Qty <= bs.ProductVariant.MinQty.Value)
                    .OrderBy(bs => bs.Qty)
                    .Take(10) // Limit to top 10 critical alerts
                    .Select(bs => new LowStockItemDto
                    {
                        ProductId = bs.ProductVariant.ProductId.Value, // Or ProductVariantId depending on your EditProduct route
                        ItemName = bs.ProductVariant.Product.ProductName + (!string.IsNullOrEmpty(bs.ProductVariant.Product.ProductName)),
                        Quantity = bs.Qty
                    })
                    .ToListAsync();

                return Json(new { statusCode = "200", data = lowStockItems });
            }
            catch (Exception ex)
            {
                return Json(new { statusCode = "500", message = ex.Message });
            }
        }







    }
}
