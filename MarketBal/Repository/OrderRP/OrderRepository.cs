using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.POSManager;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;
using static MainModels.DTOModels.AppConstants;

namespace MarketBal.Repository.OrderRP
{
    public class OrderRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly POSRepository _pOSRepository;
        public OrderRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
            _pOSRepository = new POSRepository(_config, _onedb);
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
                var model = await _onedb.OrderMasters.Where(x => x.OrderMasterId == orderMasterId).Select(x => new OrderMasterVM
                {
                    OrderMasterId = x.OrderMasterId,
                    OrderNo = x.OrderNo,
                    GrandTotal = x.GrandTotal,
                    OrderStatusId = x.OrderStatusId,
                    CustomerId = x.CustomerId,
                    DiscountAmount = x.DiscountAmount,
                    TotalAmount = x.TotalAmount,
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
                        Remarks = od.Remarks,
                        Variant = new ProductVariantVM
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

        public async Task<int> UpdateOrderStatus(int statusId, Guid OrderMasterId)
        {
            var order = await _onedb.OrderMasters.Where(x => x.OrderMasterId == OrderMasterId).FirstOrDefaultAsync();
            order.OrderStatusId = statusId;
            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> UpdateOrder(OrderMasterVM order)
        {
            var existingOrder = await _onedb.OrderMasters
                .Include(x => x.OrderDetails)
                .FirstOrDefaultAsync(x => x.OrderMasterId == order.OrderMasterId);

            if (existingOrder == null)
                throw new Exception("Order not found");

            // 🔹 Update OrderMaster fields
            existingOrder.PaymentMethodId = order.PaymentMethodId;
            existingOrder.PaymentStatusId = order.PaymentStatusId ?? 1;
            existingOrder.ShippingTypeId = order.ShippingTypeId ?? 1;
            existingOrder.CustomerId = order.CustomerId;
            existingOrder.EmployeeId = order.EmployeeId;
            existingOrder.ServingTableId = order.ServingTableId;
            existingOrder.TotalAmount = order.TotalAmount;
            existingOrder.DiscountAmount = order.DiscountAmount;
            existingOrder.TaxAmount = order.TaxAmount;
            existingOrder.GrandTotal = order.GrandTotal;
            existingOrder.CustomerRemarks = order.CustomerRemarks;
            existingOrder.OfficeRemarks = order.OfficeRemarks;
            existingOrder.OrderStatusId = order.OrderStatusId;
            existingOrder.UpdatedDate = DateTime.Now;
            existingOrder.UpdatedBy = order.UpdatedBy;

            var incomingDetails = order.OrderDetails ?? new List<OrderDetailVM>();

            // 🔹 Update or Add details
            foreach (var detail in incomingDetails)
            {
                var existingDetail = existingOrder.OrderDetails
                    .FirstOrDefault(d => d.VariantId == detail.VariantId);

                if (existingDetail != null)
                {
                    // Update existing
                    existingDetail.Quantity = detail.Quantity ?? 0;
                    existingDetail.UnitPrice = detail.UnitPrice ?? 0;
                    existingDetail.TaxRate = detail.TaxRate ?? 0;
                    existingDetail.TaxAmount = detail.TaxAmount ?? 0;
                    existingDetail.LineTotal = detail.LineTotal ?? 0;
                    existingDetail.LineTotalWithTax = detail.LineTotalWithTax ?? 0;
                    existingDetail.Discount = detail.Discount ?? 0;
                    existingDetail.Remarks = detail.Remarks;
                }
                else
                {
                    // Add new
                    var newDetail = new OrderDetail
                    {
                        OrderDetailId = Guid.NewGuid(),
                        OrderMasterId = existingOrder.OrderMasterId,
                        ProductId = detail.ProductId,
                        VariantId = detail.VariantId,
                        Quantity = detail.Quantity ?? 0,
                        UnitPrice = detail.UnitPrice ?? 0,
                        TaxRate = detail.TaxRate ?? 0,
                        TaxAmount = detail.TaxAmount ?? 0,
                        LineTotal = detail.LineTotal ?? 0,
                        LineTotalWithTax = detail.LineTotalWithTax ?? 0,
                        Discount = detail.Discount ?? 0,
                        Remarks = detail.Remarks,
                        CreatedDate = DateTime.Now
                    };
                    existingOrder.OrderDetails.Add(newDetail);
                }
            }

            // 🔹 Remove missing details
            var toRemove = existingOrder.OrderDetails
                .Where(d => !incomingDetails.Any(i => i.VariantId == d.VariantId))
                .ToList();

            foreach (var removeItem in toRemove)
            {
                _onedb.OrderDetails.Remove(removeItem);
            }

            return await _onedb.SaveChangesAsync();
        }

        public async Task<int> OrderToInvoice(Guid OmId)
        {
            try
            {
                var order = await _onedb.OrderMasters
               .Include(o => o.OrderDetails)
               .FirstOrDefaultAsync(o => o.OrderMasterId == OmId);

                if (order == null)
                    throw new Exception("Order not found.");

                // 2. Map Order → InvoiceMasterVM
                var invoiceModel = new InvoiceMasterVM
                {
                    CustomerId = order.CustomerId,
                    PaymentMethodId = order.PaymentMethodId.Value,
                    DiscountAmount = order.DiscountAmount,
                    TaxAmount = order.TaxAmount,
                    GrandTotal = order.GrandTotal.Value,
                    TotalAmount = order.TotalAmount.Value,
                    ServingTableId = order.ServingTableId,
                    EmployeeId = order.EmployeeId,
                    CustomerRemarks = order.CustomerRemarks,
                    OfficeRemarks = order.OfficeRemarks,

                    InvoiceDetails = order.OrderDetails.Select(d => new InvoiceDetailVM
                    {
                        ProductId = d.ProductId,
                        VariantId = d.VariantId,
                        Quantity = d.Quantity ?? 0,
                        UnitPrice = d.UnitPrice ?? 0,
                        TaxRate = d.TaxRate ?? 0,
                        TaxAmount = d.TaxAmount ?? 0,
                        LineTotal = d.LineTotal ?? 0,
                        LineTotalWithTax = d.LineTotalWithTax ?? 0,
                        Discount = d.Discount ?? 0,
                        Remarks = d.Remarks
                    }).ToList()
                };
                order.OrderStatusId = (int)OrderStatusEnum.Delivered;
                await _onedb.SaveChangesAsync();
                // 3. Call existing SaveInvoice
                return await _pOSRepository.SaveInvoice(invoiceModel);
            }
            catch (Exception ex)
            {

                throw;
            }
            // 1. Get the order
           
        }

    }
}
