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
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ZXing;

namespace InspireSuperStore.Areas.OrderSection.Controllers
{
    [Authorize(Roles = UserRolesConstants.Admin + "," +  UserRolesConstants.Product + "," + UserRolesConstants.POSUser)]
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
        private readonly ISessionService _sessionService;
        public InvoiceManagerController(IConfiguration config, OneDb oneDb, NotificationService notificationServices, IWebHostEnvironment env, ISessionService sessionService)
        {
            _config = config;
            _oneDb = oneDb;
            _sessionService = sessionService;
            _attrib = new AttributeRepository(_config, _oneDb, _sessionService);
            _account = new AccountRepository(_config, _sessionService);
            _admin = new AdminPanelRepository(_config, _oneDb, _sessionService);
            _assets = new AssetRepository(_config, _oneDb, _sessionService);
            _hrm = new HumanRespourceRepository(_config, _oneDb, _sessionService);
            _notificationServices = notificationServices;
            _notificationRepository = new NotificationRepository(_oneDb);
            _orderRepo = new OrderRepository(_config, _oneDb, _sessionService);
            _env = env;
            _invoicesRepo = new InvoiceRepository(_config, _oneDb,_sessionService);
        }
        public short GetActiveTill()
        {
            string activeTill = HttpContext.Session.GetString("ActiveTill") ?? "Till-1";
            short till = short.Parse(activeTill.Split('-')[1]);
            return till;
        }
        public async Task<IActionResult> Invoices()
        {
            short tillId =  GetActiveTill();
            vm.Invoices = await _invoicesRepo.GetInvoices(tillId);
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
            
            model.TillNo = GetActiveTill();
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

        [HttpPost]
        public async Task<IActionResult> CancelInvoice(InvoiceMasterVM model)
        {
            try
            {
                // Check if model is null or InvoiceMasterId is empty/default GUID
                if (model == null || model.InvoiceMasterId == Guid.Empty)
                {
                    return Json(new { statusCode = "400", message = "Invalid or missing Invoice ID." });
                }

                var result = await _invoicesRepo.CancelInvoice(model.InvoiceMasterId, "");

                if (result)
                {
                    return Json(new { statusCode = "200", message = "Invoice cancelled successfully." });
                }
                else
                {
                    return Json(new { statusCode = "300", message = "Failed to cancel the invoice." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception here if you have a logger (e.g., _logger.LogError(ex, ...))
                return Json(new { statusCode = "500", message = "An error occurred while processing the cancellation." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateReturn(Guid invoiceMasterId)
        {
            try
            {
                if (invoiceMasterId == Guid.Empty)
                {
                    return RedirectToAction("Index");
                }
                var tillId = GetActiveTill();
                // Fetch original invoice with its details, customer, and product variants
                var invoice = await _oneDb.InvoiceMasters
                    .Include(i => i.Customer).
                    Include(i=>i.Customer.Person)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Product)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Variant)
                    .FirstOrDefaultAsync(i => i.InvoiceMasterId == invoiceMasterId);

                if (invoice == null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction("Index");
                }

                // Map directly to your existing InvoiceMasterVM
                vm.Invoice = new InvoiceMasterVM
                {
                    InvoiceMasterId = invoice.InvoiceMasterId, // This will act as the Parent reference when posting back
                    Customer = invoice.Customer != null ? new CustomerVM
                    {
                        CustomerId = invoice.Customer.CustomerId,
                        FirstName = invoice.Customer.Person.FirstName,
                        LastName = invoice.Customer.Person.LastName
                    } : null,
                    CreatedDate = invoice.CreatedDate.Value,
                    GrandTotal = invoice.GrandTotal ?? 0,
                    TillNo = tillId,
                    InvoiceDetails = invoice.InvoiceDetails.Select(d => new InvoiceDetailVM
                    {
                        InvoiceDetailId = d.InvoiceDetailId,
                        InvoiceMasterId = d.InvoiceMasterId,
                        ProductId = d.ProductId,
                        VariantId = d.VariantId,
                        Product = d.Product != null ? new ProductVM { ProductName = d.Product.ProductName } : null,
                        Variant = d.Variant != null ? new ProductVariantVM { ProductName = d.Variant.Product.ProductName } : null,
                        Quantity = d.Quantity, // Original purchased quantity
                        UnitPrice = d.UnitPrice,
                        Cost = d.Cost ?? 0m,
                        LineTotal = d.LineTotal
                    }).ToList()
                };
                vm.PaymentMethods = await _assets.PaymentMethods();
                return View(vm); // Looks for Views/InvoiceManager/CreateReturn.cshtml
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while loading the return page.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> SubmitReturn([FromBody] InvoiceMasterVM model)
        {
            try
            {
                model.TillNo = GetActiveTill();
                var savedReturn = await _invoicesRepo.SaveReturnInvoice(model);

                if (savedReturn != null)
                {
                    return Json(new { statusCode = "200", message = "Return processed and inventory restocked successfully." });
                }

                return Json(new { statusCode = "300", message = "Failed to process return." });
            }
            catch (Exception ex)
            {
                return Json(new { statusCode = "500", message = ex.Message });
            }
        }


















    }
}
