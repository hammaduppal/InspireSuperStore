using System.Diagnostics.Metrics;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.AccountingRP
{
    public class AccountPayableRP
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        public AccountPayableRP(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
        }
        public async Task<int> AddCreditPurchase(PurchaseMaster master,JournalEntry journalEntry)
        {
            _onedb.AccountPayables.Add(new AccountPayable
            {
                Apid = Guid.NewGuid(),
                Amount = master.GrandTotal.Value,
                PaidAmount = 0,
                Balance = master.GrandTotal,
                JournalEntryId = journalEntry.JournalEntryId,
                BranchId = master.BranchId.Value,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = AppDataUtility.SessionUser.Id,
                DueDate = DateTime.Now,
                PurchaseId = master.PurchaseMasterId,
                SupplierId = master.SupplierId.Value,
                Status = AppConstants.PaymentStatus.Pending.ToString(),
            });
            return await _onedb.SaveChangesAsync();
        }
        public async Task<List<AccountPayableVM>> GetPayAbles()
        {
            var result = await _onedb.AccountPayables.Where(x => x.Status != AppConstants.PaymentStatus.Paid.ToString())
                .GroupBy(x => x.SupplierId)
                .Select(g => new AccountPayableVM
                {
                    SupplierId = g.Key,
                    SupplierBusinessName = g.Max(x => x.Supplier.SupplierBusinessName),
                 

                    Amount = g.Sum(x => x.Amount),
                    PaidAmount = g.Sum(x => x.PaidAmount ?? 0),
                    Balance = g.Sum(x => x.Balance ?? 0),
                    DueDate = g.Max(x => x.DueDate),
                    BranchId = g.Max(x => x.BranchId),
                    Status = g.Max(x => x.Status), // optional static label
                    CreatedAt = g.Max(x => x.CreatedAt)
                }).ToListAsync();

            return result;
        }
        public async Task<List<AccountPayableVM>> PayableToSupplier(int supplierId)
        {
            return await _onedb.AccountPayables.Where(x => x.SupplierId == supplierId && x.Status != AppConstants.PaymentStatus.Paid.ToString()).Select(x => new AccountPayableVM
            {
                Apid = x.Apid,
                SupplierId = x.SupplierId,
                SupplierBusinessName = x.Supplier.SupplierBusinessName,
                PurchaseId = x.PurchaseId,
                JournalEntryId = x.JournalEntryId,
                Amount = x.Amount,
                PaidAmount = x.PaidAmount,
                Balance = x.Balance,
                DueDate = x.DueDate,
                Status = x.Status,
                BranchId = x.BranchId,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            }).ToListAsync();
        }
    }
}
