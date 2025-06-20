using InspireSuperStore.Models;
using MainModels;
using MainModels.DTOModels;
using MarketBal.Repository.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InspireSuperStore.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		
		private readonly ILogger<HomeController> _logger;
		private readonly IConfiguration _config;
		private readonly ApiMethods _apiMethod;
		private readonly DashBoardRepository _dashboard;
		public HomeController(ILogger<HomeController> logger,IConfiguration config)
		{
			_config = config;
			_logger = logger;
			_apiMethod = new ApiMethods();
			_dashboard = new DashBoardRepository(_config);
			var systemSettings = _config.GetSection("SystemSettings").Get<SystemSettings>();
			PagesViewModel.SystemSettings = systemSettings;
			if (PagesViewModel.ThemeSettings==null)
			{
				ThemeSettings settings = new ThemeSettings();
				settings.ToggleSideBar = "";
				settings.TemplateColor = "";
				PagesViewModel.ThemeSettings=settings;
			}
			if (PagesViewModel.DashBoardSetting==null)
			{
				PagesViewModel.DashBoardSetting  =  _dashboard.Settings().GetAwaiter().GetResult();
			}
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult ToggleSideMenu()
		{
			if (PagesViewModel.ThemeSettings.ToggleSideBar == "sidebar-xs")
			{
				PagesViewModel.ThemeSettings.ToggleSideBar = "";

            }
			else
			{
				PagesViewModel.ThemeSettings.ToggleSideBar = "sidebar-xs";

            }
			

            return Json(new { Message="Done"});
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
