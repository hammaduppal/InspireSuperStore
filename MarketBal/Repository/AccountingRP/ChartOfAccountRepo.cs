using MainModels;
using MainModels.DTOModels;
using MainModels.Models;

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
                var query = "SELECT * FROM ChartOfAccounts WHERE IsActive = 1 ORDER BY AccountNumber";
                var result = await _db.GetDataListWithQueryAndParam<ChartOfAccountVM>(query);
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching chart of accounts.", ex);
            }
        }
    }
}
