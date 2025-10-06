using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;

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
                Status = "Open",
                BranchId = AppDataUtility.SessionUser.Person.Branch.BranchId,
                CreatedBy = AppDataUtility.SessionUser.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _onedb.AccountReceivables.AddAsync(ar);
            return await _onedb.SaveChangesAsync();
        }
    }
}
