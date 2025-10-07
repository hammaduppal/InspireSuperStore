using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.AccountingRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.AccountingArea.Controllers
{
    [Authorize(Roles = UserRolesConstants.Accounts + "," + UserRolesConstants.Admin)]
    [Area("AccountingArea")]
    [Route("[controller]/[action]")]
    public class AccountingController : Controller
    {
        PagesViewModel vm = new PagesViewModel();
        private readonly ILogger<AccountingController> _logger;
        private readonly IConfiguration _configuration;
        private readonly JournalsRepository _journalRepo;
        private readonly ChartOfAccountRepo _chartOfAccountRepo;
        private readonly OneDb _onedb;
        public AccountingController(ILogger<AccountingController> logger, IConfiguration configuration,OneDb onedb)
        {
            _logger = logger;
            _configuration = configuration;
            _onedb = onedb;

            _chartOfAccountRepo =new ChartOfAccountRepo(_configuration, _onedb);
        }
        public async Task<IActionResult> ChartOfAccounts()
        {
            vm.ChartofAccounts = await _chartOfAccountRepo.GetAllChartOfAccounts();
            return View(vm);
        }


        public IActionResult JournalEntries()
        {
            return View();
        }

  
        public IActionResult Receivables_Customers()
        {
            return View();
        }

        public IActionResult Receivables_Invoices()
        {
            return View();
        }

        public IActionResult Receivables_Payments()
        {
            return View();
        }

        public IActionResult Receivables_Statements()
        {
            return View();
        }


        public IActionResult Payables_Suppliers()
        {
            return View();
        }

        public IActionResult Payables_Bills()
        {
            return View();
        }

        public IActionResult Payables_Payments()
        {
            return View();
        }

        public IActionResult Payables_Statements()
        {
            return View();
        }


        public IActionResult Taxation()
        {
            return View();
        }


        public IActionResult Reconciliation()
        {
            return View();
        }
    }

}
