using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository
{
    public class AssetRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public AssetRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<List<ServingTableVM>> ServingTables()
        {

            return await _onedb.ServingTables.Select(x => new ServingTableVM
            {
                ServingTableId = x.ServingTableId,
                SittingCapacity = x.SittingCapacity,
                BuildingName = x.Floor.Building.BuildingName,
                FloorName = x.Floor.FloorName,
                TableName = x.TableName,

            }).ToListAsync();
        }
        public async Task<List<PaymentMethodVM>> PaymentMethods()
        {
            return await _onedb.PaymentMethods.Select(x => new PaymentMethodVM
            {
                PaymentMethodId = x.PaymentMethodId,
                Name = x.Name,

            }).ToListAsync();
        }
    }
}
