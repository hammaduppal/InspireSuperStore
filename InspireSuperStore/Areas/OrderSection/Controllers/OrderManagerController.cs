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
using NuGet.ContentModel;

namespace InspireSuperStore.Areas.OrderSection.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("OrderSection")]
    [Route("OrderSection/[action]")]
    public class OrderManagerController : Controller
    {
        PagesViewModel vm = new PagesViewModel();
        private readonly AttributeRepository _attrib;
        private readonly IConfiguration _config;
        private readonly AccountRepository _account;
        private readonly AdminPanelRepository _admin;
        private readonly OneDb _oneDb;
        private readonly AssetRepository _assets;
        private readonly HumanRespourceRepository _hrm;
        private readonly POSRepository _posRepo;
        private readonly NotificationService _notificationServices;
        private readonly NotificationRepository _notificationRepository;

        public OrderManagerController(IConfiguration config, OneDb oneDb, NotificationService notificationServices)
        {
            _config = config;
            _oneDb = oneDb;
            _attrib = new AttributeRepository(_config);
            _account = new AccountRepository(_config);
            _admin = new AdminPanelRepository(_config, _oneDb);
            _assets = new AssetRepository(_config, _oneDb);
            _hrm = new HumanRespourceRepository(_config, _oneDb);
            _posRepo = new POSRepository(_config, _oneDb);
            _notificationServices = notificationServices;
            _notificationRepository = new NotificationRepository(_oneDb);
        }
        public async Task<IActionResult> Orders()
        {
            return View();
        }

        public async Task<IActionResult> Order(Guid OrderId)
        {
            return View();
        }


        public async Task<IActionResult> CreateOrder()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.Countries = await _admin.Countries();
            vm.Customers = await _admin.Customers();
            vm.ServingTables = await _assets.ServingTables();
            vm.Employees = await _hrm.GetSaleStaff();
            vm.PaymentMethods = await _assets.PaymentMethods();
            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> SaveMyAssOrder(OrderMasterVM formData)
        {
            //  var result = await _posRepo.SaveOrder(formData);
            var result = 1;
            var notification = new NotificationsDTO
            {
                CreatedAt = DateTime.Now,
                GroupName = "Sales",
                IsRead = false,
                Message = "new Order",
            };

            var orderparam = new OrderParam
            {
                OrderId = Guid.NewGuid()//result.NewItemId
            };
            var Notification = new NotificationsDTO
            {
                CreatedAt = DateTime.Now,
                GroupName = "Sales",
                IsRead = false,
                Params = JsonConvert.SerializeObject(orderparam),
                UserId = AppDataUtility.SessionUser.Id,
                NotificationTypeId = 1
            };
            await _notificationServices.SendToRoleGroup("Sales", JsonConvert.SerializeObject(Notification));
            await _notificationRepository.SaveNotification(Notification);
            // var invoice = await _posRepo.GenerateInvoiceHTML(model);
            if (result > 0)
            {
                return Json(new { statusCode = "200", Success = true, Message = "Order saved successfully!" });

            }
            else
            {
                return Json(new { statusCode = "300", Success = true, Message = "Unable to Save Order" });
            }






        }

    }
}
