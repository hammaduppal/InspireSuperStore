using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Collection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Areas.Collections.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("Collections")]
    [Route("collection/[action]")]
    public class CollectionController : Controller
    {
        private readonly IConfiguration _config;
        private readonly CollectionRepository _collectionRepository;
        private readonly PagesViewModel vm = new PagesViewModel();
        private readonly OneDb _oneDb;
        public CollectionController(IConfiguration config,OneDb oneDb)
        {
            _config = config;
            _oneDb = oneDb;
            _collectionRepository = new CollectionRepository(_config,_oneDb);
        }
        public async Task<IActionResult> Collections()
        {
            vm.Collections = await _collectionRepository.GetCollections();
            return View(vm);
        }
        public async Task<IActionResult> AddCollection()
        {
            return View(vm);
        }
        public async Task<IActionResult> AddNewCollection([FromForm] AddCollectionVM model, IFormFile Image)
        {
            var result = await _collectionRepository.AddNewCollection(model,Image);
            return APIResponseHelper.ResultResponse(this,result); 
            //return Json(new {statusCode="200" });
        }
    }
}
