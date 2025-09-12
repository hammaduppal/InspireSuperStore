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

namespace MarketBal.Repository.POSManager
{
    public class POSRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        private readonly PdfService _pdfservice;
        public POSRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
            _pdfservice = new PdfService();
        }
        public async Task<int> SaveInvoice([FromBody]InvoiceMasterVM model)
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
                        PaymentStatusId = 1,
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
                        decimal taxAmount = (detail.TaxRate ) * lineTotal / 100;
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
                            TaxRate = detail.TaxRate ,
                            TaxAmount = taxAmount,
                            LineTotal = lineTotal,
                            LineTotalWithTax = lineTotalWithTax,
                            Remarks = detail.Remarks ?? string.Empty
                        };

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

                    return 1;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<byte[]> GenerateInvoiceHTML(InvoiceMasterVM items)
        {
            string companyName = "";
            string companyAddress = "";
            string companyContact = "";
            string invoiceNo = "";
            string invoiceDate = "";
            string customerName = "";
            string qrCodeBase64 = "";
            decimal subTotal = 0;
            decimal discount = 0;
            decimal tax = 0;
            decimal grandTotal = 0;
            string footerMessage = "";
            
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
                    </div>
                </section>
                </body>
                </html>";

            //var result = await new PdfPupetter().GeneratePdfFromHtml(html);
            var result =await _pdfservice.GeneratePdfFromHtml(html) ;
            //var result = PdfGenerator.GeneratePdf(html);
            return result;
        }
     
    public enum SizeVM
    {
        A0,
        A1,
        A2,
        A3,
        A4,
        A5,
        A6,
        A7,
        A8,
        A9,
        B0,
        B1,
        B2,
        B3,
        B4,
        B5,
        B6,
        B7,
        B8,
        B9,
        B10,
        C5E,
        Comm10E,
        Dle,
        Executive,
        Folio,
        Ledger,
        Legal,
        Letter,
        Tabloid
    }
}
}
