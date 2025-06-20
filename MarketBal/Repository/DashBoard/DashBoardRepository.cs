using MainModels;
using MainModels.DTOModels;
using MainModels.Models;

namespace MarketBal.Repository.DashBoard
{
    public class DashBoardRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        public DashBoardRepository(IConfiguration config)
        {
            _config = config;
            _db = new DBManager(_config);
        }
        public async Task<DashBoardSettings> Settings()
        {
            string query = $@"SELECT 
                            (SELECT COUNT(*) FROM Inv.Products) AS TotalProducts,
                            (SELECT COUNT(*) FROM Inv.ProductVariants) AS TotalVariants";
            var param = new
            {
              
            };
            var result = await _db.GetSingleItemDatatWithQueryAndParam<DashBoardSettings>(query, param);
            return result;
        }

    }
}
