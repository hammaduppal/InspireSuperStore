using System.Collections;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper.Excel;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Microsoft.IdentityModel.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace MarketBal.Repository
{
    public class AdminPanelRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly ISessionService _sessionService;
        
        public AdminPanelRepository(IConfiguration config, OneDb oneDb, ISessionService sessionService)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config,_onedb,_sessionService);
        }

        public async Task<List<LoginUserVM>> GetLoginUser()
        {
            var r = await _onedb.LoginUsers
      .Where(x => x.Id != 1)
      .Select(x => new LoginUserVM
      {
          Id = x.Id,
          UserName = x.UserName,
          IsActive = x.IsActive.Value,
          CreatedOn = x.CreatedOn,
          Person = new PersonVM
          {
              FirstName = x.Person.FirstName,
              LastName = x.Person.LastName,
              Email = x.Person.Email,
              MobileNumber = x.Person.MobileNumber,
              IsActive = x.IsActive,
              CreatedOn = x.CreatedOn,
              Branch = new BranchVM
              {
                  BranchId = x.Person.Branch.BranchId,
                  BranchName = x.Person.Branch.BranchName,
                  Organization = new OrganizationVM
                  {
                      OrganizationId = x.Person.Branch.Organization.OrganizationId,
                      OrganizationName = x.Person.Branch.Organization.OrganizationName
                  }
              }
          }
      })
      .ToListAsync();
            return r;
            //string query = $@"select  * from Hrm.LoginUsers l where l.Id !=1";
            //var result = await _db.ExecuteQueryList<LoginUserVM>(query);
            //return result.ToList();
        }
        public async Task<List<EmployeeVM>> GetEmployees()
        {
            var r = await _onedb.Employees

      .Select(x => new EmployeeVM
      {
          EmployeeId = x.EmployeeId,
          EmployeeCode = x.EmployeeCode,
          IsSalePerson = x.IsSalePerson.Value,
          IsActive = x.IsActive.Value,
          CreatedOn = x.CreatedOn,
            EmployeeDepartment=new EmployeeDepartmentVM
            {
                 Title=x.Department.Title
            },
            EmployeeDesignation = new EmployeeDesignationVM
            {
                 Title=x.Designation.Title
            },
          Person = new PersonVM
          {
              FirstName = x.Person.FirstName,
              LastName = x.Person.LastName,
              Email = x.Person.Email,
              MobileNumber = x.Person.MobileNumber,
              IsActive = x.IsActive,
              CreatedOn = x.CreatedOn,
              Branch = new BranchVM
              {
                  BranchId = x.Person.Branch.BranchId,
                  BranchName = x.Person.Branch.BranchName,
                  Organization = new OrganizationVM
                  {
                      OrganizationId = x.Person.Branch.Organization.OrganizationId,
                      OrganizationName = x.Person.Branch.Organization.OrganizationName
                  }
              }
          }
      })
      .ToListAsync();
            return r;
            //string query = $@"select  * from Hrm.LoginUsers l where l.Id !=1";
            //var result = await _db.ExecuteQueryList<LoginUserVM>(query);
            //return result.ToList();
        }
        public async Task<int> EmployeeIsSalePerson(EmployeeVM model)
        {
            var employee = await _onedb.Employees.Where(x => x.EmployeeId == model.EmployeeId).FirstOrDefaultAsync();
            employee.IsSalePerson = model.IsSalePerson;
           return await  _onedb.SaveChangesAsync();
        }
        public async Task<List<EmployeeDepartmentVM>> GetEmployeeDepartments()
        {
            return await _onedb.EmployeeDepartments.Select(x => new EmployeeDepartmentVM
            {
                EmployeeDepartmentId = x.EmployeeDepartmentId,
                Title = x.Title
            }).ToListAsync();
        }
        public async Task<List<EmployeeDesignationVM>> GetEmployeeDesignations()
        {
            return await _onedb.EmployeeDesignations.Select(x => new EmployeeDesignationVM
            {
                Id = x.Id,
                Title = x.Title
            }).ToListAsync();
        }
        public async Task<int> AddEmployeeDepartment(EmployeeDepartmentVM model)
        {
            int maxId = _onedb.EmployeeDepartments
                  .Select(x => (int?)x.EmployeeDepartmentId)
                  .Max() ?? 0;
            var EmployeDepart = new EmployeeDepartment { Title = model.Title, EmployeeDepartmentId = maxId + 1 };
            await _onedb.EmployeeDepartments.AddAsync(EmployeDepart);
            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> AddEmployeeDesignation(EmployeeDesignationVM model)
        {
            int maxId = _onedb.EmployeeDesignations
                  .Select(x => (int?)x.Id)
                  .Max() ?? 0;
            var EmployeDepart = new EmployeeDesignation { Title = model.Title, Id= maxId + 1 };
            await _onedb.EmployeeDesignations.AddAsync(EmployeDepart);
            return await _onedb.SaveChangesAsync();
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
            string query = $@"select * from Hrm.LoginUsers lu  where lu.Id= @Id";
            var loginparam = new
            {
                Id
            };
            var loginUser = await _db.ExecuteQuery<LoginUserVM>(query, loginparam);
            loginUser.Password = EncryptionPasses.Decrypt(loginUser.Password, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE);
            query = "select * from HRM.Persons where HRM.Persons.Id=@PersonId";
            var personParam = new
            {
                loginUser.PersonId
            };
            var person = await _db.ExecuteQuery<PersonVM>(query, personParam);
            loginUser.Person = person;
            query = $"select * from SYSTEM.AssignedRoles ar join SYSTEM.Roles r on ar.RoleId=r.Id where ar.LoginId={loginUser.Id}";
            var roles = await _db.ExecuteQueryList<RolesVM>(query);
            loginUser.Roles = roles.ToList();

            query = "select * from Hrm.LaneAddresses where PersonId =@PersonId";
            var addressParam = new
            {
                loginUser.PersonId
            };
            var addresses = await _db.GetDataListWithQueryAndParam<LaneAddressVM>(query, addressParam);
            loginUser.Person.LaneAddress = addresses.ToList();



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
            return await _onedb.Roles.Where(x => x.Name != UserRolesConstants.SuperAdmin).Select(r => new RolesVM
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
            user.Password = !string.IsNullOrWhiteSpace(vm.Password) ? EncryptionPasses.Encrypt(vm.Password, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE) : user.Password;
            user.Person.FirstName = !string.IsNullOrWhiteSpace(vm.Person.FirstName) ? vm.Person.FirstName : user.Person.FirstName;
            user.Person.LastName = !string.IsNullOrWhiteSpace(vm.Person.LastName) ? vm.Person.LastName : user.Person.LastName;
            user.Person.Cnic = !string.IsNullOrWhiteSpace(vm.Person.Cnic) ? vm.Person.Cnic : user.Person.Cnic;
            user.Person.SocialSecurity = !string.IsNullOrWhiteSpace(vm.Person.SocialSecurity) ? vm.Person.SocialSecurity : user.Person.SocialSecurity;
            user.Person.MobileNumber = !string.IsNullOrWhiteSpace(vm.Person.MobileNumber) ? vm.Person.MobileNumber : user.Person.MobileNumber;
            user.Person.Email = !string.IsNullOrWhiteSpace(vm.Person.Email) ? vm.Person.Email : user.Person.Email;
            user.ModifiedOn = currentTime;
            user.Person.ModifiedOn = currentTime;
            _onedb.LoginUsers.Update(user);
            var result = _onedb.SaveChanges();
            return result;
        }

        public async Task<List<CountryVM>> Countries()
        {
            return await _onedb.Countries.Select(x => new CountryVM
            {
                CountryId = x.CountryId,
                CountryName = x.CountryName
            }).ToListAsync();
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
            if (_onedb.LoginUsers.Where(x => x.UserName == model.UserName).Any())
            {
                var existingUser = await _onedb.LoginUsers.Include("Person").Where(x => x.UserName == model.UserName).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return -3;
                }
                var existingPerson = await _onedb.Persons.Where(x => x.Cnic == model.Person.Cnic || x.MobileNumber == model.Person.MobileNumber).FirstOrDefaultAsync();

                if (existingPerson != null)
                {
                    return -4;
                }
            }
            try
            {
                int maxLoginId = _onedb.LoginUsers.Any()
                    ? _onedb.LoginUsers.Max(x => x.Id)
                    : 0;
                int maxPersonId = _onedb.Persons.Any()
                      ? _onedb.Persons.Max(x => x.Id)
                      : 0;
                int maxAddressId = _onedb.LaneAddresses.Any()
                      ? _onedb.LaneAddresses.Max(x => x.AddressId)
                      : 0;
                var addone = model.Person.LaneAddress.FirstOrDefault();
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
                    BranchId = model.Person.BranchId,
                    LoginUserId = maxLoginId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedOn = currentTime,
                    Createdby = _sessionService.SessionUser.Id

                };
                assignBranches.Add(assignBranch);
                var newUser = new LoginUser
                {
                    Id = maxLoginId + 1,
                    UserName = model.UserName,
                    Password = EncryptionPasses.Encrypt(model.Password, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE),
                    CreatedOn = currentTime,
                    ModifiedOn = currentTime,
                    IsActive = true,
                    IsDeleted = false,
                    Createdby = _sessionService.SessionUser.Id,
                    UserAssignedBranches = assignBranches,
                    Person = new Person
                    {
                        Id = maxPersonId + 1,
                        Cnic = model.Person.Cnic,
                        Email = model.UserName,
                        SocialSecurity = model.Person.SocialSecurity,
                        LastName = model.Person.LastName,
                        FirstName = model.Person.FirstName,
                        MobileNumber = model.Person.MobileNumber,
                        LaneAddresses = addresses,
                        BranchId = model.Person.BranchId,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime,
                        IsActive = true,
                        IsDeleted = false,
                        Createdby = _sessionService.SessionUser.Id


                    }
                };
                _onedb.LoginUsers.Add(newUser);

                var result = _onedb.SaveChanges();
                return result;

            }
            catch (Exception )
            {

                throw;
            }


        }

        public async Task<int> ActiveDeactiveUser(LoginUserVM model)
        {
            var user = await _onedb.LoginUsers.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
            if (user != null)
            {
                user.IsActive = model.IsActive;

            }
            _onedb.LoginUsers.Update(user);
            return await _onedb.SaveChangesAsync();
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
            if (existing != null)
            {
                return -1;
            }
            int maxRoleId = _onedb.Roles.Any()
                  ? _onedb.Roles.Max(x => x.Id)
                  : 0;
            var newrole = new Role
            {
                Id = maxRoleId + 1,
                Name = role.Name,
                IsActive = true
            };
            await _onedb.Roles.AddAsync(newrole);
            return await _onedb.SaveChangesAsync();
        }

        public async Task<int> AddNewEmployee(EmployeeVM model)
        {
            //if (_onedb.LoginUsers.Where(x => x.UserName == model.UserName).Any())
            //{
            //    var existingUser = await _onedb.LoginUsers.Include("Person").Where(x => x.UserName == model.UserName).FirstOrDefaultAsync();
            //    if (existingUser != null)
            //    {
            //        return -3;
            //    }
            //    var existingPerson = await _onedb.Persons.Where(x => x.Cnic == model.Person.Cnic || x.MobileNumber == model.Person.MobileNumber).FirstOrDefaultAsync();

            //    if (existingPerson != null)
            //    {
            //        return -4;
            //    }
            //}
            try
            {
                int maxEmployeeId = _onedb.Employees.Any()
                    ? _onedb.Employees.Max(x => x.EmployeeId)
                    : 0;
                int maxPersonId = _onedb.Persons.Any()
                      ? _onedb.Persons.Max(x => x.Id)
                      : 0;
                int maxAddressId = _onedb.LaneAddresses.Any()
                      ? _onedb.LaneAddresses.Max(x => x.AddressId)
                      : 0;
                var addone = model.Person.LaneAddress.FirstOrDefault();
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
             
                var newUser = new Employee
                {
                    EmployeeId = maxEmployeeId + 1,
                    EmployeeCode = model.EmployeeCode,
                    CreatedOn = currentTime,
                    DepartmentId= model.DepartmentId,
                    DesignationId= model.DesignationId,
                    IsSalePerson=model.IsSalePerson,
                    IsActive = true,
                    Person = new Person
                    {
                        Id = maxPersonId + 1,
                        Cnic = model.Person.Cnic,
                        Email = "",
                        SocialSecurity = model.Person.SocialSecurity,
                        LastName = model.Person.LastName,
                        FirstName = model.Person.FirstName,
                        MobileNumber = model.Person.MobileNumber,
                        LaneAddresses = addresses,
                        BranchId = _sessionService.SessionUser.Person.Branch.BranchId,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime,
                        IsActive = true,
                        IsDeleted = false,
                        Createdby = _sessionService.SessionUser.Id


                    }
                };
                _onedb.Employees.Add(newUser);

                var result = _onedb.SaveChanges();
                return result;

            }
            catch (Exception e)
            {

                throw;
            }


        }







        public async Task<object> GetOrganizations(DataTableRequest request)
        {
            string tableName = "Business.Organizations";
            var columnMap = new List<string>
            {
                "OrganizationId","OrganizationName","BranchCode","IsActive","CreatedOn","ModifiedOn","CreatedBy,IsDeleted"
            };

            var queries = ParamQueries.BuildDataTableQuery(tableName, "OrganizationName", request, columnMap);
            int totalRecords = await _db.ExecuteQuery<int>(queries.TotalRecordsQuery);
            int filteredRecords = await _db.ExecuteQuery<int>(queries.FilteredRecordsQuery);
            var data = await _db.ExecuteQueryList<OrganizationVM>(queries.DataQuery);
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.ToList()
            };
        }
        public async Task<bool> AddOrganization(OrganizationVM request)
        {

            string query = "INSERT INTO Business.Organizations (OrganizationName, IsActive,CreatedOn,CreatedBy,IsDeleted) " +
                "VALUES ( @OrganizationName,@IsActive,@CreatedOn,@CreatedBy,@IsDeleted)";

            var parameters = new
            {
                OrganizationName = request.OrganizationName,
                IsActive = 1,
                CreatedOn = DateTime.Now,
                CreatedBy = _sessionService.SessionUser.Id,
                IsDeleted = 0

            };

            var data = await _db.ExecuteInsertQueryandParam(query, parameters);
            return data > 0;
        }


        public async Task<OrganizationVM> GetOrganization(int Id)
        {
            string query = $"SELECT * FROM Business.Organizations where OrganizationId='{Id}'";
            return await _db.ExecuteQuery<OrganizationVM>(query);
        }

        public async Task<bool> EditOrganization(OrganizationVM request)
        {
            string query = @"UPDATE Business.Organizations 
                     SET 
                         OrganizationName = @OrganizationName, 
                         IsActive = @IsActive, 
                         ModifiedOn = @ModifiedOn
                     WHERE 
                         OrganizationId = @OrganizationId";

            var parameters = new
            {
                OrganizationId = request.OrganizationId, // ID of the organization to be updated
                OrganizationName = request.OrganizationName,

                IsActive = request.IsActive,
                ModifiedOn = DateTime.Now, // Current timestamp for update
            };

            var result = await _db.ExecuteInsertQueryandParam(query, parameters);

            return true;
        }

        public async Task<bool> RemoveOrganization(int Id)
        {
            string query = $"UPDATE Business.Organizations SET IsDeleted=1 WHERE OrganizationId={Id}";
            var result = await _db.ExecuteInsertQueryandParam(query);

            return result > 0;
        }

        public async Task<bool> UpdateOrganization(int Id, int IsActive)
        {
            string query = $"UPDATE Business.Organizations SET IsActive={IsActive} WHERE OrganizationId={Id}";
            var result = await _db.ExecuteInsertQueryandParam(query);

            return result > 0;
        }
        public async Task<int> UpdateMasterBranch(Guid branchId, int organizationId, int isActive)
        {

            string resetQuery = $"UPDATE Business.Branches SET IsMasterBranch = 0 WHERE OrganizationId = {organizationId}";
            await _db.ExecuteInsertQueryandParam(resetQuery);

            if (isActive == 1)
            {

                string updateQuery = $"UPDATE Business.Branches SET IsMasterBranch = 1 WHERE BranchId = '{branchId}'";
                var result = await _db.ExecuteInsertQueryandParam(updateQuery);
                return result > 0 ? 1 : 0;
            }
            else
            {

                string checkQuery = $"SELECT COUNT(*) FROM Business.Branches WHERE OrganizationId = {organizationId} AND IsMasterBranch = 1";
                int activeCount = await _db.ExecuteInsertQueryandParam(checkQuery);

                if (activeCount == 0)
                {
                    // no active branch exists
                    return -1;
                }

                return 1;
            }
        }


        public async Task<List<OrganizationVM>> GetOrganizations()
        {
            return await _onedb.Organizations.Where(x => x.IsActive == true && x.IsDeleted == false).Select(x => new OrganizationVM
            {
                OrganizationId = x.OrganizationId,
                OrganizationName = x.OrganizationName
            }).ToListAsync();
        }
        public async Task<bool> AddBranch(BranchVM request)
        {

            string query = $@"INSERT INTO Business.Branches (BranchId,BranchName,OrganizationId, BranchCode,BusinessCategoryId,BusinessEntityTypeId) 
                VALUES (
NewId(),@BranchName,@OrganizationId,@BranchCode,@BusinessEntityTypeId,@BusinessCategoryId)";

            var parameters = new
            {
                BranchName = request.BranchName,
                OrganizationId = request.OrganizationId,
                BranchCode = request.BranchCode,
                request.BusinessEntityTypeId,
                request.BusinessCategoryId,
                IsActive = 1,
                CreatedOn = DateTime.Now,
                CreatedBy = _sessionService.SessionUser.Id,
                IsDeleted = 0

            };

            var data = await _db.ExecuteInsertQueryandParam(query, parameters);
            return data > 0;
        }
        public async Task<List<BranchVM>> GetBranches(int OrganizationId)
        {
            return await _onedb.Branches.Where(o => o.OrganizationId == OrganizationId).Select(x => new BranchVM
            {
                BranchId = x.BranchId,
                BranchName = x.BranchName
            }).ToListAsync();
        }
        public async Task<List<BusinessCategoryVM>> BusinessCategories()
        {
            return await _onedb.BusinessCategories.Select(x => new BusinessCategoryVM
            {
                BusinessCategoryId = x.BusinessCategoryId,
                BusinessCategoryName = x.BusinessCategoryName,

            }).ToListAsync();
        }
        public async Task<List<BusinessEntityTypeVM>> BusinessEntityType()
        {
            return await _onedb.BusinessEntityTypes.Select(x => new BusinessEntityTypeVM
            {
                BusinessEntityTypeId = x.BusinessEntityTypeId,
                BusinessEntityTypeName = x.BusinessEntityTypeName,

            }).ToListAsync();
        }
        public async Task<bool> SeedinData(OrganizationRegistrationDto model)
        {
            using var transaction = await _onedb.Database.BeginTransactionAsync();
            try
            {
                //  var removed = await removeData();
                var currentTime = DateTime.UtcNow;
                //Guid branchId = Guid.NewGuid();

                // Only proceed if tables are empty
                if (!_onedb.Organizations.Any() && !_onedb.LoginUsers.Any())
                {
                    var organization = new Organization
                    {
                        OrganizationName = model.BusinessName,
                        CreatedOn = currentTime,
                        IsActive = true,
                        IsDeleted = false,
                        ModifiedOn = currentTime
                    };

                    await _onedb.Organizations.AddAsync(organization);
                    await _onedb.SaveChangesAsync(); // Save to generate OrganizationId (if Identity)
                    var category = _onedb.BusinessCategories.Where(x => x.BusinessCategoryId == model.BusinessCategory).FirstOrDefault();
                    var businessType = _onedb.BusinessEntityTypes.Where(x => x.BusinessEntityTypeId == model.BusinessEntity).FirstOrDefault();
                    var getOrganization = _onedb.Organizations.ToList();
                    var singleOrganization = getOrganization.FirstOrDefault();
                    //var branch = new Branch
                    //{
                    //    BranchId = branchId,
                    //    BranchName = model.BranchName,
                    //    OrganizationId = singleOrganization.OrganizationId,

                    //};
                    //await _onedb.Branches.AddAsync(branch);

                    var loginUser = new LoginUser
                    {
                        Id = 1,
                        UserName = model.Email,
                        Password = EncryptionPasses.Encrypt(model.Password, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = currentTime
                    };
                    await _onedb.LoginUsers.AddAsync(loginUser);

                    //var role = new Role
                    //{
                    //    Id = 1,
                    //    Name = "SuperAdmin",
                    //    IsActive = true
                    //};
                    //await _onedb.Roles.AddAsync(role);

                    var assignedRole = new AssignedRole
                    {
                        Id = 1,
                        LoginId = loginUser.Id,
                        RoleId = 1,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime,
                        IsActive = true,
                        IsDeleted = false
                    };
                    await _onedb.AssignedRoles.AddAsync(assignedRole);

                    //var assignedBranch = new UserAssignedBranch
                    //{
                    //    LoginUserId = loginUser.Id,
                    //    BranchId = branchId,
                    //    IsActive = true,
                    //    IsDeleted = false,
                    //    CreatedOn = currentTime,
                    //    ModifiedOn = currentTime
                    //};
                    //await _onedb.UserAssignedBranches.AddAsync(assignedBranch);

                    // Save all together
                    await _onedb.SaveChangesAsync();
                    await transaction.CommitAsync();
                   // await InsertSeedData();
                    return true;
                }

                return false; // Already seeded
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the error or rethrow with more context
                throw new Exception("Seed data failed: " + ex.Message, ex);
            }
        }

        public async Task<int> removeData()
        {
            string query = $@"delete from System.AssignedRoles
            delete from SYSTEM.Roles
            delete from system.Website
            delete from system.WebsiteData
         
            delete from hrm.LaneAddresses
           delete from HRM.Cities
            delete from hrm.StateProvince

            delete from HRM.Countries
            delete from hrm.EmployeeDepartments
delete from Hrm.UserAssignedBranches
            delete from Hrm.LoginUsers
            delete from hrm.Persons
            delete from Business.Branches
            delete from Business.Organizations
select 1;
";
            return await _db.ExecuteQueryModify(query);
        }

        public async Task<int> InsertSeedData()
        {
            string query = $@"
INSERT INTO HRM.Countries(CountryId,CountryName) VALUES
(1,'AFGHANISTAN'),
(2,'ALBANIA'),
(3,'ALGERIA'),
(4,'AMERICANSAMOA'),
(5,'ANDORRA'),
(6,'ANGOLA'),
(7,'ANGUILLA'),
(8,'ANTARCTICA'),
(9,'ANTIGUAANDBARBUDA'),
(10,'ARGENTINA'),
(11,'ARMENIA'),
(12,'ARUBA'),
(13,'AUSTRALIA'),
(14,'AUSTRIA'),
(15,'AZERBAIJAN'),
(16,'BAHAMAS'),
(17,'BAHRAIN'),
(18,'BANGLADESH'),
(19,'BARBADOS'),
(20,'BELARUS'),
(21,'BELGIUM'),
(22,'BELIZE'),
(23,'BENIN'),
(24,'BERMUDA'),
(25,'BHUTAN'),
(26,'BOLIVIA'),
(27,'BOSNIAANDHERZEGOVINA'),
(28,'BOTSWANA'),
(29,'BOUVETISLAND'),
(30,'BRAZIL'),
(31,'BRITISHINDIANOCEANTERRITORY'),
(32,'BRUNEIDARUSSALAM'),
(33,'BULGARIA'),
(34,'BURKINAFASO'),
(35,'BURUNDI'),
(36,'CAMBODIA'),
(37,'CAMEROON'),
(38,'CANADA'),
(39,'CAPEVERDE'),
(40,'CAYMANISLANDS'),
(41,'CENTRALAFRICANREPUBLIC'),
(42,'CHAD'),
(43,'CHILE'),
(44,'CHINA'),
(45,'CHRISTMASISLAND'),
(46,'COCOSKEELINGISLANDS'),
(47,'COLOMBIA'),
(48,'COMOROS'),
(49,'CONGO'),
(50,'CONGO'),
(51,'COOKISLANDS'),
(52,'COSTARICA'),
(53,'COTEDIVOIRE'),
(54,'CROATIA'),
(55,'CUBA'),
(56,'CYPRUS'),
(57,'CZECHREPUBLIC'),
(58,'DENMARK'),
(59,'DJIBOUTI'),
(60,'DOMINICA'),
(61,'DOMINICANREPUBLIC'),
(62,'ECUADOR'),
(63,'EGYPT'),
(64,'ELSALVADOR'),
(65,'EQUATORIALGUINEA'),
(66,'ERITREA'),
(67,'ESTONIA'),
(68,'ETHIOPIA'),
(69,'FALKLANDISLANDSMALVINAS'),
(70,'FAROEISLANDS'),
(71,'FIJI'),
(72,'FINLAND'),
(73,'FRANCE'),
(74,'FRENCHGUIANA'),
(75,'FRENCHPOLYNESIA'),
(76,'FRENCHSOUTHERNTERRITORIES'),
(77,'GABON'),
(78,'GAMBIA'),
(79,'GEORGIA'),
(80,'GERMANY'),
(81,'GHANA'),
(82,'GIBRALTAR'),
(83,'GREECE'),
(84,'GREENLAND'),
(85,'GRENADA'),
(86,'GUADELOUPE'),
(87,'GUAM'),
(88,'GUATEMALA'),
(89,'GUINEA'),
(90,'GUINEA-BISSAU'),
(91,'GUYANA'),
(92,'HAITI'),
(93,'HEARDISLANDANDMCDONALDISLANDS'),
(94,'HOLYSEEVATICANCITYSTATE'),
(95,'HONDURAS'),
(96,'HONGKONG'),
(97,'HUNGARY'),
(98,'ICELAND'),
(99,'INDIA'),
(100,'INDONESIA'),
(101,'IRAN'),
(102,'IRAQ'),
(103,'IRELAND'),
(104,'ISRAEL'),
(105,'ITALY'),
(106,'JAMAICA'),
(107,'JAPAN'),
(108,'JORDAN'),
(109,'KAZAKHSTAN'),
(110,'KENYA'),
(111,'KIRIBATI'),
(112,'SOUTHKOREA'),
(113,'NORTHKOREA'),
(114,'KUWAIT'),
(115,'KYRGYZSTAN'),
(116,'LAOPEOPLESDEMOCRATICREPUBLIC'),
(117,'LATVIA'),
(118,'LEBANON'),
(119,'LESOTHO'),
(120,'LIBERIA'),
(121,'LIBYANARABJAMAHIRIYA'),
(122,'LIECHTENSTEIN'),
(123,'LITHUANIA'),
(124,'LUXEMBOURG'),
(125,'MACAO'),
(126,'MACEDONIA'),
(127,'MADAGASCAR'),
(128,'MALAWI'),
(129,'MALAYSIA'),
(130,'MALDIVES'),
(131,'MALI'),
(132,'MALTA'),
(133,'MARSHALLISLANDS'),
(134,'MARTINIQUE'),
(135,'MAURITANIA'),
(136,'MAURITIUS'),
(137,'MAYOTTE'),
(138,'MEXICO'),
(139,'MICRONESIA'),
(140,'MOLDOVA'),
(141,'MONACO'),
(142,'MONGOLIA'),
(143,'MONTSERRAT'),
(144,'MOROCCO'),
(145,'MOZAMBIQUE'),
(146,'MYANMAR'),
(147,'NAMIBIA'),
(148,'NAURU'),
(149,'NEPAL'),
(150,'NETHERLANDS'),
(151,'NETHERLANDSANTILLES'),
(152,'NEWCALEDONIA'),
(153,'NEWZEALAND'),
(154,'NICARAGUA'),
(155,'NIGER'),
(156,'NIGERIA'),
(157,'NIUE'),
(158,'NORFOLKISLAND'),
(159,'NORTHERNMARIANAISLANDS'),
(160,'NORWAY'),
(161,'OMAN'),
(162,'PAKISTAN'),
(163,'PALAU'),
(164,'PALESTINIANTERRITORY'),
(165,'PANAMA'),
(166,'PAPUANEWGUINEA'),
(167,'PARAGUAY'),
(168,'PERU'),
(169,'PHILIPPINES'),
(170,'PITCAIRN'),
(171,'POLAND'),
(172,'PORTUGAL'),
(173,'PUERTORICO'),
(174,'QATAR'),
(175,'REUNION'),
(176,'ROMANIA'),
(177,'RUSSIANFEDERATION'),
(178,'RWANDA'),
(179,'SAINTHELENA'),
(180,'SAINTKITTSANDNEVIS'),
(181,'SAINTLUCIA'),
(182,'SAINTPIERREANDMIQUELON'),
(183,'SAINTVINCENTANDTHEGRENADINES'),
(184,'SAMOA'),
(185,'SANMARINO'),
(186,'SAOTOMEANDPRINCIPE'),
(187,'SAUDIARABIA'),
(188,'SENEGAL'),
(189,'SERBIAANDMONTENEGRO'),
(190,'SEYCHELLES'),
(191,'SIERRALEONE'),
(192,'SINGAPORE'),
(193,'SLOVAKIA'),
(194,'SLOVENIA'),
(195,'SOLOMONISLANDS'),
(196,'SOMALIA'),
(197,'SOUTHAFRICA'),
(198,'SOUTHGEORGIAANDTHESOUTHSANDWICHISLANDS'),
(199,'SPAIN'),
(200,'SRILANKA'),
(201,'SUDAN'),
(202,'SURINAME'),
(203,'SVALBARDANDJANMAYEN'),
(204,'SWAZILAND'),
(205,'SWEDEN'),
(206,'SWITZERLAND'),
(207,'SYRIANARABREPUBLIC'),
(208,'TAIWAN'),
(209,'TAJIKISTAN'),
(210,'TANZANIA'),
(211,'THAILAND'),
(212,'TIMOR-LESTE'),
(213,'TOGO'),
(214,'TOKELAU'),
(215,'TONGA'),
(216,'TRINIDADANDTOBAGO'),
(217,'TUNISIA'),
(218,'TURKEY'),
(219,'TURKMENISTAN'),
(220,'TURKSANDCAICOSISLANDS'),
(221,'TUVALU'),
(222,'UGANDA'),
(223,'UKRAINE'),
(224,'UNITEDARABEMIRATES'),
(225,'UNITEDKINGDOM'),
(226,'UNITEDSTATES'),
(227,'UNITEDSTATESMINOROUTLYINGISLANDS'),
(228,'URUGUAY'),
(229,'UZBEKISTAN'),
(230,'VANUATU'),
(231,'VENEZUELA'),
(232,'VIETNAM'),
(233,'VIRGINISLANDS'),
(234,'VIRGINISLANDS'),
(235,'WALLISANDFUTUNA'),
(236,'WESTERNSAHARA'),
(237,'YEMEN'),
(238,'ZAMBIA'),
(239,'ZIMBABWE');
INSERT INTO HRM.StateProvince(StateProvinceId,StateProvinceName,CountryId)
VALUES
(1,'Punjab',162),
(2,'Sindh',162),
(3,'Khyber Pakhtunkhwa (KPK)',162),
(4,'Balochistan',162),
(5,'Islamabad Capital Territory',162),
(6,'Azad Jammu and Kashmir (AJK)',162),
(7,'Gilgit-Baltistan',162);
INSERT INTO HRM.Cities (CityId,CityName,StateProvinceId)
Values
(1,'Lahore  ',1),
(2,'Faisalabad  ',1),
(3,'Rawalpindi  ',1),
(4,'Gujranwala  ',1),
(5,'Multan  ',1),
(6,'Sialkot  ',1),
(7,'Bahawalpur  ',1),
(8,'Sargodha  ',1),
(9,'Rahim Yar Khan  ',1),
(10,'Sahiwal  ',1),
(11,'Sheikhupura  ',1),
(12,'Gujrat  ',1),
(13,'Jhelum  ',1),
(14,'Kasur  ',1),
(15,'Okara  ',1),
(16,'Vehari  ',1),
(17,'Muzaffargarh  ',1),
(18,'Mianwali  ',1),
(19,'Bahawalnagar  ',1),
(20,'Dera Ghazi Khan  ',1),
(21,'Toba Tek Singh  ',1),
(22,'Jhang  ',1),
(23,'Chiniot  ',1),
(24,'Khushab  ',1),
(25,'Lodhran  ',1),
(26,'Pakpattan  ',1),
(27,'Narowal  ',1),
(28,'Attock  ',1),
(29,'Rajanpur  ',1),
(30,'Bhakkar  ',1),
(31,'Layyah  ',1),
(32,'Hafizabad  ',1),
(33,'Khanewal  ',1),
(34,'Mandi Bahauddin  ',1),
(35,'Nankana Sahib  ',1),
(36,'Tando Allahyar  ',1);

INSERT INTO System.Roles (Id, Name, IsActive)
VALUES

(2, 'Admin', 1),
(3, 'PowerUser', 1),
(4, 'DataEntry', 1),
(5, 'Product', 1),
(6, 'Purchase', 1),
(7, 'Accounts', 1),
(8, 'POSUser', 1),
(9, 'Sales', 1),
(10, 'CustomerSupport', 1),
(11, 'Delivery', 1),
(12, 'Marketing', 1),
(13, 'User', 1),
(14, 'Customer', 1);
";
            return await _db.ExecuteQueryModify(query);
        }

        public async Task<byte[]> MasterDataExcel()
        {
            var subcats = await _attrib.GetDCS();
            var color = await _attrib.GetColors();
            var size = await _attrib.GetSizes();
            var materials = await _attrib.GetMaterials();
            var uom = await _attrib.GETUOMSUBUOM();
            var brands = await _attrib.GetBrands();
            var brandExcel = brands.Select(x => new
            {
                BrandId = x.BrandId,
                BrandName = x.BrandName
            }).ToList();
            var sizes = size.Select(x => new SizeExcel
            {
                SizeId = x.SizeId,
                SizeName = x.SizeName
            }).ToList();
            var materialsExcel = materials.Select(x => new MaterialExcel
            {
                MaterialId = x.MaterialId,
                MaterialName = x.MaterialName
            }).ToList();
            var colorExcel = color.Select(x => new ColorExcel
            {
                ColorId = x.ColorId,
                ColorName = x.ColorName
            }).ToList();
            var sheets = new Dictionary<string, IEnumerable>
            {
                ["SubCategories"] = subcats,
                ["Colors"] = colorExcel,
                ["Sizes"] = sizes,
                ["Materials"] = materialsExcel,
                ["UOMs"] = uom,
                ["Brands"] = brandExcel
            };

            var excelHandler = new ExcelHandler("INSPIRE"); // or your name/org
            byte[] bytes = await excelHandler.BuildWorkbook(sheets);
            return bytes;
        }


        public async Task<List<CustomerVM>> Customers()
        {
            return await _onedb.Customers.Select(x => new CustomerVM
            {
                CustomerName = x.Person.FirstName + "" + x.Person.LastName,
                Mobile = x.Person.MobileNumber,
                Email = x.Person.Email,
                CustomerId = x.CustomerId,
                CustomerCode = x.CustomerCode

            }).ToListAsync();
        }

        public async Task<CustomerVM> Customers(Guid CustomerId)
        {
            var result = await _onedb.Customers.Where(x => x.CustomerId == CustomerId).Select(x => new CustomerVM
            {
                CustomerName = x.Person.FirstName + "" + x.Person.LastName,
                Mobile = x.Person.MobileNumber,
                Email = x.Person.Email,
                CustomerId = x.CustomerId,
                CustomerCode = x.CustomerCode

            }).FirstOrDefaultAsync();
            return result;
        }
        public async Task<SystemPreferencesVM> GetSystemPreferences()
        {
            var branchId = _sessionService.SessionUser?.Person?.Branch?.BranchId ?? Guid.Empty;
            var result = await _onedb.SystemPreferences

                .Select(x => new SystemPreferencesVM
                {
                    // General Settings
                    CompanyName = x.CompanyName,
                    IsRestaurantApplication = x.IsRestaurantApplication,
                    CompanyLogoUrl = x.CompanyLogoUrl,
                    DefaultLanguage = x.DefaultLanguage,
                    TimeZone = x.TimeZone,
                    DateFormat = x.DateFormat,
                    CurrencyCode = x.CurrencyCode,
                    CurrencySymbol = x.CurrencySymbol,
                    DecimalPlaces = x.DecimalPlaces,
                    IsAffilatedInvoice = x.IsAffilatedInvoice,

                    // Tax & Financial
                    EnableTax = x.EnableTax,
                    DefaultTaxRate = x.DefaultTaxRate,
                    TaxRegistrationNumber = x.TaxRegistrationNumber,
                    PricesIncludeTax = x.PricesIncludeTax,

                    // Inventory & Sales
                    EnableInventoryTracking = x.EnableInventoryTracking,
                    DefaultWarehouse = x.DefaultWarehouse,
                    LowStockThreshold = x.LowStockThreshold,
                    AllowNegativeStock = x.AllowNegativeStock,

                    // Invoice & Document Settings
                    InvoicePrefix = x.InvoicePrefix,
                    InvoiceStartNumber = x.InvoiceStartNumber,
                    QuotationPrefix = x.QuotationPrefix,
                    ReceiptPrefix = x.ReceiptPrefix,
                    ShowLogoOnInvoices = x.ShowLogoOnInvoices,
                    ShowTaxBreakdown = x.ShowTaxBreakdown,

                    // User & Security
                    EnableTwoFactorAuth = x.EnableTwoFactorAuth,
                    SessionTimeoutMinutes = x.SessionTimeoutMinutes,
                    AllowMultipleLogins = x.AllowMultipleLogins,

                    // Email & Communication
                    SmtpServer = x.SmtpServer,
                    SmtpPort = x.SmtpPort,
                    SmtpUserName = x.SmtpUserName,
                    SmtpPassword = x.SmtpPassword,
                    EnableSsl = x.EnableSsl,
                    DefaultFromEmail = x.DefaultFromEmail,

                    // Other Options
                    EnableAutoBackup = x.EnableAutoBackup,
                    AutoBackupIntervalDays = x.AutoBackupIntervalDays,
                    BackupLocation = x.BackupLocation
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new SystemPreferencesVM();
            }
            else
            {
                return result;
            }
        }
        public async Task<SystemPreferencesVM> GetSystemPreferences(Guid branchid)
        {
            var result = await _onedb.SystemPreferences.Where(x => x.BranchId == branchid)

                .Select(x => new SystemPreferencesVM
                {
                    // General Settings
                    CompanyName = x.CompanyName,
                    IsRestaurantApplication = x.IsRestaurantApplication,
                    CompanyLogoUrl = x.CompanyLogoUrl,
                    DefaultLanguage = x.DefaultLanguage,
                    TimeZone = x.TimeZone,
                    DateFormat = x.DateFormat,
                    CurrencyCode = x.CurrencyCode,
                    CurrencySymbol = x.CurrencySymbol,
                    DecimalPlaces = x.DecimalPlaces,
                    IsAffilatedInvoice = x.IsAffilatedInvoice,

                    // Tax & Financial
                    EnableTax = x.EnableTax,
                    DefaultTaxRate = x.DefaultTaxRate,
                    TaxRegistrationNumber = x.TaxRegistrationNumber,
                    PricesIncludeTax = x.PricesIncludeTax,

                    // Inventory & Sales
                    EnableInventoryTracking = x.EnableInventoryTracking,
                    DefaultWarehouse = x.DefaultWarehouse,
                    LowStockThreshold = x.LowStockThreshold,
                    AllowNegativeStock = x.AllowNegativeStock,

                    // Invoice & Document Settings
                    InvoicePrefix = x.InvoicePrefix,
                    InvoiceStartNumber = x.InvoiceStartNumber,
                    QuotationPrefix = x.QuotationPrefix,
                    ReceiptPrefix = x.ReceiptPrefix,
                    ShowLogoOnInvoices = x.ShowLogoOnInvoices,
                    ShowTaxBreakdown = x.ShowTaxBreakdown,

                    // User & Security
                    EnableTwoFactorAuth = x.EnableTwoFactorAuth,
                    SessionTimeoutMinutes = x.SessionTimeoutMinutes,
                    AllowMultipleLogins = x.AllowMultipleLogins,

                    // Email & Communication
                    SmtpServer = x.SmtpServer,
                    SmtpPort = x.SmtpPort,
                    SmtpUserName = x.SmtpUserName,
                    SmtpPassword = x.SmtpPassword,
                    EnableSsl = x.EnableSsl,
                    DefaultFromEmail = x.DefaultFromEmail,

                    // Other Options
                    EnableAutoBackup = x.EnableAutoBackup,
                    AutoBackupIntervalDays = x.AutoBackupIntervalDays,
                    BackupLocation = x.BackupLocation
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return new SystemPreferencesVM();
            }
            else
            {
                return result;
            }
        }
        public async Task<AccountingPreferencesVM> GetAccountPrefrences(Guid branchid)
        {
            var result = await _onedb.AccountingPreferences
      .Where(x => x.BranchId == branchid)
      .Select(x => new AccountingPreferencesVM
      {
          FiscalYearStartMonth = x.FiscalYearStartMonth,
          FiscalYearEndMonth = x.FiscalYearEndMonth,
          FiscalYearStartDate = x.FiscalYearStartDate,
          FiscalYearEndDate = x.FiscalYearEndDate,

          EnableMultiCurrency = x.EnableMultiCurrency,
          BaseCurrencyCode = x.BaseCurrencyCode,
          DefaultExchangeRateSource = x.DefaultExchangeRateSource,

          EnableAutomaticYearClosing = x.EnableAutomaticYearClosing,
          LockTransactionsAfterPeriodClose = x.LockTransactionsAfterPeriodClose,

          DefaultSalesAccount = x.DefaultSalesAccount,
          DefaultPurchaseAccount = x.DefaultPurchaseAccount,
          DefaultTaxAccount = x.DefaultTaxAccount,

          AllowBackDatedTransactions = x.AllowBackDatedTransactions
      })
      .FirstOrDefaultAsync();


            if (result == null)
            {
                return new AccountingPreferencesVM();
            }
            else
            {
                return result;
            }
        }
        public async Task<List<BranchVM>> GetBranches()
        {
            return await _onedb.Branches.Where(x => x.Organization.IsActive == true).Select(x => new BranchVM
            {
                BranchId = x.BranchId,
                BranchName = x.BranchName,
                OrganizationName = x.Organization.OrganizationName,
                OrganizationId = x.Organization.OrganizationId,
                BranchCode = x.BranchCode,
                IsMasterBranch = x.IsMasterBranch

            }).ToListAsync();
        }
        public async Task<int> SavePrefrences(SystemPreferencesVM model)
        {
            // Try to find existing preferences for this BranchId
            var existing = await _onedb.SystemPreferences
                .FirstOrDefaultAsync(x => x.BranchId == model.BranchId);

            if (existing != null)
            {
                // Update existing record
                existing.CompanyName = model.CompanyName;
                existing.IsRestaurantApplication = model.IsRestaurantApplication;
                existing.CompanyLogoUrl = model.CompanyLogoUrl;
                existing.DefaultLanguage = model.DefaultLanguage;
                existing.TimeZone = model.TimeZone;
                existing.DateFormat = model.DateFormat;
                existing.CurrencyCode = model.CurrencyCode;
                existing.CurrencySymbol = model.CurrencySymbol;
                existing.DecimalPlaces = model.DecimalPlaces;
                existing.IsAffilatedInvoice = model.IsAffilatedInvoice;
                existing.IsEcommOnly = model.IsEcommOnly;
                existing.EcommTaxRate = model.EcommTaxRate;

                // Tax & Financial
                existing.EnableTax = model.EnableTax;
                existing.DefaultTaxRate = model.DefaultTaxRate;
                existing.TaxRegistrationNumber = model.TaxRegistrationNumber;
                existing.PricesIncludeTax = model.PricesIncludeTax;

                // Inventory & Sales
                existing.EnableInventoryTracking = model.EnableInventoryTracking;
                existing.DefaultWarehouse = model.DefaultWarehouse;
                existing.LowStockThreshold = model.LowStockThreshold;
                existing.AllowNegativeStock = model.AllowNegativeStock;

                // Invoice & Document Settings
                existing.InvoicePrefix = model.InvoicePrefix;
                existing.InvoiceStartNumber = model.InvoiceStartNumber;
                existing.QuotationPrefix = model.QuotationPrefix;
                existing.ReceiptPrefix = model.ReceiptPrefix;
                existing.ShowLogoOnInvoices = model.ShowLogoOnInvoices;
                existing.ShowTaxBreakdown = model.ShowTaxBreakdown;

                // User & Security
                existing.EnableTwoFactorAuth = model.EnableTwoFactorAuth;
                existing.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
                existing.AllowMultipleLogins = model.AllowMultipleLogins;

                // Email & Communication
                existing.SmtpServer = model.SmtpServer;
                existing.SmtpPort = model.SmtpPort;
                existing.SmtpUserName = model.SmtpUserName;
                existing.SmtpPassword = model.SmtpPassword;
                existing.EnableSsl = model.EnableSsl;
                existing.DefaultFromEmail = model.DefaultFromEmail;

                // Other Options
                existing.EnableAutoBackup = model.EnableAutoBackup;
                existing.AutoBackupIntervalDays = model.AutoBackupIntervalDays;
                existing.BackupLocation = model.BackupLocation;

                _onedb.SystemPreferences.Update(existing);
            }
            else
            {
                // Insert new
                var entity = new SystemPreference
                {
                    SystemPreferenceId = Guid.NewGuid(),
                    BranchId = model.BranchId,
                    CompanyName = model.CompanyName,
                    IsRestaurantApplication = model.IsRestaurantApplication,
                    CompanyLogoUrl = model.CompanyLogoUrl,
                    DefaultLanguage = model.DefaultLanguage,
                    TimeZone = model.TimeZone,
                    DateFormat = model.DateFormat,
                    CurrencyCode = model.CurrencyCode,
                    CurrencySymbol = model.CurrencySymbol,
                    DecimalPlaces = model.DecimalPlaces,
                    IsAffilatedInvoice = model.IsAffilatedInvoice,
                    IsEcommOnly = model.IsEcommOnly,
                    EcommTaxRate = model.EcommTaxRate,
                    // Tax & Financial
                    EnableTax = model.EnableTax,
                    DefaultTaxRate = model.DefaultTaxRate,
                    TaxRegistrationNumber = model.TaxRegistrationNumber,
                    PricesIncludeTax = model.PricesIncludeTax,

                    // Inventory & Sales
                    EnableInventoryTracking = model.EnableInventoryTracking,
                    DefaultWarehouse = model.DefaultWarehouse,
                    LowStockThreshold = model.LowStockThreshold,
                    AllowNegativeStock = model.AllowNegativeStock,

                    // Invoice & Document Settings
                    InvoicePrefix = model.InvoicePrefix,
                    InvoiceStartNumber = model.InvoiceStartNumber,
                    QuotationPrefix = model.QuotationPrefix,
                    ReceiptPrefix = model.ReceiptPrefix,
                    ShowLogoOnInvoices = model.ShowLogoOnInvoices,
                    ShowTaxBreakdown = model.ShowTaxBreakdown,

                    // User & Security
                    EnableTwoFactorAuth = model.EnableTwoFactorAuth,
                    SessionTimeoutMinutes = model.SessionTimeoutMinutes,
                    AllowMultipleLogins = model.AllowMultipleLogins,

                    // Email & Communication
                    SmtpServer = model.SmtpServer,
                    SmtpPort = model.SmtpPort,
                    SmtpUserName = model.SmtpUserName,
                    SmtpPassword = model.SmtpPassword,
                    EnableSsl = model.EnableSsl,
                    DefaultFromEmail = model.DefaultFromEmail,

                    // Other Options
                    EnableAutoBackup = model.EnableAutoBackup,
                    AutoBackupIntervalDays = model.AutoBackupIntervalDays,
                    BackupLocation = model.BackupLocation
                };

                await _onedb.SystemPreferences.AddAsync(entity);
            }

            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> SaveAccountingPreferences(AccountingPreferencesVM model)
        {
            // Try to find existing preferences for this BranchId
            var existing = await _onedb.AccountingPreferences
                .FirstOrDefaultAsync(x => x.BranchId == model.BranchId);

            if (existing != null)
            {
                // Update existing record
                existing.FiscalYearStartMonth = model.FiscalYearStartMonth;
                existing.FiscalYearEndMonth = model.FiscalYearEndMonth;
                existing.FiscalYearStartDate = model.FiscalYearStartDate;
                existing.FiscalYearEndDate = model.FiscalYearEndDate;

                existing.EnableMultiCurrency = model.EnableMultiCurrency;
                existing.BaseCurrencyCode = model.BaseCurrencyCode;
                existing.DefaultExchangeRateSource = model.DefaultExchangeRateSource;

                existing.EnableAutomaticYearClosing = model.EnableAutomaticYearClosing;
                existing.LockTransactionsAfterPeriodClose = model.LockTransactionsAfterPeriodClose;

                existing.DefaultSalesAccount = model.DefaultSalesAccount;
                existing.DefaultPurchaseAccount = model.DefaultPurchaseAccount;
                existing.DefaultTaxAccount = model.DefaultTaxAccount;

                existing.AllowBackDatedTransactions = model.AllowBackDatedTransactions;

                _onedb.AccountingPreferences.Update(existing);
            }
            else
            {
                // Insert new
                var entity = new AccountingPreference
                {
                    AccountingPreferenceId = Guid.NewGuid(),
                    BranchId = model.BranchId,

                    FiscalYearStartMonth = model.FiscalYearStartMonth,
                    FiscalYearEndMonth = model.FiscalYearEndMonth,
                    FiscalYearStartDate = model.FiscalYearStartDate,
                    FiscalYearEndDate = model.FiscalYearEndDate,

                    EnableMultiCurrency = model.EnableMultiCurrency,
                    BaseCurrencyCode = model.BaseCurrencyCode,
                    DefaultExchangeRateSource = model.DefaultExchangeRateSource,

                    EnableAutomaticYearClosing = model.EnableAutomaticYearClosing,
                    LockTransactionsAfterPeriodClose = model.LockTransactionsAfterPeriodClose,

                    DefaultSalesAccount = model.DefaultSalesAccount,
                    DefaultPurchaseAccount = model.DefaultPurchaseAccount,
                    DefaultTaxAccount = model.DefaultTaxAccount,

                    AllowBackDatedTransactions = model.AllowBackDatedTransactions
                };

                await _onedb.AccountingPreferences.AddAsync(entity);
            }

            return await _onedb.SaveChangesAsync();
        }


        public async Task<IEnumerable<BusinessEntityTypeVM>> GetEntities()
        {
            return await _db.ExecuteQueryList<BusinessEntityTypeVM>("SELECT * FROM Business.BusinessEntityType");
        }

        public async Task<IEnumerable<BusinessCategoryVM>> GetBusinessCategories()
        {
            return await _db.ExecuteQueryList<BusinessCategoryVM>("SELECT * FROM Business.BusinessCategory");
        }

        public async Task<int> AddBusinessEntity(BusinessEntityTypeVM model)
        {
            string query = $@"

    IF EXISTS (
        SELECT 1 
        FROM Business.BusinessEntityType
        WHERE BusinessEntityTypeName = '{model.BusinessEntityTypeName}'
    )
    BEGIN
        SELECT -2 AS Result;
    END
    ELSE
    BEGIN
        INSERT INTO Business.BusinessEntityType (
            BusinessEntityTypeId, BusinessEntityTypeName, IsActive, CreatedBy, CreatedOn
        )
        VALUES (
            (SELECT ISNULL(MAX(BusinessEntityTypeId), 0) + 1 FROM Business.BusinessEntityType),
            @BusinessEntityTypeName, @IsActive, @CreatedBy, @CreatedOn
        );

        SELECT SCOPE_IDENTITY() AS Result;
    END";

            var param = new
            {
                BusinessEntityTypeName = model.BusinessEntityTypeName,
                IsActive = 1,
                CreatedBy = _sessionService.SessionUser.Id,
                CreatedOn = DateTime.Now
            };

            return await _db.ExecuteInsertQueryandParam(query, param);

        }

        public async Task<int> AddBusinessCategory(BusinessCategoryVM model)
        {
            string query = $@"

    IF EXISTS (
        SELECT 1 
        FROM Business.BusinessCategory
        WHERE BusinessCategoryName = '{model.BusinessCategoryName}'
    )
    BEGIN
        SELECT -2 AS Result;
    END
    ELSE
    BEGIN
        INSERT INTO Business.BusinessCategory (
            BusinessCategoryId, BusinessCategoryName, IsActive, CreatedBy, CreatedOn
        )
        VALUES (
            (SELECT ISNULL(MAX(BusinessCategoryId), 0) + 1 FROM Business.BusinessCategory),
            @BusinessCategoryName, @IsActive, @CreatedBy, @CreatedOn
        );

        SELECT SCOPE_IDENTITY() AS Result;
    END";

            var param = new
            {
                BusinessCategoryName = model.BusinessCategoryName,
                IsActive = 1,
                CreatedBy = _sessionService.SessionUser.Id,
                CreatedOn = DateTime.Now
            };

            return await _db.ExecuteInsertQueryandParam(query, param);



        }


    }
}
