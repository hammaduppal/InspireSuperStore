using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;

namespace MarketBal.Repository
{
    public class AdminPanelRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        public AdminPanelRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
        }
        public async Task<List<LoginUserVM>> GetLoginUser()
        {
            string query = $@"select * from Hrm.LoginUsers lu JOIN Hrm.Persons p on lu.PersonId = p.Id";
            var result = await _db.ExecuteQueryList<LoginUserVM>(query);
            return result.ToList();
        }
        public async Task<LoginUserVM> GetLoginUser(int Id)
        {
            //var rr = _onedb.LoginUsers.Where(x => x.Id == Id).Select(lu => new LoginUserVM
            //{
            //    UserName = lu.UserName,
            //    PersonId = lu.PersonId,
            //    Id = lu.Id,
            //    Passwords = lu.Passwords,
            //    PersonVM = new PersonVM
            //    {
            //        Id = lu.Person.Id,
            //        FirstName = lu.Person.FirstName,
            //        LastName = lu.Person.LastName,
            //        Cnic = lu.Person.Cnic,
            //        SocialSecurity = lu.Person.SocialSecurity,
            //        Email = lu.Person.Email,
            //        MobileNumber = lu.Person.MobileNumber,

            //    }


            //}).FirstOrDefault();
            string query = $@"select * from Hrm.LoginUsers lu JOIN Hrm.Persons p on lu.PersonId = p.Id where lu.Id= @Id";
            var loginparam = new
            {
                Id
            };
            var loginUser = await _db.ExecuteQuery<LoginUserVM>(query, loginparam);
            loginUser.Passwords = EncryptionPasses.Decrypt(loginUser.Passwords, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE);
            query = "select * from HRM.Persons where HRM.Persons.Id=@PersonId";
            var personParam = new
            {
                loginUser.PersonId
            };
            var person = await _db.ExecuteQuery<PersonVM>(query, personParam);
            loginUser.PersonVM = person;
            query = $"select * from SYSTEM.AssignedRoles ar join SYSTEM.Roles r on ar.RoleId=r.Id where ar.LoginId={loginUser.Id}";
            var roles = await _db.ExecuteQueryList<RolesVM>(query);
            loginUser.Roles = roles.ToList();

            query = "select * from Hrm.LaneAddresses where PersonId =@PersonId";
            var addressParam = new
            {
                loginUser.PersonId
            };
            var addresses = await _db.GetDataListWithQueryAndParam<LaneAddressVM>(query, addressParam);
            loginUser.PersonVM.LaneAddress = addresses.ToList();



            foreach (var item in addresses)
            {
                query = "select * from HRM.Cities where HRM.Cities.CityId=@CityId";
                var cityParam = new
                {
                    item.CityId
                };
                var city = await _db.GetSingleItemDatatWithQueryAndParam<CityVM>(query, cityParam);
                item.City = city;
                query = "select * from HRM.StateProvince where StateProvinceId =@StateProvinceId";
                var stateParam = new
                {
                    city.StateProvinceId
                };
                var state = await _db.GetSingleItemDatatWithQueryAndParam<StateProvinceVM>(query, stateParam);
                city.StateProvince = state;


                query = "select * from HRM.Countries where CountryId= @CountryId";
                var countryParam = new
                {
                    state.CountryId
                };
                var country = await _db.GetSingleItemDatatWithQueryAndParam<CountryVM>(query, countryParam);

                state.Country = country;





            }
            return loginUser;
        }

        public async Task<List<RolesVM>> GetRoles()
        {
            return await _onedb.Roles.Select(r => new RolesVM
            {
                Id = r.Id,
                Name = r.Name,
                IsActive = r.IsActive
            }).ToListAsync();
        }
        public async Task<int> UpdateRoles(AssignRolesVM data)
        {
            // Remove existing roles for the user
            var previousRoles = _onedb.AssignedRoles.Where(x => x.LoginId == data.Id);
            _onedb.AssignedRoles.RemoveRange(previousRoles);

            if (data.RoleIds?.Any() == true)
            {
                int maxId = _onedb.AssignedRoles.Any()
                    ? _onedb.AssignedRoles.Max(x => x.Id)
                    : 0;

                var now = DateTime.Now;
                var newRoles = data.RoleIds.Select(roleId => new AssignedRole
                {
                    Id = ++maxId,
                    LoginId = data.Id,
                    RoleId = roleId,
                    CreatedOn = now,
                    ModifiedOn = now,
                    IsActive = true,
                    IsDeleted = false,
                    Createdby = 1 // Replace with logged-in user id if available
                }).ToList();

                await _onedb.AssignedRoles.AddRangeAsync(newRoles);
            }

            await _onedb.SaveChangesAsync();

            return 1;
        }
        public async Task<int> UpdateLoginUser(LoginUserVM vm)
        {
            var currentTime = DateTime.UtcNow;
            var user = _onedb.LoginUsers.Include("Person").Where(x => x.Id == vm.Id).FirstOrDefault();
            user.UserName = !string.IsNullOrWhiteSpace(vm.UserName) ? vm.UserName : user.UserName;
            user.Passwords = !string.IsNullOrWhiteSpace(vm.Passwords) ? EncryptionPasses.Encrypt(vm.Passwords, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE) : user.Passwords;
            user.Person.FirstName = !string.IsNullOrWhiteSpace(vm.PersonVM.FirstName) ? vm.PersonVM.FirstName : user.Person.FirstName;
            user.Person.LastName = !string.IsNullOrWhiteSpace(vm.PersonVM.LastName) ? vm.PersonVM.LastName : user.Person.LastName;
            user.Person.Cnic = !string.IsNullOrWhiteSpace(vm.PersonVM.Cnic) ? vm.PersonVM.Cnic : user.Person.Cnic;
            user.Person.SocialSecurity = !string.IsNullOrWhiteSpace(vm.PersonVM.SocialSecurity) ? vm.PersonVM.SocialSecurity : user.Person.SocialSecurity;
            user.Person.MobileNumber = !string.IsNullOrWhiteSpace(vm.PersonVM.MobileNumber) ? vm.PersonVM.MobileNumber : user.Person.MobileNumber;
            user.Person.Email = !string.IsNullOrWhiteSpace(vm.PersonVM.Email) ? vm.PersonVM.Email : user.Person.Email;
            user.ModifiedOn = currentTime;
            user.Person.ModifiedOn = currentTime;
            _onedb.LoginUsers.Update(user);
            var result = _onedb.SaveChanges();
            return result;
        }

        public async Task<List<CountryVM>> Countries()
        {
            return _onedb.Countries.Select(x => new CountryVM
            {
                CountryId = x.CountryId,
                CountryName = x.CountryName
            }).ToList();
        }
        public async Task<List<StateProvinceVM>> GetStatesByCountryId(int? Id)
        {
            return _onedb.StateProvinces.Where(x => x.CountryId == Id).Select(x => new StateProvinceVM
            {
                CountryId = x.CountryId,
                StateProvinceName = x.StateProvinceName,
                StateProvinceId = x.StateProvinceId
            }).ToList();
        }
        public async Task<List<CityVM>> GetCityByStateId(int? Id)
        {
            return _onedb.Cities.Where(x => x.StateProvinceId == Id).Select(x => new CityVM
            {
                CityId = x.CityId,
                CityName = x.CityName
            }).ToList();
        }

        public async Task<int> UpdateAddress(LaneAddressVM model)
        {
            var result = _onedb.LaneAddresses.Where(x => x.AddressId == model.AddressId).FirstOrDefault();
            result.Area = model.Area;
            result.LaneAddressOne = model.LaneAddressOne;
            result.LaneAddressTwo = model.LaneAddressTwo;
            result.FamousPlace = model.FamousPlace;
            result.CityId = model.CityId;
            _onedb.LaneAddresses.Update(result);
            var updateResult = _onedb.SaveChanges();
            return updateResult;
        }
        public async Task<int> AddNewUser(LoginUserVM model)
        {
            var existingUser = await _onedb.LoginUsers.Include("Person").Where(x => x.UserName == model.UserName).FirstOrDefaultAsync();
            if (existingUser != null)
            {
                return -3;
            }
            var existingPerson = await _onedb.Persons.Where(x => x.Cnic == model.PersonVM.Cnic || x.MobileNumber == model.PersonVM.MobileNumber).FirstOrDefaultAsync();

            if (existingPerson != null)
            {
                return -4;
            }
            int maxLoginId = _onedb.LoginUsers.Any()
                    ? _onedb.LoginUsers.Max(x => x.Id)
                    : 0;
            int maxPersonId = _onedb.Persons.Any()
                  ? _onedb.Persons.Max(x => x.Id)
                  : 0;
            int maxAddressId = _onedb.LaneAddresses.Any()
                  ? _onedb.LaneAddresses.Max(x => x.AddressId)
                  : 0;
            var addone = model.PersonVM.LaneAddress.FirstOrDefault();
            var addressBilling = new LaneAddress
            {
                AddressId = maxAddressId + 1,
                LaneAddressOne = addone.LaneAddressOne,
                LaneAddressTwo = addone.LaneAddressTwo,
                Area = addone.Area,
                FamousPlace = addone.FamousPlace,
                CityId = addone.CityId,
                AddressType = "Billing",


            };
            var addressShipping = new LaneAddress
            {
                AddressId = maxAddressId + 2,
                LaneAddressOne = addone.LaneAddressOne,
                LaneAddressTwo = addone.LaneAddressTwo,
                Area = addone.Area,
                FamousPlace = addone.FamousPlace,
                CityId = addone.CityId,
                AddressType = "Shipping"
            };

            var addresses = new List<LaneAddress>();
            addresses.Add(addressShipping);
            addresses.Add(addressBilling);
            var currentTime = DateTime.UtcNow;
            var assignBranches = new List<UserAssignedBranch>();

            var assignBranch = new UserAssignedBranch
            {
                BranchId = model.PersonVM.BranchId,
                LoginUserId = maxLoginId,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = currentTime,
                Createdby = AppDataUtility.SessionUser.Id

            };
            assignBranches.Add(assignBranch);
            var newUser = new LoginUser
            {
                Id = maxLoginId + 1,
                UserName = model.UserName,
                Passwords = EncryptionPasses.Encrypt(model.Passwords, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE),
                CreatedOn = currentTime,
                ModifiedOn = currentTime,
                IsActive = true,
                IsDeleted = false,
                Createdby = AppDataUtility.SessionUser.Id, UserAssignedBranches=assignBranches,
                Person = new Person
                {
                    Id = maxPersonId + 1,
                    Cnic = model.PersonVM.Cnic,
                    Email = model.PersonVM.Email,
                    SocialSecurity = model.PersonVM.SocialSecurity,
                    LastName = model.PersonVM.LastName,
                    FirstName = model.PersonVM.FirstName,
                    MobileNumber = model.PersonVM.MobileNumber,
                    LaneAddresses = addresses,
                    BrancId = model.PersonVM.BranchId,
                    CreatedOn = currentTime,
                    ModifiedOn = currentTime,
                    IsActive = true,
                    IsDeleted = false,
                    Createdby = AppDataUtility.SessionUser.Id


                }
            };
            _onedb.LoginUsers.Add(newUser);
         
            var result = _onedb.SaveChanges();

            return result;
        }

        public async Task<int> ActiveDeactiveUser(LoginUserVM model)
        {
            var user = await _onedb.LoginUsers.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (user!=null)
            {
                user.IsActive = model.IsActive;

            }
            _onedb.LoginUsers.Update(user);
            return await  _onedb.SaveChangesAsync();
        }
        public async Task<List<RolesVM>> Roles()
        {
            return await _onedb.Roles
                .Where(x => x.Id != 1)
                .Select(x => new RolesVM
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<int> AddRole(RolesVM role)
        {
            var existing = await _onedb.Roles.Where(x => x.Name == role.Name).FirstOrDefaultAsync();
            if (existing!=null)
            {
                return -1;
            }
            int maxRoleId = _onedb.Roles.Any()
                  ? _onedb.Roles.Max(x => x.Id)
                  : 0;
            var newrole = new Role
            {
                 Id=maxRoleId+1,
                 Name = role.Name,
                 IsActive =true
            };
           await _onedb.Roles.AddAsync(newrole);
            return await _onedb.SaveChangesAsync();
        }













        public async Task<List<OrganizationVM>> GetOrganizations()
        {
            return await _onedb.Organizations.Select(x => new OrganizationVM
            {
                 OrganizationId=x.OrganizationId,
                 OrganizationName=x.OrganizationName
            }).ToListAsync();
        }
        public async Task<List<BranchVM>> GetBranches(int OrganizationId)
        {
            return await _onedb.Branches.Where(o=>o.OrganizationId==OrganizationId).Select(x => new BranchVM
            {
                BranchId = x.BranchId,
                 BranchName = x.BranchName
            }).ToListAsync();
        }

    }
}
