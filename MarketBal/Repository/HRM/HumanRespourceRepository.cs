using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.HRM
{
    public class HumanRespourceRepository
    {
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public HumanRespourceRepository(IConfiguration config, OneDb oneDb, ISessionService sessionService)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config,_onedb,_sessionService);
        }
        public async Task<List<EmployeeVM>> GetSaleStaff()
        {
            return await _onedb.Employees.Where(x=>x.IsSalePerson==true).Select(x => new EmployeeVM
            {
                EmployeeCode = x.EmployeeCode, FirstName= x.Person.FirstName, LastName = x.Person.LastName,
                EmployeeId = x.EmployeeId,
                IsSalePerson = x.IsSalePerson,

            }).ToListAsync();
        }

        public async Task<Customer> GetCustomer(Guid CustomerId)
        {
            return await _onedb.Customers.Include(x=>x.Person).FirstOrDefaultAsync(x => x.CustomerId == CustomerId && x.IsDeleted == false && x.IsActive==true);
        }

        public async Task<List<Customer>> GetAllActiveCustomers()
        {
            return await _onedb.Customers.Include(x => x.Person).Where(x => x.IsDeleted == false && x.IsActive == true).ToListAsync();
        }


        public async Task<List<CityVM>> GetCitybyCountry(string countryName)
        {
            return await _onedb.Cities.Where(x=>x.StateProvince.Country.CountryName==countryName).Select(x => new CityVM
            {
                CityId = x.CityId,
                CityName = x.CityName
            }).ToListAsync();
        }
    }
}
