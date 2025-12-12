using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;
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
        private readonly AccountsReceivableRepository _accountsReceivableRepository;
        private readonly AccountPayableRP _accountPayable;
        public JournalsRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
            _accountsReceivableRepository = new AccountsReceivableRepository(_config, _onedb);
            _accountPayable = new AccountPayableRP(_config, _onedb);
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
                    EntryNumber = await GetNewJournalNumber()
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
                        CoaId = CoaAccounts.CashOnHand, // 🔸 replace with your actual COA ID for Cash
                        Description = "Cash Sale",
                        Debit = invoiceMaster.GrandTotal,
                        Credit = 0,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
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
                    await _accountsReceivableRepository.AddCreditSale(invoiceMaster, customer, journalEntry);
                }

                // -----------------------------------------------------------
                // 2️⃣  Sales Income (Revenue)
                // -----------------------------------------------------------
                await _onedb.JournalLines.AddAsync(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = CoaAccounts.SalesIncome, // 🔸 your Sales Income COA ID
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
                        CoaId = CoaAccounts.TaxesPayable, // 🔸 your Output VAT COA ID
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
            catch (Exception)
            {
                // Optional: log ex.Message
                throw;
            }
        }

        public async Task<int> AddServiceInvoiceJournals(InvoiceMaster invoiceMaster, bool isCash, Customer customer)
        {
            try
            {
                var commonParams = CommonParamHelper.GetCommonParams();

                var journalEntry = new JournalEntry
                {
                    JournalEntryId = Guid.NewGuid(),
                    EntryDate = commonParams.CreatedOn.Value,
                    ReferenceNumber = invoiceMaster.InvoiceNo,
                    Description = "Service Sale to " + (customer.Person?.FirstName ?? "") + " (" + customer.CustomerCode + ")",
                    BranchId = AppDataUtility.SessionUser.Person.Branch.BranchId,
                    CreatedBy = AppDataUtility.SessionUser.Id,
                    CreatedAt = commonParams.CreatedOn.Value,
                    SourceModule = "Sales",
                    EntryNumber = await GetNewJournalNumber()
                };

                await _onedb.JournalEntries.AddAsync(journalEntry);

                // -----------------------------------------------------------
                // 1️⃣  DR Cash / Accounts Receivable
                // -----------------------------------------------------------
                if (isCash)
                {
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.CashOnHand,
                        Description = "Cash Sale (Service)",
                        Debit = invoiceMaster.GrandTotal,
                        Credit = 0,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
                    });
                }
                else
                {
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.AccountsReceivable,
                        Description = "Accounts Receivable (Service)",
                        Debit = invoiceMaster.GrandTotal,
                        Credit = 0,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
                    });

                    // Your existing credit sale function
                    await _accountsReceivableRepository.AddCreditSale(invoiceMaster, customer, journalEntry);
                }

                // -----------------------------------------------------------
                // 2️⃣  CR Service Income (NOT Sales Income)
                // -----------------------------------------------------------
                await _onedb.JournalLines.AddAsync(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = CoaAccounts.ServiceIncome,  // <-- 4020 / ID: 23
                    Description = "Service Income",
                    Debit = 0,
                    Credit = invoiceMaster.TotalAmount,
                    ReferenceType = "Invoice",
                    ReferenceId = invoiceMaster.InvoiceMasterId
                });

                // -----------------------------------------------------------
                // 3️⃣  Output Tax (if enabled)
                // -----------------------------------------------------------
                if (AppDataUtility.SystemPreferences.EnableTax && invoiceMaster.TaxAmount > 0)
                {
                    await _onedb.JournalLines.AddAsync(new JournalLine
                    {
                        JournalLineId = Guid.NewGuid(),
                        JournalEntryId = journalEntry.JournalEntryId,
                        CoaId = CoaAccounts.TaxesPayable, // SAME as product invoices
                        Description = "Output VAT on Service",
                        Debit = 0,
                        Credit = invoiceMaster.TaxAmount,
                        ReferenceType = "Invoice",
                        ReferenceId = invoiceMaster.InvoiceMasterId
                    });
                }

              
                await _onedb.SaveChangesAsync();
                return 1;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> AddPurchasejournals(PurchaseMaster master)
        {
            var journalEntry = new JournalEntry
            {
                JournalEntryId = Guid.NewGuid(),
                EntryDate = DateTime.UtcNow,
                EntryNumber = await GetNewJournalNumber(),
                ReferenceNumber = master.PurchaseNumber,
                BranchId = master.BranchId.Value,
                Description = $"GRN posted for Purchase #{master.PurchaseNumber}",
                CreatedBy = master.Createdby,
                CreatedAt = DateTime.UtcNow,
                SourceModule = "Purchase"
            };
            _onedb.JournalEntries.Add(journalEntry);

            _onedb.JournalLines.Add(new JournalLine
            {
                JournalLineId = Guid.NewGuid(),
                JournalEntryId = journalEntry.JournalEntryId,
                CoaId = 7, // from ChartOfAccounts (Inventory)
                Debit = master.TotalAmount ?? 0M,
                Credit = 0,
                Description = "Inventory increased by GRN"
            });
            if (AppDataUtility.SystemPreferences.EnableTax && master.TaxAmount > 0)
            {
                _onedb.JournalLines.Add(new JournalLine
                {
                    JournalLineId = Guid.NewGuid(),
                    JournalEntryId = journalEntry.JournalEntryId,
                    CoaId = 39, // Purchase Tax Account
                    Debit = master.TaxAmount ?? 0M,
                    Credit = 0,
                    Description = "Input Tax on Purchase"
                });
            }
            _onedb.JournalLines.Add(new JournalLine
            {
                JournalLineId = Guid.NewGuid(),
                JournalEntryId = journalEntry.JournalEntryId,
                CoaId = 13, // Supplier's account in COA
                Debit = 0,
                Credit = master.GrandTotal ?? 0M,
                Description = "Accounts Payable for Purchase"
            });
            _onedb.SaveChanges();
            await _accountPayable.AddCreditPurchase(master, journalEntry);

            return 1;
        }

        public async Task<List<JournalEntryVM>> GetAllJournalEntries()
        {
            var data = await _onedb.JournalEntries
                .Include(e => e.JournalLines)
                .ThenInclude(l => l.Coa) // Assuming JournalLine has navigation property 'Coa'
                .Select(e => new JournalEntryVM
                {
                    JournalEntryId = e.JournalEntryId,
                    EntryNumber = e.EntryNumber,
                    EntryDate = e.EntryDate,
                    ReferenceNumber = e.ReferenceNumber,
                    Description = e.Description,
                    SourceModule = e.SourceModule,
                    //BranchName = e.Branch != null ? e.Branch.BranchName : "—",
                    CreatedBy = e.CreatedBy,
                    TotalDebit = e.JournalLines.Sum(l => l.Debit),
                    TotalCredit = e.JournalLines.Sum(l => l.Credit),

                    JournalLines = e.JournalLines.Select(l => new JournalLineVM
                    {
                        JournalLineId = l.JournalLineId,
                        AccountName = l.Coa.AccountName,
                        AccountCode = l.Coa.AccountCode,
                        Description = l.Description,
                        Debit = l.Debit,
                        Credit = l.Credit
                    }).ToList()
                })
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();

            return data;
        }











        public async Task<string> GetNewJournalNumber()
        {
            var lastJournal = await _onedb.JournalEntries
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefaultAsync();
            var invoicePrefix = "JV";
            string newJournalEntry = $"{invoicePrefix}-001";

            if (lastJournal != null && !string.IsNullOrEmpty(lastJournal.EntryNumber))
            {
                var lastNo = lastJournal.EntryNumber.Split('-')[1];
                if (int.TryParse(lastNo, out int number))
                {
                    newJournalEntry = $"{invoicePrefix}-{(number + 1).ToString("D3")}";
                }
            }
            return newJournalEntry;
        }

    }
}
