using System.Text;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using MarketBal.Helper.PDF;
using MarketBal.Repository.Products;
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
        public POSRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config);
        }
        public async Task<int> SaveInvoice(InvoiceMasterVM model)
        {
            using (var transaction = await _onedb.Database.BeginTransactionAsync())
            {
                try
                {
                    var commonParams = CommonParamHelper.GetCommonParams();
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
                    // Map Invoice Master
                    var invoiceMaster = new InvoiceMaster
                    {
                        InvoiceMasterId = Guid.NewGuid(),
                        InvoiceNo = newInvoiceNo,
                        InvoiceDate = commonParams.CreatedOn.Value,

                        CustomerId = (model.CustomerId.HasValue && model.CustomerId.Value != Guid.Empty)
                                        ? model.CustomerId
                                        : null,

                        TotalAmount = model.TotalAmount,
                        DiscountAmount = model.DiscountAmount,
                        TaxAmount = model.TaxAmount,
                        NetAmount = model.NetAmount,
                        PaymentMethodId = model.PaymentMethodId,
                        PaymentStatus = model.PaymentStatus,
                        Remarks = model.Remarks,
                        CreatedBy = 1,
                        CreatedDate = commonParams.CreatedOn,
                        UpdatedDate = commonParams.CreatedOn,
                        ServingTableId = (model.ServingTableId.HasValue && model.ServingTableId.Value != Guid.Empty)
                                            ? model.ServingTableId
                                            : null,

                        EmployeeId = (model.EmployeeId.HasValue && model.EmployeeId.Value != 0)
                                        ? model.EmployeeId
                                        : null
                    };

                    _onedb.InvoiceMasters.Add(invoiceMaster);
                    await _onedb.SaveChangesAsync();

                    foreach (var detail in model.InvoiceDetails)
                    {
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
                            Discount = detail.Discount,
                            Tax = detail.Tax,
                            TotalAmount = detail.TotalAmount,
                            Remarks = detail.Remarks
                        };

                        _onedb.InvoiceDetails.Add(invoiceDetail);
                        if (detail.VariantId.HasValue && detail.VariantId.Value != Guid.Empty)
                        {
                            var variant = await _onedb.ProductVariants
                                .FirstOrDefaultAsync(v => v.VariantId == detail.VariantId);

                            if (variant != null)
                            {

                                variant.QoH -= detail.Quantity;
                                _onedb.ProductVariants.Update(variant);
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
       
      

            var result = PdfGenerator.GeneratePdf(html);
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
