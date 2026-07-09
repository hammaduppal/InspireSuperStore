using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.SystemRP
{
    public class SystemRepository
    {
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public SystemRepository(IConfiguration config, OneDb oneDb, ISessionService sessionService)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config,_onedb,_sessionService);
        }
        public async Task<List<TaxSlabVM>> TaxSlabs()
        {
            return await _onedb.TaxSlabs.Select(x => new TaxSlabVM
            {
                TaxSlabId = x.TaxSlabId,
                SlabName = x.SlabName,
                Rate = x.Rate
            }).ToListAsync();
        }
    }
}
