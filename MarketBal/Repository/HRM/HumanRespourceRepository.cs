using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.HRM
{
    public class HumanRespourceRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public HumanRespourceRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<List<EmployeeVM>> GetSaleStaff()
        {
            return await _onedb.Employees.Where(x=>x.IsSalePerson==true).Select(x => new EmployeeVM
            {
                EmployeeCode = x.EmployeeCode,
                EmployeeId = x.EmployeeId,
                IsSalePerson = x.IsSalePerson,

            }).ToListAsync();
        }
    }
}
