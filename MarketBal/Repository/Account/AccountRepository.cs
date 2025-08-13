using System.Security.Claims;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MarketBal.Repository.Account
{
    public class AccountRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        public AccountRepository(IConfiguration config)
        {
            _config = config;
            _db = new DBManager(_config);
        }
        public async Task<LoginUserVM> ValidateLogin(LoginUserVM model)
        {
            string encodedPass = EncryptionPasses.Encrypt(model.Passwords, PassesCore.INIT_VECTOR, PassesCore.PASS_PHRASE, PassesCore.KEY_SIZE);

            var query = $"select * from HRM.LoginUsers  u  where u.UserName='{model.UserName}' and u.Passwords='{encodedPass}' and u.IsActive = 1   ";
            var result = await _db.ExecuteQuery<LoginUserVM>(query);

            if (result != null)
            {
                query = $"select * from SYSTEM.AssignedRoles ar join SYSTEM.Roles r on ar.RoleId=r.Id where ar.LoginId={result.Id}";
                var roles = await _db.ExecuteQueryList<RolesVM>(query);
                result.Roles = roles.ToList();

                if (result.Roles != null)
                {
                    var singleRole = result.Roles.FirstOrDefault();
                    result.RoleName = singleRole.Name;
                    if (singleRole.Name == "SuperAdmin")
                    {
                        return result;

                    }
                }
                query = $"SELECT * FROM HRM.Persons WHERE Id={result.PersonId}";

                var person = await _db.ExecuteQuery<PersonVM>(query);
                result.PersonVM = person;
                query = $@"select * from Business.Branches where BranchId = '{result.PersonVM.BranchId}'";
                var branch = await _db.ExecuteQuery<BranchVM>(query);
                result.PersonVM.Branch = branch;
                query = $@"select * from Business.Organizations where OrganizationId={result.PersonVM.Branch.OrganizationId}";
                var organzation = await _db.ExecuteQuery<OrganizationVM>(query);
                result.PersonVM.Branch.Organization = organzation;
            }
            return result;
        }

        public async Task SigninAsync(LoginUserVM u, HttpContext httpcontext)
        {
            List<Claim> Claims = new List<Claim> {
                new Claim("UserId",u.Id.ToString()??""),
                //new Claim("UserName",u.PersonVM.FirstName??""),
                new Claim("UserEmail", u.UserName??""),
                new Claim("ThemeStyle", "1"),

                new Claim(ClaimTypes.NameIdentifier,u.UserName??""),
                };
            foreach (var item in u.Roles)
            {
                var res = new Claim(ClaimTypes.Role, item?.Name);
                Claims.Add(res);
            }
            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.Now,
                RedirectUri = "/",
            };
            var identity = new ClaimsIdentity(Claims, CookieAuthenticationDefaults.AuthenticationScheme + _config.GetValue<string>("SystemSettings:CookieName"));
            await httpcontext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme + _config.GetValue<string>("SystemSettings:CookieName"), new ClaimsPrincipal(identity), authProperties);
        }
        public async Task LogoutAsync(HttpContext httpContext)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme + _config.GetValue<string>("SystemSettings:CookieName"));
            httpContext.Response.Cookies.Delete(_config.GetValue<string>("SystemSettings:CookieName"));
        }

        public async Task<Guid> AddCustomer(PersonVM model)
        {
            string query = "select * from HRM.Persons where MobileNumber = @MobileNumber";
            var param = new
            {
                MobileNumber = model.MobileNumber
            };
            var existingPerson = await _db.GetSingleItemDatatWithQueryAndParam<PersonVM>(query, param);
            string insertQuery = $@"INSERT INTO HRM.Customers (CustomerId,CustomerCode,PersonId,CreatedOn,Createdby,IsActive,BranchId,IsDeleted) 
                        VALUES (@CustomerId,@CustomerCode,@PersonId,@CreatedOn,@Createdby,@IsActive,@BranchId,@IsDeleted) select 1";
            if (existingPerson != null)
            {

                var parameters = new
                {
                    CustomerId = Guid.NewGuid(), // updated to generate a new Guid

                    CustomerCode = RandomHelper.GenerateRandomAlphaNumeric(),
                    PersonId = existingPerson.Id,
                    BranchId = AppDataUtility.SessionUser.PersonVM.BranchId,
                    Createdby = AppDataUtility.SessionUser.Id,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                };
                try
                {
                     await _db.ExecuteQuery<int>(insertQuery, parameters); // revert back to ExecuteQueryModify
                    return parameters.CustomerId;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error adding customer.", ex);
                }
            }
            else
            {
                model.IsActive = true;
                model.CreatedOn = DateTime.Now;
                model.BranchId = AppDataUtility.SessionUser.PersonVM.BranchId;
                insertQuery = $@"DECLARE @NewPersonId INT;

-- Get max PersonId and add 1 (handle null if table is empty)
SELECT @NewPersonId = ISNULL(MAX(Id), 0) + 1
FROM HRM.Persons;

INSERT INTO HRM.Persons 
(Id, FirstName, LastName, MobileNumber, Email, IsActive, CreatedOn, BranchId)
OUTPUT INSERTED.Id
VALUES 
(@NewPersonId,@FirstName, @LastName, @MobileNumber, @Email, @IsActive, @CreatedOn, @BranchId) select 1



";
                var personId = await _db.ExecuteQuery<int>
                    (insertQuery, model);
                var parameters = new
                {
                    CustomerId = Guid.NewGuid(), // updated to generate a new Guid
                    CustomerCode = RandomHelper.GenerateRandomAlphaNumeric(),
                    PersonId = personId,
                    BranchId = AppDataUtility.SessionUser.PersonVM.BranchId,
                    Createdby = AppDataUtility.SessionUser.Id,
                    CreatedOn = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                };
                insertQuery = $@"INSERT INTO HRM.Customers (CustomerId,CustomerCode,PersonId,CreatedOn,Createdby,IsActive,BranchId,IsDeleted) 
                        VALUES (@CustomerId,@CustomerCode,@PersonId,@CreatedOn,@Createdby,@IsActive,@BranchId,@IsDeleted) select 1";
                try
                {
                     await _db.ExecuteQuery<int>(insertQuery, parameters); // revert back to ExecuteQueryModify
                    return parameters.CustomerId;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error adding customer.", ex);
                }
                return Guid.Empty;
            }
        }
    

    
    
    }
}
