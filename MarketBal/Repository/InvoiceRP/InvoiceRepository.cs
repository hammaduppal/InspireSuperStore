using System.Text;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using MarketBal.Helper.PDF.OMS.Data.Repositories.PDFGenerate;
using MarketBal.Repository.AccountingRP;
using MarketBal.Repository.HRM;
using MarketBal.Repository.POSManager;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using static MainModels.DTOModels.AppConstants;

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
        private readonly HumanRespourceRepository _hrmRepository;
        private readonly JournalsRepository _journalRepo;
        private readonly AccountsReceivableRepository _accountsReceivableRepository ;
        public InvoiceRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config, _onedb);
            _pOSRepository = new POSRepository(_config, _onedb);
            _hrmRepository = new HumanRespourceRepository(_config, _onedb);
            _journalRepo = new JournalsRepository(_config, _onedb);
            _accountsReceivableRepository = new AccountsReceivableRepository(_config, _onedb);
        }

        public async Task<List<InvoiceMasterVM>> GetInvoices()
        {
            try
            {
                var model = await _onedb.InvoiceMasters.Select(x => new InvoiceMasterVM
                {
                    InvoiceMasterId = x.InvoiceMasterId,
                    InvoiceNo = x.InvoiceNo,
                    GrandTotal = x.GrandTotal.Value,
                    CustomerId = x.CustomerId,
                    DiscountAmount = x.DiscountAmount,
                    TaxAmount = x.TaxAmount,
                    PaymentStatusId=x.PaymentStatusId.Value,
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
                    InvoiceDate = x.InvoiceDate.Value,
                    CreatedBy = x.CreatedBy,
                    CreatedDate = x.CreatedDate.Value,
                }).ToListAsync();

                return model;
            }
            catch (Exception)
            {

                throw;
            }

        }


        public async Task<InvoiceMaster> SaveInvoice(InvoiceMasterVM model)
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
                    if (model.PaymentStatusId == 1)
                    {
                        invoiceMaster.DueDate = commonParams.CreatedOn?.Date;

                    }
                    else
                    {
                        invoiceMaster.DueDate = model.DueDate;
                    }
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
                    bool isCashINvoice = false;
                    if (invoiceMaster.PaymentStatusId == 1)
                    {
                        isCashINvoice = true;
                    }
                    

                    var customer = await _hrmRepository.GetCustomer(invoiceMaster.CustomerId.Value);
                    var cost = await GetCostofGoods(
                        model.InvoiceDetails.Select(x => new InvoiceDetail
                        {
                            VariantId = x.VariantId,
                            Quantity = x.Quantity
                        }).ToList());
                    var productIds = model.InvoiceDetails.Select(x => x.ProductId).ToList();

                    bool isServiceInvoice= await _onedb.Products
                        .AnyAsync(p => productIds.Contains(p.ProductId) && p.ProductType == 2);
                    int revenueAccount = isServiceInvoice
                    ? CoaAccounts.ServiceIncome
                    : CoaAccounts.SalesIncome;
                    if (isServiceInvoice)
                    {
                        await _journalRepo.AddServiceInvoiceJournals(invoiceMaster, isCashINvoice, customer);
                    }
                    else
                    {
                        await _journalRepo.AddInvoiceJournals(invoiceMaster, isCashINvoice, customer, cost);
                    }

                   // var tesla = await _journalRepo.AddInvoiceJournals(invoiceMaster, isCashINvoice, customer, cost);
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

        public async Task<bool> CancelInvoice(Guid invoiceId, string reason)
        {
            using (var transaction = await _onedb.Database.BeginTransactionAsync())
            {
                try
                {
                    var invoice = await _onedb.InvoiceMasters
                        .Include(x => x.InvoiceDetails)
                        .FirstOrDefaultAsync(x => x.InvoiceMasterId == invoiceId);

                    if (invoice == null)
                        throw new Exception("Invoice not found.");

                    //if (invoice.IsCancelled)
                    //    throw new Exception("Invoice is already cancelled.");

                    // ---------------------------
                    // 1️⃣ Mark Invoice as Cancelled
                    // ---------------------------
                    
                    invoice.IsCancelled = true;
                    invoice.CanceledDate= DateTime.Now;
                    invoice.PaymentStatusId = (int)AppConstants.PaymentStatus.Cancelled;
                    _onedb.InvoiceMasters.Update(invoice);

                    // ---------------------------
                    // 2️⃣ Restore Inventory
                    // ---------------------------
                    foreach (var detail in invoice.InvoiceDetails)
                    {
                        // Variant stock
                        if (detail.VariantId.HasValue && detail.VariantId != Guid.Empty)
                        {
                            var branchStock = await _onedb.BranchStocks
                                .FirstOrDefaultAsync(s =>
                                    s.ProductVariantId == detail.VariantId &&
                                    s.BranchId == AppDataUtility.SessionUser.Person.Branch.BranchId);

                            if (branchStock != null)
                            {
                                branchStock.Qty += detail.Quantity;
                                _onedb.BranchStocks.Update(branchStock);
                            }
                        }

                        // Product stock (QOH)
                        var product = await _onedb.Products
                            .FirstOrDefaultAsync(p => p.ProductId == detail.ProductId);

                        if (product != null)
                        {
                            product.Qoh += detail.Quantity;
                            _onedb.Products.Update(product);
                        }
                    }

                    // ---------------------------
                    // 3️⃣ Reverse Journal Entries
                    // ---------------------------

                    var originalEntry = await _onedb.JournalEntries
                        .Include(x => x.JournalLines)
                        .FirstOrDefaultAsync(x =>
                            x.ReferenceNumber == invoice.InvoiceNo &&
                            x.SourceModule == "Sales");

                    if (originalEntry == null)
                        throw new Exception("Original journal entry not found!");

                    // Create reversal journal entry
                    var reversalEntry = new JournalEntry
                    {
                        JournalEntryId = Guid.NewGuid(),
                        EntryDate = DateTime.Now,
                        ReferenceNumber = invoice.InvoiceNo + "-REV",
                        Description = $"Reversal of Invoice {invoice.InvoiceNo} Journal Entry number: {originalEntry.JournalEntryId}",
                        BranchId = originalEntry.BranchId,
                        CreatedBy = AppDataUtility.SessionUser.Id,
                        CreatedAt = DateTime.Now,
                        SourceModule = "Sales-Reversal",
                        EntryNumber = await _journalRepo.GetNewJournalNumber(),
                    };

                    await _onedb.JournalEntries.AddAsync(reversalEntry);

                    // Reverse each journal line
                    foreach (var line in originalEntry.JournalLines)
                    {
                        await _onedb.JournalLines.AddAsync(new JournalLine
                        {
                            JournalLineId = Guid.NewGuid(),
                            JournalEntryId = reversalEntry.JournalEntryId,
                            CoaId = line.CoaId,
                            Description = "Reversal - " + line.Description,
                            Debit = line.Credit,
                            Credit = line.Debit,
                            ReferenceType = "Invoice-Reversal",
                            ReferenceId = invoice.InvoiceMasterId
                        });
                    }

                    // ---------------------------
                    // 4️⃣ Update A/R if credit invoice
                    // ---------------------------
                    if (invoice.PaymentStatusId != 1) // 1 = Cash
                    {
                        await _accountsReceivableRepository.ReverseCreditSale(invoice, reversalEntry);
                    }

                    await _onedb.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<string> SendInvoicetoFBR()
        {
            var res = await _api.PostMethodNew("FBR/SendInvoice", "", "", "");
            return "";
        }
        public async Task<InvoiceMaster> GetInvoiceById(Guid invoiceId)
        {
            try
            {
                var model = await _onedb.InvoiceMasters
                    .Include(i => i.Customer).ThenInclude(c => c.Person)
                    .Include(i => i.Employee).ThenInclude(e => e.Person)
                    .Include(i => i.PaymentMethod)
                    .Include(i => i.PaymentStatus)
                    .Include(i => i.ServingTable)
                    .Include(i => i.InvoiceDetails).ThenInclude(d => d.Product).ThenInclude(p => p.Brand)
                    .FirstOrDefaultAsync(i => i.InvoiceMasterId == invoiceId);
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<byte[]> GenerateInvoiceHTML(InvoiceMaster model)
        {
            try
            {
                string imageUrl = AppDataUtility.SystemPreferences.CompanyLogoUrl;
                string base64Logo = "";

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = await client.GetByteArrayAsync(imageUrl);
                        var base64 = Convert.ToBase64String(bytes);

                        // detect file extension (fallback: png)
                        string ext = Path.GetExtension(imageUrl)?.ToLower() switch
                        {
                            ".jpg" or ".jpeg" => "jpeg",
                            ".gif" => "gif",
                            ".bmp" => "bmp",
                            _ => "png"
                        };

                        base64Logo = $"data:image/{ext};base64,{base64}";
                    }
                }

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
                    <img width='250px' height='20px' src='{GenerateBarCode.GenerateBarcode(model.InvoiceMasterId.ToString())}' alt='QR Code' />
                </p>
                      <div class='row align-items-center text-center'>
        <!-- Logo -->
        <div class='col-3'>
          	<img src={base64Logo}' />
        </div>

        <div class='col-9'>
            <h3 style='font-size:14px; margin:0;'>
                {@AppDataUtility.SystemPreferences.CompanyName}
            </h3>
        </div>
    </div>
                        <p>{AppDataUtility.SystemPreferences.CompanyName}</p>
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

        public async Task<decimal> GetCostofGoods(List<InvoiceDetail> invoiceDetails)
        {
            try
            {
                var branchId = AppDataUtility.SessionUser.Person.Branch.BranchId;

                // Filter valid items
                var validItems = invoiceDetails
                    .Where(i => i.VariantId.HasValue && i.Quantity > 0)
                    .ToList();

                if (!validItems.Any())
                    return 0m;

                var variantIds = validItems.Select(i => i.VariantId.Value).Distinct().ToList();

                // Single DB call for all variants in this branch
                var stocks = await _onedb.BranchStocks
                    .Where(v => variantIds.Contains(v.ProductVariantId.Value) && v.BranchId == branchId)
                    .Select(v => new { v.ProductVariantId, v.Cost })
                    .ToListAsync();

                // Map variants to their cost
                var costLookup = stocks.ToDictionary(x => x.ProductVariantId, x => x.Cost ?? 0m);

                // Multiply cost * quantity
                decimal totalCost = validItems.Sum(i =>
                    costLookup.TryGetValue(i.VariantId.Value, out decimal unitCost)
                        ? unitCost * i.Quantity
                        : 0m
                );

                return totalCost;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error calculating cost of goods", ex);
            }
        }

    }
}