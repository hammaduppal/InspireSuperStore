using MainModels;
using MainModels.DTOModels;
using Microsoft.AspNetCore.Mvc;

namespace InspireSuperStore.Controllers
{
    public class FileManagerController : Controller
    {
        private readonly ApiMethods _api;
        public FileManagerController()
        {
            _api = new ApiMethods();
        }
        public IActionResult UploadImage(UploadImage model)
        {
            if (model.File!=null)
            {
                string companyName = PagesViewModel.SystemSettings.BranchId;
              
            }
            return Json(new { });
        }
    }
}
