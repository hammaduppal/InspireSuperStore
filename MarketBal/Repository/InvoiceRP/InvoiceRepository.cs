using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.POSManager;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.InvoiceRP
{
    public class InvoiceRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly POSRepository _pOSRepository;
        public InvoiceRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
            _pOSRepository = new POSRepository(_config, _onedb);
        }

        public async Task<List<InvoiceMasterVM>> GetInvoices()
        {
            try
            {
                var model = await _onedb.InvoiceMasters.Select(x => new InvoiceMasterVM
                {
                    InvoiceMasterId = x.InvoiceMasterId,
                    InvoiceNo = x.InvoiceNo,
                    GrandTotal = x.GrandTotal,
                    CustomerId = x.CustomerId,
                    DiscountAmount = x.DiscountAmount,
                    TaxAmount = x.TaxAmount,
                    PaymentMethodId = x.PaymentMethodId.Value,
                    PaymentStatus = x.PaymentStatus == null ? null : new PaymentStatusVM
                    {
                        PaymentStatusId = x.PaymentStatus.PaymentStatusId,
                        Name = x.PaymentStatus.Name
                    },
                    InvoiceSource = x.InvoiceSource == null ? null : new InvoiceSourceVM
                    {
                        InvoiceSourceId = x.InvoiceSource.InvoiceSourceId,
                        SourceName = x.InvoiceSource.SourceName
                    },
                    Employee = x.Employee == null ? null : new EmployeeVM
                    {
                        EmployeeId = x.Employee.EmployeeId,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },
                    ServingTable = x.ServingTable == null ? null : new ServingTableVM
                    {
                        ServingTableId = x.ServingTable.ServingTableId,
                        TableName = x.ServingTable.TableName,
                    },
                    Customer = x.Customer == null ? null : new CustomerVM
                    {
                        CustomerId = x.Customer.CustomerId,
                        FirstName = x.Customer.Person.FirstName,
                        LastName = x.Customer.Person.LastName,
                        Email = x.Customer.Person.Email,
                    },
                    PaymentMethod = x.PaymentMethod == null ? null : new PaymentMethodVM
                    {
                        PaymentMethodId = x.PaymentMethod.PaymentMethodId,
                        Name = x.PaymentMethod.Name
                    },
                    InvoiceDate = x.InvoiceDate,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                }).ToListAsync();

                return model;
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}