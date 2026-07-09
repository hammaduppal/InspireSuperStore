using System.Linq;
using Dapper;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using MarketBal.Repository.AccountingRP;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.PurchaseRP
{
    public class PurchaseRepository
    {
        private readonly ISessionService _sessionService;
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        private readonly JournalsRepository _journalRepo;
        public PurchaseRepository(IConfiguration config, OneDb oneDb, ISessionService sessionService)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _sessionService = sessionService;
            _dap = new DapperContext(_config);
            _journalRepo = new JournalsRepository(_config, oneDb, _sessionService);
        }


        //Save As Requisition

        public async Task<int> SavePurchase(PurchaseDataDto model)
        {
            var groupedItems = model.Items
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalCost = g.Sum(x => x.TotalPrice),
                    Items = g.ToList(),
                    SupplierId = model.SupplierId,
                    BranchId = model.BranchId,
                }).ToList();
            try
            {
                Guid pmId = Guid.NewGuid();
                var commonParams = CommonParamHelper.GetCommonParams(_sessionService);
                var pm = new PurchaseMaster
                {
                    Createdby = int.Parse(commonParams.CreatedBy.ToString()),
                    CreatedOn = commonParams.CreatedOn,
                    ModifiedOn = commonParams.ModifiedOn,
                    BranchId = commonParams.BranchId,
                    PurchaseDate = commonParams.CreatedOn,
                    IsActive = commonParams.IsActive,
                    GrandTotal = model.GrandTotal,
                    DiscountAmount = model.Discount,
                    PurchaseMasterId = pmId,
                    SupplierId = model.SupplierId,
                    TotalAmount = model.GrandTotal + model.Discount,
                    PurchaseNumber = PurchaseNumberGenerator.Generate(AppConstants.PurchaseType.Requisition.ToString(), model.SupplierId),
                    PurchaseType = (int)AppConstants.PurchaseType.Requisition,
                    Status = (int)AppConstants.PurchaseStatus.Draft,
                    PurchaseTypeId=model.PurchaseTypeId
                };
                var pditems = new List<PurchaseDetail>();
                foreach (var item in model.Items)
                {
                    var guid = Guid.NewGuid();
                    pditems.Add(new PurchaseDetail
                    {
                        PurchaseDetailId = guid,
                        Createdby = int.Parse(commonParams.CreatedBy.ToString()),
                        CreatedOn = commonParams.CreatedOn,
                        ModifiedOn = commonParams.ModifiedOn,
                        IsActive = commonParams.IsActive,
                        VariantId = item.VariantId,
                        Qty = item.Quantity,
                        UnitPrice = item.PurchasePrice,
                        DiscountAmount = 0,

                    });

                }
                pm.PurchaseDetails = pditems;
                await _onedb.PurchaseMasters.AddAsync(pm);
                return await _onedb.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        public async Task<int> SaveOpeningStock(PurchaseDataDto model)
        {
            try
            {



            var commonParams = CommonParamHelper.GetCommonParams(_sessionService);
            Guid pmId = Guid.NewGuid();

            // 1️⃣ Create PurchaseMaster
            var pm = new PurchaseMaster
            {
                PurchaseMasterId = pmId,
                Createdby = int.Parse(commonParams.CreatedBy.ToString()),
                CreatedOn = commonParams.CreatedOn,
                ModifiedOn = commonParams.ModifiedOn,
                BranchId = model.BranchId,
                PurchaseDate = commonParams.CreatedOn,
                IsActive = commonParams.IsActive,
                GrandTotal = model.GrandTotal,
                DiscountAmount = model.Discount,
                TotalAmount = model.GrandTotal + model.Discount,
                Status = (int)AppConstants.PurchaseStatus.Approved,
                PurchaseTypeId = model.PurchaseTypeId,
                Remarks = "Opening Stock Initialization", 
                PurchaseNumber = PurchaseNumberGenerator.Generate(AppConstants.PurchaseType.Requisition.ToString(), model.SupplierId),
                PurchaseType = (int)AppConstants.PurchaseType.OpeningStock
            };

            // 2️⃣ Add PurchaseDetails
            var pditems = model.Items.Select(item => new PurchaseDetail
            {
                PurchaseDetailId = Guid.NewGuid(),
                Createdby = int.Parse(commonParams.CreatedBy.ToString()),
                CreatedOn = commonParams.CreatedOn,
                ModifiedOn = commonParams.ModifiedOn,
                IsActive = commonParams.IsActive,
                VariantId = item.VariantId,
                Qty = item.Quantity,
                UnitPrice = item.PurchasePrice,
                DiscountAmount = 0,
                TotalPrice = item.TotalPrice
            }).ToList();

            pm.PurchaseDetails = pditems;
            await _onedb.PurchaseMasters.AddAsync(pm);
            await _onedb.SaveChangesAsync(); // Save to get IDs

            // 3️⃣ Update branch stock and product QOH
            var variantIds = model.Items.Select(x => x.VariantId).ToList();
            var variants = await _onedb.ProductVariants
                .Where(v => variantIds.Contains(v.VariantId))
                .ToListAsync();

            var branchStocks = await _onedb.BranchStocks
                .Where(bs => bs.BranchId == model.BranchId && variantIds.Contains(bs.ProductVariantId.Value))
                .ToListAsync();

            foreach (var item in model.Items)
            {
                var variant = variants.FirstOrDefault(v => v.VariantId == item.VariantId);
                if (variant == null) continue;

                decimal receivedQty = item.Quantity * (variant.QuantityPerUnit ?? 1M);

                // Update branch stock
                var branchStock = branchStocks.FirstOrDefault(bs => bs.ProductVariantId == item.VariantId);
                if (branchStock != null)
                {
                    branchStock.Qty += receivedQty;
                    branchStock.Cost = item.PurchasePrice;
                }
                else
                {
                    _onedb.BranchStocks.Add(new BranchStock
                    {
                        BranchStockId = Guid.NewGuid(),
                        BranchId = model.BranchId,
                        ProductVariantId = item.VariantId,
                        Qty = receivedQty,
                        Cost = item.PurchasePrice,
                        CreatedOn = DateTime.UtcNow
                    });
                }

                // Update product QOH
                var product = await _onedb.Products.FirstOrDefaultAsync(p => p.ProductId == variant.ProductId);
                if (product != null)
                {
                    product.Qoh = (product.Qoh ?? 0M) + receivedQty;
                    product.ModifiedOn = DateTime.UtcNow;
                }
            }

            await _onedb.SaveChangesAsync();

            // 4️⃣ Create Journal Entry (Opening Stock)
            var journalEntry = new JournalEntry
            {
                JournalEntryId = Guid.NewGuid(),
                EntryDate = DateTime.UtcNow,
                EntryNumber = await _journalRepo.GetNewJournalNumber(),
                ReferenceNumber = $"OS-{pmId.ToString().Substring(0, 8)}",
                BranchId = pm.BranchId.Value,
                Description = "Opening Stock Initialization",
                CreatedBy = pm.Createdby,
                CreatedAt = DateTime.UtcNow,
                SourceModule = "OpeningStock"
            };
            _onedb.JournalEntries.Add(journalEntry);

            // Debit Inventory
            _onedb.JournalLines.Add(new JournalLine
            {
                JournalLineId = Guid.NewGuid(),
                JournalEntryId = journalEntry.JournalEntryId,
                CoaId = AppConstants.CoaAccounts.Inventory, // Inventory account
                Debit = pm.TotalAmount ?? 0M,
                Credit = 0,
                Description = "Inventory increased by Opening Stock",
            });

            // Credit Opening Stock / Adjustment account
            _onedb.JournalLines.Add(new JournalLine
            {
                JournalLineId = Guid.NewGuid(),
                JournalEntryId = journalEntry.JournalEntryId,
                CoaId = AppConstants.CoaAccounts.OpeningStock, // Opening Stock / Capital account
                Debit = 0,
                Credit = pm.TotalAmount ?? 0M,
                Description = "Opening Stock balance",
            });

            await _onedb.SaveChangesAsync();

            return 1;
        }
              catch (Exception ex)
            {

                throw;
            }
        }

        //Get Single Requisition
        public async Task<PurchaseMasterVM> GetSingleRequisition(Guid Id, AppConstants.PurchaseType purchaseType)
        {
            var purchase = await _onedb.PurchaseMasters
                .Where(pm => pm.PurchaseMasterId == Id && pm.PurchaseType == (int)purchaseType)
                .Select(pm => new PurchaseMasterVM
                {
                    PurchaseMasterId = pm.PurchaseMasterId,
                    PurchaseNumber = pm.PurchaseNumber,
                    PurchaseType = pm.PurchaseType,
                    TotalAmount = pm.TotalAmount,
                    DiscountAmount = pm.DiscountAmount,
                    GrandTotal = pm.GrandTotal,
                    CreatedOn = pm.CreatedOn,
                    Createdby = pm.Createdby,
                    IsActive = pm.IsActive,
                    Status = pm.Status,
                    Supplier = new SupplierVM
                    {
                        SupplierId = pm.Supplier.SupplierId,
                        SupplierBusinessName = pm.Supplier.SupplierBusinessName
                    },
                    PurchaseDetails = pm.PurchaseDetails.Select(pd => new PurchaseDetailVM
                    {
                        PurchaseDetailId = pd.PurchaseDetailId,
                        Qty = pd.Qty,
                        UnitPrice = pd.UnitPrice,
                        TotalPrice = pd.TotalPrice,
                        LineTotal = pd.LineTotal,
                        VariantId = pd.VariantId,
                        ProductVariant = new ProductVariantVM
                        {
                            VariantId = Guid.Parse(pd.VariantId.ToString()),
                            BarCode = pd.Variant.BarCode,
                            Product = new ProductVM
                            {
                                ProductId = pd.Variant.Product.ProductId,
                                ProductName = pd.Variant.Product.ProductName,
                                Brand = new BrandVM
                                {
                                    BrandId = pd.Variant.Product.Brand.BrandId,
                                    BrandName = pd.Variant.Product.Brand.BrandName
                                }
                            },
                            Color = new ColorVM
                            {
                                ColorId = pd.Variant.Color.ColorId,
                                ColorName = pd.Variant.Color.ColorName
                            }
                        }
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return purchase;
        }


        //Get Purchases by Type
        public async Task<List<PurchaseMasterVM>> GetPurchaseRequisition(AppConstants.PurchaseType purchaseType)
        {
            return await _onedb.PurchaseMasters.Where(x => x.PurchaseType == (int)purchaseType).Select(p => new PurchaseMasterVM
            {
                PurchaseMasterId = p.PurchaseMasterId,
                PurchaseNumber = p.PurchaseNumber,
                PurchaseType = p.PurchaseType,
                SupplierId = p.SupplierId,
                PurchaseDate = p.PurchaseDate,
                DiscountAmount = p.DiscountAmount,
                TotalAmount = p.TotalAmount,
                GrandTotal = p.GrandTotal,
                Status = p.Status,
                Remarks = p.Remarks,
                UpdatedBy = p.UpdatedBy,
                UpdatedDate = p.UpdatedDate,
                IsActive = p.IsActive,
                CreatedOn = p.CreatedOn,
                Createdby = p.Createdby,
                ModifiedOn = p.ModifiedOn,
                BranchId = p.BranchId,
                SupplierBusinessName = p.Supplier.SupplierBusinessName
            }).ToListAsync();
        }


        public async Task<List<PurchaseTypeVM>> GetPurchaseTypes()
        {
            return await _onedb.PurchaseTypes.Select(pt => new PurchaseTypeVM
            {
                PurchaseTypeId = pt.Id,
                Name= pt.Name,
                 Code= pt.Code,
                  IsActive= pt.IsActive
            }).ToListAsync();
        }






        public async Task<int> UpdatePoQTY(PurchaseDetailVM model)
        {
            var result = await _onedb.PurchaseDetails.Where(x => x.PurchaseDetailId == model.PurchaseDetailId).FirstOrDefaultAsync();
            result.Qty = model.Qty;
            await _onedb.SaveChangesAsync();
            var purchaseMaster = await _onedb.PurchaseMasters
            .Include(x => x.PurchaseDetails)
            .FirstOrDefaultAsync(x => x.PurchaseMasterId == model.PurchaseMasterId);

            if (purchaseMaster != null)
            {
                purchaseMaster.TotalAmount = purchaseMaster.PurchaseDetails
                    .Sum(x => x.Qty * x.UnitPrice);
                purchaseMaster.GrandTotal = purchaseMaster.TotalAmount;
            }
            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> UpdatePOSTATUS(PurchaseMasterVM model)
        {
            var result = await _onedb.PurchaseMasters.Where(x => x.PurchaseMasterId == model.PurchaseMasterId).FirstOrDefaultAsync();
            result.Status = model.Status;
            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> UpdatePOType(PurchaseMasterVM model)
        {
            var result = await _onedb.PurchaseMasters.Where(x => x.PurchaseMasterId == model.PurchaseMasterId).FirstOrDefaultAsync();
            result.PurchaseType = model.PurchaseType;
            result.Status = model.Status;
            return await _onedb.SaveChangesAsync();
        }
        public async Task<int> CreateGRNNote(PurchaseMasterVM model)
        {
            // 1. Get PurchaseMaster (for branch info)
            var master = await _onedb.PurchaseMasters
                .Include(x => x.PurchaseDetails)
                .FirstOrDefaultAsync(x => x.PurchaseMasterId == model.PurchaseMasterId);

            if (master == null)
                return 0;

            var branchId = master.BranchId;

            // Load variants
            var variantIds = model.PurchaseDetails.Select(x => x.VariantId).ToList();
            var variants = await _onedb.ProductVariants
                .Where(x => variantIds.Contains(x.VariantId))
                .ToListAsync();

            // Fetch products
            var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
            var products = await _onedb.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // Load branch stocks
            var branchStocks = await _onedb.BranchStocks
                .Where(bs => bs.BranchId == branchId && variantIds.Contains(bs.ProductVariantId))
                .ToListAsync();

            // 2. Update details and stock
            foreach (var detail in model.PurchaseDetails)
            {
                var existingDetail = await _onedb.PurchaseDetails
                    .FirstOrDefaultAsync(x => x.PurchaseDetailId == detail.PurchaseDetailId);

                if (existingDetail != null)
                {
                    existingDetail.Qty = detail.Qty;
                    existingDetail.UnitPrice = detail.UnitPrice;
                    existingDetail.TotalPrice = detail.TotalPrice;
                    existingDetail.ModifiedOn = DateTime.UtcNow;
                }

                var variant = variants.FirstOrDefault(v => v.VariantId == detail.VariantId);
                var qpu = variant?.QuantityPerUnit ?? 1M;   // quantity per unit
                var receivedQty = (detail.Qty ?? 0M) * qpu;

                // Update / create branch stock
                var branchStock = branchStocks.FirstOrDefault(bs => bs.ProductVariantId == detail.VariantId);
                if (branchStock != null)
                {
                    branchStock.Qty += receivedQty;
                    branchStock.Cost = detail.UnitPrice;
                }
                else
                {
                    _onedb.BranchStocks.Add(new BranchStock
                    {
                        BranchStockId = Guid.NewGuid(),
                        BranchId = branchId,
                        ProductVariantId = detail.VariantId,
                        Qty = receivedQty,
                        Cost = detail.UnitPrice,
                        CreatedOn = DateTime.UtcNow
                    });
                }

                // Increment Product Qoh directly
                var product = products.FirstOrDefault(p => p.ProductId == variant.ProductId);
                if (product != null)
                {
                    product.Qoh = (product.Qoh ?? 0M) + receivedQty;
                    product.ModifiedOn = DateTime.UtcNow;
                }
            }

            // 3. Update PurchaseMaster
            master.TotalAmount = model.TotalAmount;
            master.DiscountAmount = model.DiscountAmount;
            master.GrandTotal = model.GrandTotal;
            master.Status = 4; // received
            master.ModifiedOn = DateTime.UtcNow;

            await _onedb.SaveChangesAsync();
            try
            {
                await _journalRepo.AddPurchasejournals(master);
            }
            catch (Exception ex)
            {

                throw;
            }

            return 1;
        }

        //    private async Task<PurchaseMasterVM> GetRequisition(Guid Id)
        //    {
        //        var query = @"
        //SELECT 
        //    -- PurchaseMaster
        //    pm.PurchaseMasterId, pm.PurchaseNumber, pm.PurchaseType, pm.TotalAmount, pm.DiscountAmount, pm.GrandTotal,
        //    s.SupplierId, s.SupplierBusinessName,

        //    -- PurchaseDetail
        //    pd.PurchaseDetailId, pd.Qty, pd.UnitPrice, pd.TotalPrice, pd.LineTotal,

        //    -- ProductVariant
        //    pv.VariantId, pv.BarCode, pv.ProductId, pv.ColorId,s.SupplierBusinessName

        //    -- Product
        //    p.ProductId, p.ProductName,

        //    -- Brand
        //    b.BrandId, b.BrandName,

        //    -- Color
        //    c.ColorId, c.ColorName

        //FROM INV.PurchaseMaster pm
        //JOIN INV.PurchaseDetail pd ON pd.PurchaseMasterId = pm.PurchaseMasterId
        //JOIN INV.ProductVariants pv ON pd.VariantId = pv.VariantId
        //JOIN INV.Products p ON pv.ProductId = p.ProductId
        //JOIN INV.Brands b ON p.BrandId = b.BrandId
        //JOIN INV.Colors c ON pv.ColorId = c.ColorId
        //JOIN HRM.Supplier s ON pm.SupplierId = s.SupplierId
        //WHERE pd.PurchaseMasterId = @PurchaseMasterId";

        //        var param = new { PurchaseMasterId = Id };

        //        using var connection = _dap.CreateConnection();
        //        var lookup = new Dictionary<Guid, PurchaseMasterVM>();

        //        await connection.QueryAsync<
        //            PurchaseMasterVM,
        //            PurchaseDetailVM,
        //            ProductVariantVM,
        //            ProductVM,
        //            BrandVM,
        //            ColorVM,
        //            PurchaseMasterVM
        //        >(
        //            query,
        //            (master, detail, variant, product, brand, color) =>
        //            {
        //                if (!lookup.TryGetValue(master.PurchaseMasterId, out var masterEntry))
        //                {
        //                    masterEntry = master;
        //                    masterEntry.PurchaseDetails = new List<PurchaseDetailVM>();
        //                    lookup[master.PurchaseMasterId] = masterEntry;
        //                }

        //                // Assign product and brand
        //                product.Brand = brand;

        //                // Assign product and color into variant
        //                variant.Product = product;
        //                variant.Color = color;

        //                // Assign variant to detail
        //                detail.ProductVariant = variant;

        //                // Add detail to master
        //                masterEntry.PurchaseDetails.Add(detail);

        //                return masterEntry;
        //            },
        //            param,
        //            splitOn: "PurchaseDetailId,VariantId,ProductId,BrandId,ColorId"
        //        );

        //        return lookup.Values.FirstOrDefault();
        //    }


    }
}
