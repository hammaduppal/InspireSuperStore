using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using static MainModels.DTOModels.AppConstants;
using static MainModels.Util.CommonParamHelper;

namespace MarketBal.Repository.AccountingRP
{
    public class JournalsRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        public JournalsRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
        }
        public async Task<int> AddInvoiceJournals(InvoiceMaster invoiceMaster, bool isCash, Customer customer, decimal cost)
        {
            try
            {
                var commonParams = CommonParamHelper.GetCommonParams();

                var journalEntry = new JournalEntry
                {
                    JournalEntryId = Guid.NewGuid(),
                    EntryDate = commonParams.CreatedOn.Value,
                    ReferenceNumber = invoiceMaster.InvoiceNo,
                    Description = "Sales to " + (customer.Person?.FirstName ?? "") + " (" + customer.CustomerCode + ")",
                    BranchId = AppDataUtility.SessionUser.Person.Branch.BranchId,
                    CreatedBy = AppDataUtility.SessionUser.Id,
                    CreatedAt = commonParams.CreatedOn.Value,
                    SourceModule = "Sales",
                    EntryNumber = invoiceMaster.InvoiceNo
                };

                await _onedb.JournalEntries.AddAsync(journalEntry);

                // -----------------------------------------------------------
                // 1️⃣  Customer Receivable OR Cash Account
                // -----------------------------------------------------------
                if (isCash)
                {
                    // Cash Sale → Debit Cash
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.Cash, // 🔸 replace with your actual COA ID for Cash
                        Description = "Cash Sale",
                        Debit = invoiceMaster.GrandTotal,
                        Credit = 0, ReferenceType="Invoice", ReferenceId=invoiceMaster.InvoiceMasterId
                    });
                }
                else
                {
                    // Credit Sale → Debit Accounts Receivable
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.AccountsReceivable, // 🔸 your AR COA ID
                        Description = "Accounts Receivable",
                        Debit = invoiceMaster.GrandTotal,
                        Credit = 0,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
                    });
                }

                // -----------------------------------------------------------
                // 2️⃣  Sales Income (Revenue)
                // -----------------------------------------------------------
                await _onedb.JournalLines.AddAsync(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = CoaAccounts.SalesRevenue, // 🔸 your Sales Income COA ID
                    Description = "Sales Revenue",
                    Debit = 0,
                    Credit = invoiceMaster.TotalAmount,
                    ReferenceType = "Invoice",
                    ReferenceId = invoiceMaster.InvoiceMasterId
                });

                // -----------------------------------------------------------
                // 3️⃣  Tax Payable (if enabled)
                // -----------------------------------------------------------
                if (AppDataUtility.SystemPreferences.EnableTax && invoiceMaster.TaxAmount > 0)
                {
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.OutputTaxPayable, // 🔸 your Output VAT COA ID
                        Description = "Output VAT Payable",
                        Debit = 0,
                        Credit = invoiceMaster.TaxAmount,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
                    });
                }

                // -----------------------------------------------------------
                // 4️⃣  COGS (Cost of Goods Sold)
                // -----------------------------------------------------------
                await _onedb.JournalLines.AddAsync(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = CoaAccounts.COGS, // 🔸 your COGS COA ID
                    Description = "Cost of Goods Sold",
                    Debit = cost,
                    Credit = 0,
                    ReferenceType = "Invoice",
                    ReferenceId = invoiceMaster.InvoiceMasterId
                });

                // -----------------------------------------------------------
                // 5️⃣  Inventory Reduction
                // -----------------------------------------------------------
                await _onedb.JournalLines.AddAsync(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = CoaAccounts.Inventory, // 🔸 your Inventory COA ID
                    Description = "Inventory Reduction",
                    Debit = 0,
                    Credit = cost,
                    ReferenceType = "Invoice",
                    ReferenceId = invoiceMaster.InvoiceMasterId
                });

                await _onedb.SaveChangesAsync();
                return 1;
            }
            catch (Exception ex)
            {
                // Optional: log ex.Message
                throw;
            }
        }


    }
}
