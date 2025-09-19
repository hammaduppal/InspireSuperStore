using InspireSuperStore.Areas.Notification.Data;
using InspireSuperStore.Models;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
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
		private readonly AdminPanelRepository _admin;
		private readonly NotificationService _notificationServices;
        private readonly IHubContext<NotificationHub> _hubContext;
		private readonly OneDb _onedb;
        public HomeController(ILogger<HomeController> logger, OneDb onedb,IConfiguration config, NotificationService notificationService, IHubContext<NotificationHub> hubContext)
		{
			_config = config;
			_logger = logger;
			_apiMethod = new ApiMethods();
			_onedb = onedb;
            _dashboard = new DashBoardRepository(_config,_onedb);
			_admin = new AdminPanelRepository(_config, _onedb);
			_notificationServices = notificationService;
            _hubContext = hubContext;
            var systemSettings = _config.GetSection("SystemSettings").Get<SystemSettings>();
			var systemPref =  _admin.GetSystemPreferences();
			AppDataUtility.SystemPreferences = systemPref.GetAwaiter().GetResult();
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
                PagesViewModel.Settings = _dashboard.GetSettings().GetAwaiter().GetResult();

                PagesViewModel.DashBoardSetting  =  _dashboard.Settings().GetAwaiter().GetResult();
			}
		}

		public async Task<IActionResult> Index()
		{
			await _notificationServices.NotifyOnlineUsersUpdated();
			await _hubContext.Clients.All.SendAsync("ReceiveNotification", "Welcome! You are logged in.");
			

			string message = "this is my message to send to admin@inspirenation.us";
			await _hubContext.Clients.User("admin@inspirenation.us").SendAsync("ReceiveNotification", $"Private: {message}");

            await _notificationServices.SendToRoleGroup("Sales", "New Order Recieved");

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
