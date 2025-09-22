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
                    Customer = x.Customer == null ? null : new CustomerVM
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
        public async Task<OrderMasterVM> GetOrderById(Guid orderMasterId)
        {
            try
            {
                var model = await _onedb.OrderMasters.Where(x=>x.OrderMasterId==orderMasterId).Select(x => new OrderMasterVM
                {
                    OrderMasterId = x.OrderMasterId,
                    OrderNo = x.OrderNo,
                    GrandTotal = x.GrandTotal,
                    OrderStatusId = x.OrderStatusId,
                    CustomerId = x.CustomerId,
                    DiscountAmount = x.DiscountAmount, TotalAmount = x.TotalAmount, 
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
                    Customer = x.Customer == null ? null : new CustomerVM
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
                    OrderDetails = x.OrderDetails.Select(od => new OrderDetailVM
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderMasterId = od.OrderMasterId,
                        ProductId = od.ProductId,
                        VariantId = od.VariantId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        TaxRate = od.TaxRate,
                        TaxAmount = od.TaxAmount,
                        LineTotal = od.LineTotal,
                        LineTotalWithTax = od.LineTotalWithTax,
                        Discount = od.Discount,
                        Remarks = od.Remarks, Variant=new ProductVariantVM
                        {
                            VariantId = od.Variant.VariantId,
                            TaxSlabId = od.Variant.TaxSlabId,
                            MaterialId = od.Variant.MaterialId,
                            ColorId = od.Variant.ColorId,
                            SizeId = od.Variant.SizeId,
                            ProductId = od.Variant.ProductId,
                            BarCode = od.Variant.BarCode,
                            MinQty = od.Variant.MinQty,
                            MaxQty = od.Variant.MaxQty,
                            LastPurchase = od.Variant.LastPurchase,
                            CreatedOn = od.Variant.CreatedOn,
                            Createdby = od.Variant.Createdby,
                            ModifiedOn = od.Variant.ModifiedOn,
                            IsActive = od.Variant.IsActive,
                            IsDeleted = od.Variant.IsDeleted,
                            BranchId = od.Variant.BranchId,
                            VariantImageId = od.Variant.VariantImageId,

                            ProductName = od.Variant.Product.ProductName,
                            ProductDescription = od.Variant.Product.ProductDescription,
                            ProductSlug = od.Variant.Product.ProductSlug,
                            ColorName = od.Variant.Color.ColorName,
                            MaterialName = od.Variant.Material.MaterialName,
                            SizeName = od.Variant.Size.SizeName,
                            UOMName = od.Variant.Product.Uom.Uomname,
                            SubUOMName = od.Variant.SubUom.SubUomname,
                            PriceFormat = od.Variant.PriceFormat,
                            SubUomid = od.Variant.SubUomid,
                            BrandName = od.Variant.Product.Brand.BrandName,
                            QuantityPerUnit = od.Variant.QuantityPerUnit
                        },
                        CreatedDate = od.CreatedDate
                    }).ToList(),
                }).FirstOrDefaultAsync();

                return model;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<int> UpdateOrderStatus(int statusId,Guid OrderMasterId)
        {
            var order = await _onedb.OrderMasters.Where(x => x.OrderMasterId == OrderMasterId).FirstOrDefaultAsync();
            order.OrderStatusId = statusId;
            return await _onedb.SaveChangesAsync();
        }


    }
}
