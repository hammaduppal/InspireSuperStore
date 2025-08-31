using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.DashBoard
{
    public class DashBoardRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly OneDb _onedb;
        public DashBoardRepository(IConfiguration config, OneDb onedb)
        {
            _config = config;
            _db = new DBManager(_config);
            _onedb = onedb;
        }
        public async Task<DashBoardSettings> Settings()
        {
            string query = $@"SELECT 
                            (SELECT COUNT(*) FROM Inv.Products) AS TotalProducts,
                            (SELECT COUNT(*) FROM Inv.ProductVariants) AS TotalVariants,
                            (SELECT COUNT(*) FROM HRM.LoginUsers) AS TotalUsers,
                            (SELECT COUNT(*) FROM Business.Organizations where IsActive =1) AS TotalOrganizations,
                            (SELECT COUNT(*) FROM Business.Branches) AS TotalBranches

";
            var param = new
            {
              
            };
            var result = await _db.GetSingleItemDatatWithQueryAndParam<DashBoardSettings>(query, param);
            result.TotalUsers = result.TotalUsers - 1;
            return result;
        }
        public async Task<List<SettingVM>> GetSettings()
        {
            return await _onedb.Settings.Where(x=>x.IsActive==true).Select(x => new SettingVM
            {
                 ApplicationId=x.ApplicationId, ApplicationName=x.ApplicationName, ApplicationUrl   =x.ApplicationUrl,
                 SettingsId=x.SettingsId, BranchId=x.BranchId,IsActive=x.IsActive
            }).ToListAsync();
        }
     
    }
}
