using System.Globalization;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.InvoiceRP;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.ReportRP
{
    public class ReportRepository
    {
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;

        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly InvoiceRepository _invoiceRepository;
        public ReportRepository(IConfiguration config, OneDb oneDb, ISessionService sessionService)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config, _onedb, _sessionService);
            _invoiceRepository = new InvoiceRepository(_config, _onedb, _sessionService);
        }

        public async Task<InvoiceChartDto> GetInvoiceChart()
        {
            var data = await _onedb.InvoiceMasters
                .GroupBy(x => new { x.InvoiceDate.Value.Year, x.InvoiceDate.Value.Month })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.GrandTotal)
                })
                .OrderBy(x => x.Month)
                .ToListAsync();

            // Convert month number to month name
            var months = data
                .Select(x => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(x.Month))
                .ToList();

            var sales = data.Select(x => x.Total ?? 0).ToList();

            return new InvoiceChartDto
            {
                Months = months,
                Sales = sales
            };
        }

        public async Task<InvoiceChartDto> GetSalesProfitChart()
        {
            var invoices = await _onedb.InvoiceMasters
        .Include(i => i.InvoiceDetails)
        .ToListAsync();

            // 2. Group by Year + Month
            var grouped = invoices
                .GroupBy(i => new { i.InvoiceDate.Value.Year, i.InvoiceDate.Value.Month })
                .OrderBy(g => g.Key.Month)
                .ToList();

            List<string> months = new();
            List<decimal> salesList = new();
            List<decimal> profitList = new();

            // 3. Process each month
            foreach (var monthGroup in grouped)
            {
                string monthName =
                    CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthGroup.Key.Month);

                decimal monthlySales = monthGroup.Sum(i => i.GrandTotal ?? 0);

                decimal monthlyCost = 0;

                // 4. Loop each invoice in the month to calculate cost
                foreach (var invoice in monthGroup)
                {
                    var details = invoice.InvoiceDetails?.ToList() ?? new List<InvoiceDetail>();

                    // Await cost function
                    monthlyCost += await _invoiceRepository.GetCostofGoods(details);
                }

                decimal monthlyProfit = monthlySales - monthlyCost;

                months.Add(monthName);
                salesList.Add(monthlySales);
                profitList.Add(monthlyProfit);
            }

            // 5. Return DTO
            return new InvoiceChartDto
            {
                Months = months,
                Sales = salesList,
                Profit = profitList
            };
        }

    }
}
