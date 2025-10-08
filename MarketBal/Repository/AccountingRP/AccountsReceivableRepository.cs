using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.AccountingRP
{
    public class AccountsReceivableRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        public AccountsReceivableRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
        }
        public async Task<int> AddCreditSale(InvoiceMaster invoiceMaster, Customer customer, JournalEntry journalEntry)
        {
            var ar = new AccountReceivable
            {
                Arid = Guid.NewGuid(),
                CustomerId = customer.CustomerId,
                InvoiceId = invoiceMaster.InvoiceMasterId,
                JournalEntryId = journalEntry.JournalEntryId,
                Amount = invoiceMaster.GrandTotal.Value,     // Full invoice amount
                ReceivedAmount = 0,                    // None received yet
                Balance = invoiceMaster.GrandTotal,    // Customer owes this much
                DueDate = DateTime.Now.AddDays(10),       // Optional, if you track credit terms
                Status = AppConstants.PaymentStatus.Pending.ToString(),
                BranchId = AppDataUtility.SessionUser.Person.Branch.BranchId,
                CreatedBy = AppDataUtility.SessionUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _onedb.AccountReceivables.AddAsync(ar);
            return await _onedb.SaveChangesAsync();
        }
        public async Task<List<AccountReceivableVM>> GetReceivables()
        {
            var result = await _onedb.AccountReceivables.Where(x => x.Status != AppConstants.PaymentStatus.Paid.ToString())
                .GroupBy(x => x.CustomerId)
                .Select(g => new AccountReceivableVM
                {
                    CustomerId = g.Key,
                    CustomerName = g.Max(x => x.Customer.Person.FirstName),
                    CustomerCode = g.Max(x => x.Customer.CustomerCode),

                    Amount = g.Sum(x => x.Amount),
                    ReceivedAmount = g.Sum(x => x.ReceivedAmount ?? 0),
                    Balance = g.Sum(x => x.Balance ?? 0),
                    DueDate = g.Max(x => x.DueDate),
                    BranchId = g.Max(x => x.BranchId),
                    Status = g.Max(x => x.Status), // optional static label
                    CreatedAt = g.Max(x => x.CreatedAt)
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<AccountReceivableVM>> ReceivablebyCustomer(Guid customerId)
        {
            return await _onedb.AccountReceivables.Where(x => x.CustomerId == customerId && x.Status != AppConstants.PaymentStatus.Paid.ToString()).Select(x => new AccountReceivableVM
            {
                Arid = x.Arid,
                CustomerId = x.CustomerId,
                CustomerName = x.Customer.Person.FirstName + x.Customer.Person.LastName,
                CustomerCode = x.Customer.CustomerCode,
                InvoiceId = x.InvoiceId,
                JournalEntryId = x.JournalEntryId,
                Amount = x.Amount,
                ReceivedAmount = x.ReceivedAmount,
                Balance = x.Balance,
                DueDate = x.DueDate,
                Status = x.Status,
                BranchId = x.BranchId,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            }).ToListAsync();
        }

        public async Task<int> ReceiveCash(AccountReceivableVM model)
        {
            var customer = await _onedb.Customers.Where(x => x.CustomerId == model.CustomerId).FirstOrDefaultAsync();
            int cashCoaId = 1;
            int receivableCoaId = 10;
            // Step 1: Update receivable
            var receivable = await _onedb.AccountReceivables.FirstOrDefaultAsync(x => x.Arid == model.Arid);
            receivable.ReceivedAmount += model.ReceivedAmount;
            receivable.Status = receivable.ReceivedAmount >= receivable.Amount ? "Paid" :
                                receivable.ReceivedAmount > 0 ? "Partial" : "Pending";
            await _onedb.SaveChangesAsync();

            // Step 2: Create journal entry
            var journalEntry = new JournalEntry
            {
                JournalEntryId = Guid.NewGuid(),
                EntryDate = DateTime.Now,
                SourceModule = "Accounts Receivable Payment",
                ReferenceNumber = receivable.InvoiceId.ToString(),
                Description = $"Payment received from {customer.Person.FirstName}",
                BranchId = receivable.BranchId,
                CreatedBy = AppDataUtility.SessionUser.Id,
                CreatedAt = DateTime.Now
            };
            await _onedb.JournalEntries.AddAsync(journalEntry);

            // Step 3: Journal lines
            await _onedb.JournalLines.AddRangeAsync(new List<JournalLine>
            {
                new()
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = cashCoaId, // e.g., Cash or Bank Account
                    Description = "Cash received from customer",
                    Debit = model.ReceivedAmount,
                    Credit = 0
                },
                new()
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = receivableCoaId, // Accounts Receivable
                    Description = "Reduce customer receivable",
                    Debit = 0,
                    Credit = model.ReceivedAmount
                }
            });
            await _onedb.SaveChangesAsync();

            // Step 4: Update invoice
            var invoice = await _onedb.InvoiceMasters.FirstOrDefaultAsync(x => x.InvoiceMasterId == receivable.InvoiceId);
            invoice.PaymentStatusId = (int)AppConstants.PaymentStatus.Paid;
            await _onedb.SaveChangesAsync();
            return 1;
        }
    }
}
