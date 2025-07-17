using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.SuppliersRP
{
    public class SupplierRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        public SupplierRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
        }
        public async Task<List<SupplierVM>> GetSuppliers()
        {
            return await _onedb.Suppliers.Select(x => new SupplierVM
            {
                SupplierId = x.SupplierId,
                Ntn = x.Ntn,
                SupplierCode = x.SupplierCode,
                SupplierBusinessName = x.SupplierBusinessName
            }).ToListAsync();
        }
        public async Task<int> AddSupplier(SupplierVM s)
        {
            int maxLaneAddress = _onedb.LaneAddresses.Any()
               ? _onedb.LaneAddresses.Max(x => x.AddressId)
               : 0;
            int maxpersonId = _onedb.Persons.Any()
              ? _onedb.Persons.Max(x => x.Id)
              : 0;
            int maxSupplierId = _onedb.Suppliers.Any()
           ? _onedb.Suppliers.Max(x => x.SupplierId)
           : 0;
            int maxSupplierContactId = _onedb.SupplierContacts.Any()
           ? _onedb.SupplierContacts.Max(x => x.SpplierContactId)
           : 0;
            var currentTime = DateTime.UtcNow;
            var toaddaddress = s.Person.LaneAddress.FirstOrDefault();
            List<LaneAddress> addresses = new List<LaneAddress>
            {
                new LaneAddress
                {
                     LaneAddressOne=toaddaddress.LaneAddressOne,
                     LaneAddressTwo= toaddaddress.LaneAddressTwo,
                     FamousPlace= toaddaddress.FamousPlace,
                     Area=toaddaddress.Area,
                     CityId=toaddaddress.CityId,
                      AddressId = maxLaneAddress+1
                }
            };
            var person = new Person
            {
                Id = maxpersonId + 1,
                Cnic = s.Person.Cnic,
                SocialSecurity = s.Person.SocialSecurity,
                Email = s.Person.Email,
                MobileNumber = s.Person.MobileNumber,
                FirstName = s.Person.FirstName,
                LastName = s.Person.LastName,
                LaneAddresses = addresses,
                Createdby = AppDataUtility.SessionUser.Id,
                CreatedOn = currentTime,
                IsActive = true,
                IsDeleted = false

            };
            List<SupplierContact> newSupplierContact = new List<SupplierContact>
            {
                 new SupplierContact
                 {
                     SpplierContactId=maxSupplierContactId+1,
                      Person=person,
                       IsActive=true,
                        IsDeleted=false,
                         CreatedOn=currentTime,
                         ModifiedOn=currentTime,


                 }
            };

            var newSupplier = new Supplier
            {
                SupplierId = maxSupplierId + 1,
                Ntn = s.Ntn,
                SupplierBusinessName = s.SupplierBusinessName,
                SupplierCode = s.SupplierCode,
                SupplierContacts = newSupplierContact,
                IsActive = true,
                CreatedOn = currentTime,


            };
            var result = await _onedb.Suppliers.AddAsync(newSupplier);
            var result2 = await _onedb.SaveChangesAsync();
            return result2;
        }


    }
}
