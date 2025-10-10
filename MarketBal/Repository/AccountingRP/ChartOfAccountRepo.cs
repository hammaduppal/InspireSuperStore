using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace MarketBal.Repository.AccountingRP
{
    public class ChartOfAccountRepo
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        public ChartOfAccountRepo(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
        }
        public async Task<List<ChartOfAccountVM>> GetAllChartOfAccounts()
        {
            try
            {
                var allAccounts = await _onedb.ChartOfAccounts
             .Select(x => new ChartOfAccountVM
             {
                 CoaId = x.CoaId,
                 AccountCode = x.AccountCode,
                 AccountName = x.AccountName,
                 AccountType = x.AccountType,
                 ParentCoaId = x.ParentCoaId,
                 IsActive = x.IsActive,
                 CreatedAt = x.CreatedAt,
             })
             .ToListAsync();

                // 2️⃣ Recursive build
                List<ChartOfAccountVM> BuildTree(List<ChartOfAccountVM> all, int? parentId)
                {
                    return all
                        .Where(x => x.ParentCoaId == parentId)
                        .Select(x => new ChartOfAccountVM
                        {
                            CoaId = x.CoaId,
                            AccountCode = x.AccountCode,
                            AccountName = x.AccountName,
                            AccountType = x.AccountType,
                            ParentCoaId = x.ParentCoaId,
                            IsActive = x.IsActive,
                            CreatedAt = x.CreatedAt,
                            Children = BuildTree(all, x.CoaId) // recursion here 🔁
                        })
                        .ToList();
                }

                // 3️⃣ Start recursion from root (ParentCoaId == null)
                return BuildTree(allAccounts, null);

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching chart of accounts.", ex);
            }
        }

        public async Task<List<ChartOfAccountVM>> GetChildChartOfAccounts()
        {
            try
            {
                return await _onedb.ChartOfAccounts.Where(x=>x.ParentCoaId!=null)
             .Select(x => new ChartOfAccountVM
             {
                 CoaId = x.CoaId,
                 AccountCode = x.AccountCode,
                 AccountName = x.AccountName,
                 AccountType = x.AccountType,
                 ParentCoaId = x.ParentCoaId,
                 IsActive = x.IsActive,
                 CreatedAt = x.CreatedAt,
             })
             .ToListAsync();

           

            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching chart of accounts.", ex);
            }
        }

        public async Task<List<TrialBalanceVM>> GetTrialBalances(DateTime? fromDate, DateTime? toDate)
        {
            fromDate ??= new DateTime(DateTime.Now.Year, 1, 1);
            toDate ??= DateTime.Now;

            var data = await (from jl in _onedb.JournalLines
                              join coa in _onedb.ChartOfAccounts on jl.CoaId equals coa.CoaId
                              join je in _onedb.JournalEntries on jl.JournalEntryId equals je.JournalEntryId
                              where je.EntryDate >= fromDate && je.EntryDate <= toDate
                              group jl by new { jl.CoaId, coa.AccountCode, coa.AccountName, coa.AccountType } into g
                              select new TrialBalanceVM
                              {
                                  CoaId = g.Key.CoaId,
                                  AccountCode = g.Key.AccountCode,
                                  AccountName = g.Key.AccountName,
                                  AccountType = g.Key.AccountType,
                                  TotalDebit = g.Sum(x => x.Debit ?? 0),
                                  TotalCredit = g.Sum(x => x.Credit ?? 0)
                              }).OrderBy(x => x.AccountCode)
                              .ToListAsync();

    
            return data;
        }
        public async Task<List<LedgerVM>> GetLedger(int coaId, DateTime? from, DateTime? to)
        {
            var query = _onedb.JournalLines
                .Include(j => j.JournalEntry)
                .Where(j => j.CoaId == coaId);

            if (from.HasValue && to.HasValue)
                query = query.Where(j => j.JournalEntry.EntryDate >= from && j.JournalEntry.EntryDate <= to);

            var data = await query
                .OrderBy(j => j.JournalEntry.EntryDate)
                .Select(j => new LedgerVM
                {
                    EntryDate = j.JournalEntry.EntryDate,
                    Description = j.Description,
                    ReferenceNumber = j.JournalEntry.ReferenceNumber,
                    Debit = j.Debit ?? 0,
                    Credit = j.Credit ?? 0
                })
                .ToListAsync();

            decimal balance = 0;
            foreach (var item in data)
            {
                balance += (item.Debit - item.Credit);
                item.RunningBalance = balance;
            }

            return data;
        }

    }
}
