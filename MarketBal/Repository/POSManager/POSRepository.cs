using System.Text;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using MarketBal.Helper.PDF;
using MarketBal.Helper.PDF.OMS.Data.Repositories.PDFGenerate;
using MarketBal.Repository.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace MarketBal.Repository.POSManager
{
    public class POSRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;

        public POSRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<InvoiceMaster> SaveInvoice([FromBody] InvoiceMasterVM model)
        {
            using (var transaction = await _onedb.Database.BeginTransactionAsync())
            {
                try
                {
                    var commonParams = CommonParamHelper.GetCommonParams();

                    // Generate Invoice Number
                    var lastInvoice = await _onedb.InvoiceMasters
                        .OrderByDescending(i => i.CreatedDate)
                        .FirstOrDefaultAsync();

                    var invoicePrefix = AppDataUtility.SystemPreferences.InvoicePrefix;
                    string newInvoiceNo = $"{invoicePrefix}-001";

                    if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNo))
                    {
                        var lastNo = lastInvoice.InvoiceNo.Split('-')[1];
                        if (int.TryParse(lastNo, out int number))
                        {
                            newInvoiceNo = $"{invoicePrefix}-{(number + 1).ToString("D3")}";
                        }
                    }

                    // ---------- Calculations ----------
                    decimal totalAmount = model.InvoiceDetails.Sum(d => d.UnitPrice * d.Quantity);
                    decimal totalTax = model.InvoiceDetails.Sum(d => (d.TaxRate) * (d.UnitPrice * d.Quantity) / 100);
                    decimal discount = model.DiscountAmount ?? 0;
                    decimal grandTotal = totalAmount + totalTax - discount;

                    // ---------- Invoice Master ----------
                    var invoiceMaster = new InvoiceMaster
                    {
                        InvoiceMasterId = Guid.NewGuid(),
                        InvoiceNo = newInvoiceNo,
                        InvoiceDate = commonParams.CreatedOn ?? DateTime.Now,

                        CustomerId = (model.CustomerId.HasValue && model.CustomerId.Value != Guid.Empty)
                                        ? model.CustomerId
                                        : null,

                        TotalAmount = totalAmount,
                        DiscountAmount = discount,
                        TaxAmount = totalTax,
                        GrandTotal = grandTotal,

                        PaymentMethodId = model.PaymentMethodId,
                        PaymentStatusId = model.PaymentStatusId,
                        ShippingTypeId = 1,
                        InvoiceSourceId = (int)AppConstants.InvoiceSource.POS,

                        CustomerRemarks = model.CustomerRemarks,
                        OfficeRemarks = model.OfficeRemarks,

                        CreatedBy = 1,
                        CreatedDate = commonParams.CreatedOn ?? DateTime.Now,
                        UpdatedDate = commonParams.CreatedOn ?? DateTime.Now,

                        ServingTableId = (model.ServingTableId.HasValue && model.ServingTableId.Value != Guid.Empty)
                                            ? model.ServingTableId
                                            : null,

                        EmployeeId = (model.EmployeeId.HasValue && model.EmployeeId.Value != 0)
                                        ? model.EmployeeId
                                        : null
                    };

                    _onedb.InvoiceMasters.Add(invoiceMaster);
                    await _onedb.SaveChangesAsync();

                    // ---------- Invoice Details ----------
                    foreach (var detail in model.InvoiceDetails)
                    {
                        decimal lineTotal = detail.UnitPrice * detail.Quantity;
                        decimal taxAmount = (detail.TaxRate) * lineTotal / 100;
                        decimal lineTotalWithTax = lineTotal + taxAmount;

                        var invoiceDetail = new InvoiceDetail
                        {
                            InvoiceDetailId = Guid.NewGuid(),
                            InvoiceMasterId = invoiceMaster.InvoiceMasterId,
                            ProductId = detail.ProductId,
                            VariantId = (detail.VariantId.HasValue && detail.VariantId.Value != Guid.Empty)
                                            ? detail.VariantId
                                            : null,

                            Quantity = detail.Quantity,
                            UnitPrice = detail.UnitPrice,
                            Discount = detail.Discount ?? 0,
                            TaxRate = detail.TaxRate,
                            TaxAmount = taxAmount,
                            LineTotal = lineTotal,
                            LineTotalWithTax = lineTotalWithTax,
                            Remarks = detail.Remarks ?? string.Empty
                        };
                        invoiceMaster.InvoiceDetails.Add(invoiceDetail);
                        _onedb.InvoiceDetails.Add(invoiceDetail);

                        // ---------- Update Inventory ----------
                        if (detail.VariantId.HasValue && detail.VariantId.Value != Guid.Empty)
                        {
                            var branchstock = await _onedb.BranchStocks
                                .FirstOrDefaultAsync(v =>
                                    v.ProductVariantId == detail.VariantId &&
                                    v.BranchId == AppDataUtility.SessionUser.Person.Branch.BranchId);

                            if (branchstock != null)
                            {
                                branchstock.Qty -= detail.Quantity;
                                _onedb.BranchStocks.Update(branchstock);
                            }
                        }

                        var product = await _onedb.Products
                            .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

                        if (product != null)
                        {
                            product.Qoh -= detail.Quantity;
                            _onedb.Products.Update(product);
                        }
                    }

                    await _onedb.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return invoiceMaster;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public class SaveStatusVM
        {
            public int StatusId { get; set; }
            public Guid NewItemId { get; set; }
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


        public async Task<byte[]> GenerateInvoiceHTML(InvoiceMaster model)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in model.InvoiceDetails)
                {
                    sb.AppendLine($"<tr>");
                    sb.AppendLine($"<td style='text-align:left; font-size:12px;'>{item.Product.ProductName}</td> <td style='text-align:left; font-size:12px;'>{item.Quantity}</td> <td style='text-align:left; font-size:12px;'>{item.LineTotal}</td>");
                    sb.AppendLine($"</tr>");
                }

                var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                 <meta charset='UTF-8'>
                   <style>
                {ReportHelper.GetCustomCSS()}
                    </style>
                </head>
                <body>
                <section style='width:80mm; margin:0 auto;'>
                     <div class='company-info'>
<p>
    <img width='250px' height='20px' src='{GenerateBarCode.GenerateBarcode("INV-1111")}' alt='QR Code' />
</p>
                      <div class='row align-items-center text-center'>
        <!-- Logo -->
        <div class='col-3'>
          	<img src=""/global_assets/images/logo_dark.png"" />
        </div>

        <!-- Company Name -->
        <div class='col-9'>
            <h3 style='font-size:14px; margin:0;'>
                {@AppDataUtility.SystemPreferences.CompanyName}
            </h3>
        </div>
    </div>
                        <p>123 Main St, City, Country</p>
                        <p>Phone: +1234567890 | Email: info@company.com</p>


                    </div>
<div class='row'>
<table class='table table-striped'>
<thead>
<tr>
<th style='text-align:left; font-size:12px;'>Item</th>
<th style='text-align:left; font-size:12px;'>Qty</th>
<th style='text-align:left; font-size:12px;'>Price</th>
</tr>
</thead>
<tbody>
{sb.ToString()}
</tbody>
<tfoot>
<tr>
<td colspan='2' style='text-align:left; font-size:12px;'><strong>Subtotal</strong></td>
<td style='text-align:left; font-size:12px;'>{model.TotalAmount}</td>
</tr>
<tr>
<td colspan='2' style='text-align:left; font-size:12px;'><strong>Tax</strong></td>
<td style='text-align:left; font-size:12px;'>{model.TaxAmount}</td>
</tr>
<tr>
<td colspan='2' style='text-align:left; font-size:12px;'><strong>Discount</strong></td>
<td style='text-align:left; font-size:12px;'>{model.DiscountAmount}</td>
</tr>
<tr>
<td colspan='2' style='text-align:left; font-size:12px;'><strong>Total</strong></td>
<td style='text-align:left; font-size:12px;'>{model.GrandTotal}</td>
</tr>
</tfoot>
</table>
</div>
                </section>
<div style='page-break-after: always;'></div>
  
</body>
                </html>
";

                var pdfOptions = new PdfOptions
                {
                    // Format = PaperFormat.A4,
                    PrintBackground = true,
                    Width = "80mm",
                    PreferCSSPageSize = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "2mm",
                        Bottom = "2mm",
                        Left = "2mm",
                        Right = "2mm"
                    }
                };

                pdfOptions.PageRanges = "1";
                var result = await new PdfPupetter().GeneratePdfFromHtml(html, pdfOptions);
                return result;
            }
            catch (Exception ex)
            {
                var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                 <meta charset='UTF-8'>
                   <style>
                {ReportHelper.GetCustomCSS()}
                    </style>
                </head>
                <body>
                <section style='width:80mm; margin:0 auto;'>
                     <div class=""company-info"">
                        <h2>My Company Name</h2>
                        <p>123 Main St, City, Country</p>
                        <p>Phone: +1234567890 | Email: info@company.com</p>
<p>{ex.InnerException.Message}</p>
<p>{ex.Message}</p>

                    </div>
                </section>
                </body>
                </html>";
                var pdfOptions = new PdfOptions
                {
                    PrintBackground = true,
                    Width = "80mm",
                    Height = null, // continuous
                    MarginOptions = new MarginOptions
                    {
                        Top = "2mm",
                        Bottom = "2mm",
                        Left = "2mm",
                        Right = "2mm"
                    }
                };


                var result = await new PdfPupetter().GeneratePdfFromHtml(html, pdfOptions);
                //var result =await _pdfservice.GeneratePdfFromHtml(html) ;
                //var result = PdfGenerator.GeneratePdf(html);
                return result;

            }

        }


    }
}
