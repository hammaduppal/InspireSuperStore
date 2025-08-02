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

namespace MarketBal.Repository
{
    public class AdminPanelRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public AdminPanelRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }
        
        public async Task<List<LoginUserVM>> GetLoginUser()
        {
            string query = $@"select  * from Hrm.LoginUsers l where l.Id !=1";
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
            string query = $@"select * from Hrm.LoginUsers lu  where lu.Id= @Id";
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
                Createdby = AppDataUtility.SessionUser.Id,
                UserAssignedBranches = assignBranches,
                Person = new Person
                {
                    Id = maxPersonId + 1,
                    Cnic = model.PersonVM.Cnic,
                    Email = model.UserName,
                    SocialSecurity = model.PersonVM.SocialSecurity,
                    LastName = model.PersonVM.LastName,
                    FirstName = model.PersonVM.FirstName,
                    MobileNumber = model.PersonVM.MobileNumber,
                    LaneAddresses = addresses,
                    BranchId = model.PersonVM.BranchId,
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

        








        public async Task<List<OrganizationVM>> GetOrganizations()
        {
            return await _onedb.Organizations.Select(x => new OrganizationVM
            {
                OrganizationId = x.OrganizationId,
                OrganizationName = x.OrganizationName
            }).ToListAsync();
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
                var removed = await removeData();
                var currentTime = DateTime.UtcNow;
                Guid branchId = Guid.NewGuid();

                // Only proceed if tables are empty
                if (!_onedb.Organizations.Any() && !_onedb.LoginUsers.Any())
                {
                    var organization = new Organization
                    {
                        OrganizationName = model.BranchName,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime
                    };

                    await _onedb.Organizations.AddAsync(organization);
                   await _onedb.SaveChangesAsync(); // Save to generate OrganizationId (if Identity)
                    var category = _onedb.BusinessCategories.Where(x => x.BusinessCategoryId == model.BusinessCategory).FirstOrDefault();
                    var businessType = _onedb.BusinessEntityTypes.Where(x => x.BusinessEntityTypeId == model.BusinessEntity).FirstOrDefault();
                    var getOrganization = _onedb.Organizations.ToList();
                    var singleOrganization = getOrganization.FirstOrDefault();
                    var branch = new Branch
                    {
                        BranchId = branchId,
                        BranchName = model.BranchName,
                        OrganizationId = singleOrganization.OrganizationId,
                     
                    };
                    await _onedb.Branches.AddAsync(branch);

                    var loginUser = new LoginUser
                    {
                        Id = 1,
                        UserName = model.Email,
                        Passwords = EncryptionPasses.Encrypt(model.Password, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = currentTime
                    };
                    await _onedb.LoginUsers.AddAsync(loginUser);

                    var role = new Role
                    {
                        Id = 1,
                        Name = "SuperAdmin",
                        IsActive=true
                    };
                    await _onedb.Roles.AddAsync(role);

                    var assignedRole = new AssignedRole
                    {
                        Id = 1,
                        LoginId = loginUser.Id,
                        RoleId = role.Id,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime,
                        IsActive = true,
                        IsDeleted = false
                    };
                    await _onedb.AssignedRoles.AddAsync(assignedRole);

                    var assignedBranch = new UserAssignedBranch
                    {
                        LoginUserId = loginUser.Id,
                        BranchId = branchId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedOn = currentTime,
                        ModifiedOn = currentTime
                    };
                    await _onedb.UserAssignedBranches.AddAsync(assignedBranch);

                    // Save all together
                    await _onedb.SaveChangesAsync();
                    await transaction.CommitAsync();
                    await InsertSeedData();
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
            delete from hrm.Department
delete from Hrm.UserAssignedBranches
            delete from Hrm.LoginUsers
            delete from hrm.Persons
            delete from Business.Branches
            delete from Business.Organizations
select 1;
";
           return  await _db.ExecuteQueryModify(query);
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
            var size= await _attrib.GetSizes();
            var materials = await _attrib.GetMaterials();
            var uom = await _attrib.GETUOMSUBUOM();
            var sizes = size.Select(x=> new SizeExcel
            {
                SizeId=x.SizeId,SizeName=x.SizeName
            }).ToList();
            var materialsExcel = materials.Select(x => new MaterialExcel
            {
                MaterialId = x.MaterialId,
                MaterialName = x.MaterialName
            }).ToList();
            var colorExcel = color.Select(x => new ColorExcel
            {
                 ColorId=x.ColorId,
                 ColorName=x.ColorName
            }).ToList();
            var sheets = new Dictionary<string, IEnumerable>
            {
                ["SubCategories"] = subcats,
                ["Colors"] = colorExcel,
                ["Sizes"] = sizes,
                ["Materials"] = materialsExcel,
                ["UOMs"] = uom
            };

            var excelHandler = new ExcelHandler("INSPIRE"); // or your name/org
            byte[] bytes = await excelHandler.BuildWorkbook(sheets);
            return bytes;
        }
    
    
    
    
    
    
    
    
    
    }
}
