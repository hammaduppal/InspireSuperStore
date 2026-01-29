using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.InvoiceRP;
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
        private readonly InvoiceRepository _invoiceRepo;
        public OrderRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config,_onedb);
            _invoiceRepo=new InvoiceRepository(_config, _onedb);
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

                    // Check if PaymentStatus exists
                    PaymentStatus = x.PaymentStatus == null ? null : new PaymentStatusVM
                    {
                        PaymentStatusId = x.PaymentStatus.PaymentStatusId,
                        Name = x.PaymentStatus.Name
                    },

                    // Check if OrderSource exists
                    OrderSource = x.OrderSource == null ? null : new InvoiceSourceVM
                    {
                        InvoiceSourceId = x.OrderSource.InvoiceSourceId,
                        SourceName = x.OrderSource.SourceName
                    },

                    // Check if Employee exists (Common culprit)
                    Employee = x.Employee == null ? null : new EmployeeVM
                    {
                        EmployeeId = x.Employee.EmployeeId,
                        EmployeeCode = x.Employee.EmployeeCode,
                        FirstName = x.Employee.Person.FirstName,
                        LastName = x.Employee.Person.LastName,
                    },

                    // Check if ServingTable exists (Common in POS systems to be null)
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

                    // Check if PaymentMethod exists
                    PaymentMethod = x.PaymentMethod == null ? null : new PaymentMethodVM
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
                var model = await _onedb.OrderMasters
      .Where(x => x.OrderMasterId == orderMasterId)
      .Select(x => new OrderMasterVM
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

          // Handle Top-Level Navigations
          PaymentStatus = x.PaymentStatus == null ? null : new PaymentStatusVM
          {
              PaymentStatusId = x.PaymentStatus.PaymentStatusId,
              Name = x.PaymentStatus.Name
          },
          OrderSource = x.OrderSource == null ? null : new InvoiceSourceVM
          {
              InvoiceSourceId = x.OrderSource.InvoiceSourceId,
              SourceName = x.OrderSource.SourceName
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
          ParentOrderId = x.ParentOrderId,
          OrderDate = x.OrderDate,
          CreatedBy = x.CreatedBy,
          CreatedDate = x.CreatedDate,

          // Handle Nested Order Details
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

              // Critical: Guard the Variant and its nested relations
              Variant = od.Variant == null ? null : new ProductVariantVM
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

                  // Deep nested null checks
                  ProductName = od.Variant.Product != null ? od.Variant.Product.ProductName : null,
                  ProductDescription = od.Variant.Product != null ? od.Variant.Product.ProductDescription : null,
                  ProductSlug = od.Variant.Product != null ? od.Variant.Product.ProductSlug : null,
                  ColorName = od.Variant.Color != null ? od.Variant.Color.ColorName : null,
                  MaterialName = od.Variant.Material != null ? od.Variant.Material.MaterialName : null,
                  SizeName = od.Variant.Size != null ? od.Variant.Size.SizeName : null,
                  UOMName = (od.Variant.Product != null && od.Variant.Product.Uom != null) ? od.Variant.Product.Uom.Uomname : null,
                  SubUOMName = od.Variant.SubUom != null ? od.Variant.SubUom.SubUomname : null,
                  BrandName = (od.Variant.Product != null && od.Variant.Product.Brand != null) ? od.Variant.Product.Brand.BrandName : null,

                  PriceFormat = od.Variant.PriceFormat,
                  SubUomid = od.Variant.SubUomid,
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

        public async Task<InvoiceMaster> OrderToInvoice(Guid OmId)
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
                    PaymentStatusId=order.PaymentStatusId.Value,
                    InvoiceSourceId=order.OrderSourceId,

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
                return await _invoiceRepo.SaveInvoice(invoiceModel);
            }
            catch (Exception )
            {

                throw;
            }
            // 1. Get the order
           
        }

        public async Task<SaveStatusVM> SaveOrder(OrderMasterVM model)
        {
            using (var transaction = await _onedb.Database.BeginTransactionAsync())
            {
                try
                {
                    var commonParams = CommonParamHelper.GetCommonParams();

                    // Generate Order Number
                    var lastOrder = await _onedb.OrderMasters
                        .OrderByDescending(i => i.CreatedDate)
                        .FirstOrDefaultAsync();

                    var orderPrefix = AppDataUtility.SystemPreferences.InvoicePrefix ?? "ORD";
                    string newOrderNo = $"{orderPrefix}-001";

                    if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.OrderNo))
                    {
                        var lastNo = lastOrder.OrderNo.Split('-')[1];
                        if (int.TryParse(lastNo, out int number))
                        {
                            newOrderNo = $"{orderPrefix}-{(number + 1).ToString("D3")}";
                        }
                    }

                    // ---------- Calculations ----------
                    decimal subTotal = model.OrderDetails.Sum(d => (d.UnitPrice ?? 0) * (d.Quantity ?? 0));
                    decimal? totalTax = model.OrderDetails.Sum(d => ((d.TaxRate ?? 0) * ((d.UnitPrice ?? 0) * (d.Quantity ?? 0))) / 100);
                    decimal discount = model.DiscountAmount ?? 0;
                    decimal grandTotal = subTotal + (totalTax ?? 0) - discount;
                    Guid orderMasterId = Guid.NewGuid();
                    // ---------- Order Master ----------
                    var orderMaster = new OrderMaster
                    {
                        OrderMasterId = orderMasterId,
                        OrderNo = newOrderNo,
                        OrderDate = commonParams.CreatedOn ?? DateTime.Now,

                        ParentOrderId = model.ParentOrderId,

                        CustomerId = (model.CustomerId.HasValue && model.CustomerId.Value != Guid.Empty)
                                        ? model.CustomerId
                                        : null,

                        TotalAmount = subTotal,
                        DiscountAmount = discount,
                        TaxAmount = totalTax,
                        GrandTotal = grandTotal,
                        
                        PaymentMethodId = model.PaymentMethodId,
                        PaymentStatusId = model.PaymentStatusId ?? 2,
                        ShippingTypeId = model.ShippingTypeId ?? 1,
                        OrderSourceId = model.OrderSourceId ?? (int)AppConstants.InvoiceSource.POS,
                        OrderStatusId=(int)OrderStatusEnum.Pending,
                        CustomerRemarks = model.CustomerRemarks,
                        OfficeRemarks = model.OfficeRemarks,

                        CreatedBy = model.CreatedBy ?? 1,
                        CreatedDate = commonParams.CreatedOn ?? DateTime.Now,
                        UpdatedBy = model.UpdatedBy,
                        UpdatedDate = commonParams.CreatedOn ?? DateTime.Now,

                        ServingTableId = (model.ServingTableId.HasValue && model.ServingTableId.Value != Guid.Empty)
                                            ? model.ServingTableId
                                            : null,

                        EmployeeId = (model.EmployeeId.HasValue && model.EmployeeId.Value != 0)
                                        ? model.EmployeeId
                                        : null
                    };
                    
                    _onedb.OrderMasters.Add(orderMaster);
                    await _onedb.SaveChangesAsync();

                    // ---------- Order Details ----------
                    foreach (var detail in model.OrderDetails)
                    {
                        decimal lineTotal = (detail.UnitPrice ?? 0) * (detail.Quantity ?? 0);
                        decimal taxAmount = ((detail.TaxRate ?? 0) * lineTotal) / 100;
                        decimal lineTotalWithTax = lineTotal + taxAmount;

                        var orderDetail = new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid(),
                            OrderMasterId = orderMaster.OrderMasterId,
                            ProductId = detail.ProductId,
                            VariantId = detail.VariantId,

                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice,
                            Discount = detail.Discount ?? 0,
                            TaxRate = detail.TaxRate,
                            TaxAmount = taxAmount,
                            LineTotal = lineTotal,
                            LineTotalWithTax = lineTotalWithTax,
                            Remarks = detail.Remarks ?? string.Empty,
                            CreatedDate = DateTime.Now
                        };

                        _onedb.OrderDetails.Add(orderDetail);
                    }

                    await _onedb.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new SaveStatusVM
                    {
                        NewItemId = orderMasterId,
                        StatusId = 1
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

    }
}
