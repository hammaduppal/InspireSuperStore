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

        public async Task<IActionResult> AllContacts(string contactType)
        {
            if (string.Equals(contactType, "all", StringComparison.OrdinalIgnoreCase))
            {
                vm.CompanyContacts = await _repo.GetContacts();
                vm.ContactTypes = await _repo.ContactTypes();
            }
            else if (Enum.TryParse<ReContactType>(contactType, true, out var parsedType))
            {
                vm.CompanyContacts = await _repo.GetContacts(parsedType.ToString());
            }
            else
            {
                // Handle invalid contactType (e.g., return BadRequest or empty list)
                return BadRequest("Invalid contact type specified.");
            }
            ViewBag.ContactType = contactType ?? string.Empty;

            return View(vm);
        }

        public async Task<IActionResult> _AddEstateContactForm(string contactType = null, int? contactId = null)
        {
            vm.Countries = await _adminPanel.Countries();
            if (contactId != null)
            {
                vm.CompanyContact = await _repo.GetContact(contactId.Value);
            }
            ViewBag.ContactType = contactType ?? string.Empty;
            vm.ContactTypes = await _repo.ContactTypes();

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> AddContactPerson([FromForm] RecompanyContactVM modal)
        {
            var result = await _repo.AddContact(modal);
            if (result)
            {
                return Json(new { statusCode = "200", Message = $"Successfully saved contact type {modal.RecontactTypeName}" });

            }
            else
            {
                return Json(new { statusCode = "200", Message = $"Unable to save contact type {modal.RecontactTypeName}" });


            }
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
        #region Menus
        [Route("/labour")]
        public IActionResult WorkerMenu()
        {
            return View(vm);
        }


        [Route("/suppliers")]
        public IActionResult SupplierMenu()
        {
            return View(vm);
        }


        [Route("/companies")]
        public IActionResult CompanyMenu()
        {
            return View(vm);
        }

        #endregion







        public async Task<IActionResult> Properties(int propertyType = 1)
        {
            vm.Properties = await _repo.GetAllProperties();
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
                if (model.PropertyMediaFiles != null)
                {
                    foreach (var item in model.PropertyMediaFiles)
                    {
                        uploadResult.Add(await _file.SaveFile(item, "REProperties", "RealEstate"));

                    }

                }
                var result = await _repo.AddProperty(model, uploadResult);
                if (result)
                {
                    return Json(new { statusCode = "200", Message = "Property saved", filesCount });
                }
                else
                {
                    return Json(new { statusCode = "300", Message = "Unable to save property" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", Message = ex.Message });
            }
        }

        public async Task<IActionResult> MediaUpload(string uploadType)
        {
            ViewBag.UploadType = uploadType;
            vm.MediaData = await _repo.LinksData(uploadType);
            return View(vm);
        }
        public async Task<IActionResult> EditMediaUpload(int mediaId)
        {
            vm.MediaDataItem = await _repo.GetLinksDataById(mediaId);
            return View(vm);
        }
        public async Task<IActionResult> AddMedia()
        {
            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedia([FromForm] PropertyMediumFormSubmit model)
        {
            var filesCount = model.PropertyMediaFiles?.Count ?? 0;
            List<APIImageContentResponse> uploadResult = new List<APIImageContentResponse>();
            if (model.PropertyMediaFiles != null)
            {
                foreach (var item in model.PropertyMediaFiles)
                {
                    uploadResult.Add(await _file.SaveFile(item, "MediaFiles", "RealEstate"));
                }
            }
            var result = await _repo.AddMedia(model, uploadResult);

            if (result)
            {
                return Json(new { statusCode = "200", Message = "Media Save", filesCount });
            }
            else
            {
                return Json(new { statusCode = "300", Message = "Unable to save Media" });
            }
        }




















    }
}
