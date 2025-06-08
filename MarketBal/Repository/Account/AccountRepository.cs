using System.Security.Claims;
using MainModels;
using MainModels.DTOModels;
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

            var query = $"select * from HRM.LoginUsers  u left JOIN HRM.UserAssignedBranches ab on u.Id=ab.LoginUserId   where u.UserName='{model.UserName}' and u.Passwords='{encodedPass}' and ab.IsActive=1";
            var result = await _db.ExecuteQuery<LoginUserVM>(query);

            if (result != null)
            {
                query = $"select * from SYSTEM.AssignedRoles ar join SYSTEM.Roles r on ar.RoleId=r.Id where ar.LoginId={result.Id}";
                var roles = await _db.ExecuteQueryList<RolesVM>(query);
                result.Roles = roles.ToList();

                query = $"SELECT * FROM HRM.Persons WHERE Id={result.PersonId}";

                var person = await _db.ExecuteQuery<PersonVM>(query);
                result.PersonVM = person;
                query = $@"select * from Business.Branches where BranchId = '{person.BranchId}'";
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
                new Claim("UserName",u.PersonVM.FirstName??""),
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

    }
}
