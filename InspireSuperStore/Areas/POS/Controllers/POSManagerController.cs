using InspireSuperStore.Areas.Notification.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using MarketBal.Repository.POSManager;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace InspireSuperStore.Areas.POS.Controllers
{
    [Area("POS")]
    [Route("[controller]/[action]")]

    public class POSManagerController : Controller
    {
        PagesViewModel vm = new PagesViewModel();
        private readonly AttributeRepository _attrib;
        private readonly IConfiguration _config;
        private readonly AccountRepository _account;
        private readonly AdminPanelRepository _admin;
        private readonly OneDb _oneDb;
        private readonly AssetRepository _assets;
        private readonly HumanRespourceRepository _hrm;
        private readonly NotificationService _notificationServices;
        private readonly NotificationRepository _notificationRepository;
        private readonly ISessionService _sessionService;

        public POSManagerController(IConfiguration config, OneDb oneDb, NotificationService notificationServices, ISessionService sessionService)
        {
            _config = config;
            _oneDb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config,_oneDb,_sessionService);
            _account = new AccountRepository(_config,_sessionService);
            _admin = new AdminPanelRepository(_config, _oneDb,_sessionService);
            _assets = new AssetRepository(_config, _oneDb,_sessionService);
            _hrm = new HumanRespourceRepository(_config, _oneDb,_sessionService);
            _notificationRepository = new NotificationRepository(_oneDb);
        }
      
    
    }
}
