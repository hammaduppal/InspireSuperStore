using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.DCS
{
    public class DCSRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        public DCSRepository(IConfiguration config)
        {
            _config = config;
            _db = new DBManager(_config);
        }

        

    }
}
