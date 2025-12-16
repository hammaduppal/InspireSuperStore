using InspireSuperStore.Areas.Notification.Data;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository;
using MarketBal.Repository.Account;
using MarketBal.Repository.HRM;
using MarketBal.Repository.InvoiceRP;
using MarketBal.Repository.OrderRP;
using MarketBal.Repository.POSManager;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZXing;

namespace InspireSuperStore.Areas.OrderSection.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," + UserRolesConstants.DataEntry + "," + UserRolesConstants.Product + "," + UserRolesConstants.Purchase)]
    [Area("OrderSection")]
    [Route("InvoiceManager/[action]")]
    public class InvoiceManagerController : Controller
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
        private readonly OrderRepository _orderRepo;
        private readonly InvoiceRepository _invoicesRepo;
        private readonly IWebHostEnvironment _env;
        public InvoiceManagerController(IConfiguration config, OneDb oneDb, NotificationService notificationServices, IWebHostEnvironment env)
        {
            _config = config;
            _oneDb = oneDb;
            _attrib = new AttributeRepository(_config, _oneDb);
            _account = new AccountRepository(_config);
            _admin = new AdminPanelRepository(_config, _oneDb);
            _assets = new AssetRepository(_config, _oneDb);
            _hrm = new HumanRespourceRepository(_config, _oneDb);
            _notificationServices = notificationServices;
            _notificationRepository = new NotificationRepository(_oneDb);
            _orderRepo = new OrderRepository(_config, _oneDb);
            _env = env;
            _invoicesRepo = new InvoiceRepository(_config, _oneDb);
        }
        public async Task<IActionResult> Invoices()
        {
            vm.Invoices = await _invoicesRepo.GetInvoices();
            return View(vm);
        }
        public async Task<IActionResult> CreateInvoice()
        {
            vm.Departments = await _attrib.GetDepartment();
            vm.Countries = await _admin.Countries();
            vm.Customers = await _admin.Customers();
            vm.ServingTables = await _assets.ServingTables();
            vm.Employees = await _hrm.GetSaleStaff();
            vm.PaymentMethods = await _assets.PaymentMethods();
            vm.PaymentStatuses = await _assets.PaymentStatuses();
            return View(vm);
        }
        public async Task<IActionResult> SaveInvoice(InvoiceMasterVM model)
        {
            var result = await _invoicesRepo.SaveInvoice(model);
            try
            {
                var invoice = await _invoicesRepo.GenerateInvoiceHTML(result,_env);
                if (invoice != null)
                {
                    var orderparam = new OrderParam
                    {
                        OrderId = result.InvoiceMasterId
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
                    await _notificationServices.SendToRoleGroup("Admin", JsonConvert.SerializeObject(Notification));
                    await _notificationRepository.SaveNotification(Notification);
                    return File(invoice, "application/pdf", "invoice.pdf");
                }
                else
                {
                    return Json(new { success = true, message = "Invoice saved successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = true, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetDuplicateInvoice(InvoiceMasterVM model)
        {
            var result = await _invoicesRepo.GetInvoiceById(model.InvoiceMasterId);

            var invoice = await _invoicesRepo.GenerateInvoiceHTML(result,_env);

            return File(invoice, "application/pdf", "invoice.pdf");


        }

        public async Task<IActionResult> CancelInvoice(InvoiceMasterVM model)
        {
            var result = await _invoicesRepo.CancelInvoice(model.InvoiceMasterId, "");
            if (result)
            {
                return Json(new { statusCode="200" });

            }
            else
            {
                return Json(new { statusCode="300" });

            }
        }



    }
}
