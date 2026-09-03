using InspireSuperStore.Areas.REstate.Models;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using MarketBal.Repository.RealEstateRP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Intrinsics.Arm;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Net.WebRequestMethods;

namespace InspireSuperStore.Areas.REstate.Controllers
{
    [Authorize(Roles = UserRolesConstants.SuperAdmin + "," + UserRolesConstants.Admin)]
    [Area("REstate")]
    [Route("[controller]/[action]")]
    public class EstateContactsController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly RealEstateRepository _repo;
        private readonly OneDb _oneDb;
        private readonly AdminPanelRepository _adminPanel;
        private readonly HumanRespourceRepository _hrmRepo;
        private readonly FileRepository _file;

        private readonly PagesViewModel vm = new PagesViewModel();
        public EstateContactsController(OneDb oneDb, ISessionService sessionService, IConfiguration config)
        {
            _oneDb = oneDb;
            _sessionService = sessionService;
            _config = config;
            _repo = new RealEstateRepository(_oneDb);
            _adminPanel = new AdminPanelRepository(_config, _oneDb, _sessionService);
            _hrmRepo = new HumanRespourceRepository(_config, _oneDb, _sessionService);
            _file = new FileRepository(_sessionService);

        }

        public async Task<IActionResult> AllContacts()
        {
            vm.CompanyContacts = _repo.GetContacts();
            return View(vm);
        }


        [Route("/labour")]
        public IActionResult WorkerMenu()
        {
            return View(vm);
        }

        #region workerviewmenu
        public IActionResult Contractors()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Contractor.ToString());
            return View(vm);
        }
        public IActionResult Masons()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Mason.ToString());
            return View(vm);
        }

        public IActionResult Labour()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Laborer.ToString());
            return View(vm);
        }
        public IActionResult CivilEngineer()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.CivilEngineer.ToString());
            return View(vm);
        }

        public IActionResult Electrician()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Electrician.ToString());
            return View(vm);
        }

        public IActionResult Plumber()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Plumber.ToString());
            return View(vm);
        }

        public IActionResult Carpenter()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Carpenter.ToString());
            return View(vm);
        }

        #endregion

        [Route("/suppliers")]
        public IActionResult SupplierMenu()
        {
            return View(vm);
        }
       
        #region SuppliersMenu
        public IActionResult SandSupplier()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.SandSupplier.ToString());
            return View(vm);
        }
        public IActionResult CrushSupplier()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.CrushSupplier.ToString());
            return View(vm);
        }
        public IActionResult CementSupplier()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.CementSupplier.ToString());
            return View(vm);
        }
        public IActionResult InteriorSupplier()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.InteriorSupplier.ToString());
            return View(vm);
        }
        #endregion

        [Route("/companies")]
        public IActionResult CompanyMenu()
        {
            return View(vm);
        }


        
       
      
        #region CompaniesMenu
        public IActionResult LegalAdvisors()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.LegalAdvisor.ToString());
            return View(vm);
        }

        public IActionResult RealEstateAgent()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.RealEstateAgent.ToString());
            return View(vm);
        }

        #endregion


        public IActionResult PropertyOwner()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.PropertyOwner.ToString());
            return View(vm);
        }

        public IActionResult Tenant()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Tenant.ToString());
            return View(vm);
        }

        public IActionResult Investor()
        {
            vm.CompanyContacts = _repo.GetContacts(ReContactType.Investor.ToString());
            return View(vm);
        }

        
        [Route("/gallery")]
        public IActionResult Gallery()
        {
            
            return View(vm);
        }
        public async Task<IActionResult> AddProperty()
        {
            vm.PropertyTypes = await _repo.GetPropertyTypes();
            vm.PropertyPurposes = await _repo.GetPropertyPurposeTypes();
            vm.Cities = await _hrmRepo.GetCitybyCountry("Pakistan");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty([FromForm] AddPropertyModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Where(ms => ms.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { status = "error", message = "Validation failed", errors });
                }

                var filesCount = model.PropertyMediaFiles?.Count ?? 0;
                List<APIImageContentResponse> uploadResult = new List<APIImageContentResponse>();
                if (model.PropertyMediaFiles!=null)
                {
                    foreach (var item in model.PropertyMediaFiles)
                    {
                         uploadResult.Add(await _file.SaveFile(item, "Products", "Products"));

                    }

                }
                var result = await _repo.AddProperty(model, uploadResult);
                if (result)
                {
                    return Json(new { status = "success", message = "Property saved", filesCount });
                }
                else
                {
                    return Json(new { status = "error", message = "Unable to save property" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }



















        [HttpPost]
        public async Task<IActionResult> AddContactPerson([FromForm] RecompanyContactVM modal)
        {
            var result = await _repo.AddContact(modal);
            if (result)
            {
                return Json(new { statusCode="200",Message=$"Successfully saved contact type {modal.RecontactTypeName}" });

            }
            else
            {
                return Json(new { statusCode = "200", Message = $"Unable to save contact type {modal.RecontactTypeName}" });


            }
        }
        public async Task<IActionResult> _AddEstateContactForm(string contactType = null, int? contactId = null)
        {
            vm.Countries = await _adminPanel.Countries();
            if (contactId != null)
            {
                vm.CompanyContact = await _repo.GetContact(contactId.Value);
            }
            ViewBag.ContactType = contactType ?? string.Empty;
            return View(vm);
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            var list = _oneDb.Recompanies.Select(x => new { x.RecompanyId, x.RecontactName }).ToList();
            return Json(list);
        }

        [HttpGet]
        public IActionResult GetContactTypes()
        {
            var list = _oneDb.RecontactTypes.Select(x => new { x.RecontactTypeId, x.RecontactTypeName }).ToList();
            return Json(list);
        }

        [HttpPost]
        public IActionResult SaveContact([FromBody] MainModels.DTOModels.RecompanyContactVM model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            try
            {
                var entity = new MainModels.Models.RecompanyContact
                {
                    FullName = model.FullName,
                    Cnic = model.Cnic,
                    RecontactTypeId = model.RecontactTypeId,
                    RecompanyId = model.RecompanyId,
                    CreatedOn = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                    Email = model.Email,
                    MobileHome = model.MobileHome,
                    MobileWork = model.MobileWork,
                    LandLine = model.LandLine
                };

                _oneDb.RecompanyContacts.Add(entity);
                _oneDb.SaveChanges();

                return Json(new { success = true, id = entity.RecompanyContactId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

   



    }
}
