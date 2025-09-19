using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.OrderRP
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public OrderRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }

        public async Task<List<OrderMasterVM>> GetOrders()
        {
            try
            {
                var model = await _onedb.OrderMasters.Select(x => new OrderMasterVM
                {
                    OrderMasterId = x.OrderMasterId,
                    OrderNo = x.OrderNo,
                    GrandTotal = x.GrandTotal,
                    OrderStatusId = x.OrderStatusId,
                    CustomerId = x.CustomerId,
                    DiscountAmount = x.DiscountAmount,
                    TaxAmount = x.TaxAmount,
                    PaymentMethodId = x.PaymentMethodId,
                    PaymentStatus = new PaymentStatusVM
                    {
                        PaymentStatusId = x.PaymentStatus.PaymentStatusId,
                        Name = x.PaymentStatus.Name
                    },
                    OrderSource = new InvoiceSourceVM
                    {
                        InvoiceSourceId = x.OrderSource.InvoiceSourceId,
                        SourceName = x.OrderSource.SourceName
                    },
                    Employee = new EmployeeVM
                    {
                        EmployeeId = x.Employee.EmployeeId,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },
                    ServingTable = new ServingTableVM
                    {
                        ServingTableId = x.ServingTable.ServingTableId,
                        TableName = x.ServingTable.TableName,
                    },
                    Customer = new CustomerVM
                    {
                        CustomerId = x.Customer.CustomerId,
                        FirstName = x.Customer.Person.FirstName,
                        LastName = x.Customer.Person.LastName,
                        Email = x.Customer.Person.Email,
                    },
                    PaymentMethod = new PaymentMethodVM
                    {
                        PaymentMethodId = x.PaymentMethod.PaymentMethodId,
                        Name = x.PaymentMethod.Name
                    },
                    ParentOrderId = x.ParentOrderId,
                    OrderDate = x.OrderDate,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate,
                }).ToListAsync();
                return model;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
