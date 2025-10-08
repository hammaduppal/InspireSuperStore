using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.AccountingRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        private readonly AccountsReceivableRepository _accountreceivable;
        private readonly OneDb _onedb;
        public AccountingController(ILogger<AccountingController> logger, IConfiguration configuration, OneDb onedb)
        {
            _logger = logger;
            _configuration = configuration;
            _onedb = onedb;
            _accountreceivable = new AccountsReceivableRepository(_configuration, _onedb);
            _journalRepo = new JournalsRepository(_configuration, _onedb);
            _chartOfAccountRepo = new ChartOfAccountRepo(_configuration, _onedb);
        }
        public async Task<IActionResult> ChartOfAccounts()
        {
            vm.ChartofAccounts = await _chartOfAccountRepo.GetAllChartOfAccounts();
            return View(vm);
        }

        public async Task<IActionResult> Receivables()
        {
            vm.AccountReceiables = await _accountreceivable.GetReceivables();
            return View(vm);
        }
        public async Task<IActionResult> ReceivablesCustomers(Guid CustomerId)
        {
            vm.AccountReceiables = await _accountreceivable.ReceivablebyCustomer(CustomerId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReceivePayment(Guid arId, Guid customerId, decimal amount)
        {
            try
            {


                if (amount <= 0) return Json(new { success = false, message = "Invalid amount" });

                var ar = await _onedb.AccountReceivables.FirstOrDefaultAsync(a => a.Arid == arId && a.CustomerId == customerId);
                if (ar == null) return Json(new { success = false, message = "Receivable record not found" });

                // Update AR
                ar.ReceivedAmount = (ar.ReceivedAmount ?? 0M) + amount;
                var invoice = _onedb.InvoiceMasters.Where(x => x.InvoiceMasterId == ar.InvoiceId).FirstOrDefault();

                // update status
                if (ar.ReceivedAmount >= ar.Amount)
                {
                    ar.Status = AppConstants.PaymentStatus.Paid.ToString();

                    // Also update linked invoice (fully paid)
                    if (invoice != null)
                    {
                        invoice.PaymentStatusId = (int)AppConstants.PaymentStatus.Paid;
                    }
                }
                else if (ar.ReceivedAmount > 0)
                {
                    ar.Status = AppConstants.PaymentStatus.PartiallyPaid.ToString();

                    // Invoice partially paid
                    if (invoice != null)
                    {
                        invoice.PaymentStatusId = (int)AppConstants.PaymentStatus.PartiallyPaid;
                    }
                }
                else
                {
                    ar.Status = AppConstants.PaymentStatus.Pending.ToString();

                    // Invoice still unpaid
                    if (invoice != null)
                    {
                        invoice.PaymentStatusId = (int)AppConstants.PaymentStatus.Pending;
                    }
                }
                // Save AR first (so we have latest balance)
                await _onedb.SaveChangesAsync();
                var newVoucherNumber = await _journalRepo.GetNewJournalNumber();
                // Create Journal Entry and JournalLines (use your accounting repo/service ideally)
                var journalEntry = new JournalEntry
                {
                    JournalEntryId = Guid.NewGuid(),
                    EntryDate = DateTime.UtcNow,
                    ReferenceNumber = ar.Invoice.InvoiceNo?.ToString(),
                    Description = $"Payment received for Invoice {ar.InvoiceId}",
                    BranchId = ar.BranchId,
                    SourceModule = "AR Payment",
                    CreatedBy = AppDataUtility.SessionUser.Id,
                    CreatedAt = DateTime.UtcNow,
                    EntryNumber= newVoucherNumber
                };
                _onedb.JournalEntries.Add(journalEntry);

                // Debit: Cash/Bank (use a mapped COA id for Cash)
                _onedb.JournalLines.Add(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = AppConstants.CoaAccounts.Cash,
                    Description = "Cash received",
                    Debit = amount,
                    Credit = 0
                });

                // Credit: Accounts Receivable
                _onedb.JournalLines.Add(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = AppConstants.CoaAccounts.AccountsReceivable,
                    Description = $"Payment applied to AR {arId}",
                    Debit = 0,
                    Credit = amount
                });

                await _onedb.SaveChangesAsync();

                var newBalance = ar.Amount - ar.ReceivedAmount;

                return Json(new
                {
                    success = true,
                    newBalance = newBalance,
                    status = ar.Status,
                    receivedAmount = ar.ReceivedAmount,
                    totalAmount = ar.Amount
                });
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //public IActionResult JournalEntries()
        //{
        //    return View();
        //}




        //public IActionResult Receivables_Invoices()
        //{
        //    return View();
        //}

        //public IActionResult Receivables_Payments()
        //{
        //    return View();
        //}

        //public IActionResult Receivables_Statements()
        //{
        //    return View();
        //}


        //public IActionResult Payables_Suppliers()
        //{
        //    return View();
        //}

        //public IActionResult Payables_Bills()
        //{
        //    return View();
        //}

        //public IActionResult Payables_Payments()
        //{
        //    return View();
        //}

        //public IActionResult Payables_Statements()
        //{
        //    return View();
        //}


        //public IActionResult Taxation()
        //{
        //    return View();
        //}


        //public IActionResult Reconciliation()
        //{
        //    return View();
        //}
    }

}
