using InspireSuperStore.Areas.Notification.Data;
using InspireSuperStore.Models;
using MainModels;
using MainModels.DTOModels;
using MainModels.Util;
using MarketBal.Repository.DashBoard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
		private readonly NotificationService _notificationServices;
        private readonly IHubContext<NotificationHub> _hubContext;
        public HomeController(ILogger<HomeController> logger,IConfiguration config, NotificationService notificationService, IHubContext<NotificationHub> hubContext)
		{
			_config = config;
			_logger = logger;
			_apiMethod = new ApiMethods();
			_dashboard = new DashBoardRepository(_config);
			_notificationServices = notificationService;
            _hubContext = hubContext;
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

		public async Task<IActionResult> Index()
		{
			//var reu = await _notificationServices.NotifyOnlineUsersUpdated();
			//await _hubContext.Clients.All.SendAsync("ReceiveNotification", "Welcome! You are logged in.");

			
			//string message = "this is my message to send to admin@inspirenation.us";
   //         await _hubContext.Clients.User("admin@inspirenation.us").SendAsync("ReceiveNotification", $"Private: {message}");
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
