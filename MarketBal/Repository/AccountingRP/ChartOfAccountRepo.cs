using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
