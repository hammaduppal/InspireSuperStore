using Dapper;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Helper;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.PurchaseRP
{
    public class PurchaseRepository 
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly DapperContext _dap;
        public PurchaseRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _dap = new DapperContext(_config);
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
                var commonParams = CommonParamHelper.GetCommonParams();
                var pm = new PurchaseMaster
                {
                    Createdby = int.Parse(CommonParamHelper.GetCommonParams().CreatedBy.ToString()),
                    CreatedOn = commonParams.CreatedOn,
                    ModifiedOn = commonParams.ModifiedOn,
                    BranchId = commonParams.BranchId,
                    PurchaseDate = commonParams.CreatedOn,
                    IsActive = commonParams.IsActive,
                    GrandTotal = model.GrandTotal ,
                    DiscountAmount = model.Discount,
                    PurchaseMasterId = pmId,
                    SupplierId = model.SupplierId,
                    TotalAmount = model.GrandTotal + model.Discount,
                    PurchaseNumber = PurchaseNumberGenerator.Generate(AppConstants.PurchaseType.Requisition.ToString(), model.SupplierId),
                    PurchaseType = (int)AppConstants.PurchaseType.Requisition,
                    Status = (int)AppConstants.PurchaseStatus.Draft
                };
                var pditems = new List<PurchaseDetail>();
                foreach (var item in model.Items)
                {
                    var guid = Guid.NewGuid();
                    pditems.Add(new PurchaseDetail
                    {
                        PurchaseDetailId = guid,
                        Createdby = int.Parse(CommonParamHelper.GetCommonParams().CreatedBy.ToString()),
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
       //Get Single Requisition
       public async Task<PurchaseMasterVM> GetSingleRequisition(Guid Id,AppConstants.PurchaseType purchaseType)
        {
            var purchase = await _onedb.PurchaseMasters
                .Where(pm => pm.PurchaseMasterId == Id&& pm.PurchaseType==(int)purchaseType)
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
                        VariantId=pd.VariantId,
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
                SupplierBusinessName=p.Supplier.SupplierBusinessName
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
            var result =await _onedb.PurchaseMasters.Where(x => x.PurchaseMasterId == model.PurchaseMasterId).FirstOrDefaultAsync();
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
            // Load all variants involved
            var variantIds = model.PurchaseDetails.Select(x => x.VariantId).ToList();
            var variants = await _onedb.ProductVariants
                .Where(x => variantIds.Contains(x.VariantId))
                .ToListAsync();

            // Fetch their corresponding products
            var productIds = variants.Select(v => v.ProductId).Distinct().ToList();
            var products = await _onedb.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToListAsync();

            // 1. Update PurchaseDetail received quantity
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

                // 2. Update Variant Quantity
                var variant = variants.FirstOrDefault(x => x.VariantId == detail.VariantId);
                if (variant != null)
                {
                    var qpu = variant.QuantityPerUnit??1;
                    variant.QoH = (variant.QoH ?? 0) + ((detail.Qty * qpu) ?? 0); variant.ModifiedOn = DateTime.UtcNow;
                    variant.Cost = detail.UnitPrice;
                }
            }

            // 3. Update Product Quantity (sum of its variants)
            foreach (var product in products)
            {
                var productVariants = variants.Where(v => v.ProductId == product.ProductId).ToList();
                product.Qoh = productVariants.Sum(v => v.QoH);
                product.ModifiedOn = DateTime.UtcNow;
            }

            // 4. Update PurchaseMaster totals if needed
            var master = await _onedb.PurchaseMasters
                .FirstOrDefaultAsync(x => x.PurchaseMasterId == model.PurchaseMasterId);

            if (master != null)
            {
                master.TotalAmount = model.TotalAmount;
                master.DiscountAmount = model.DiscountAmount;
                master.GrandTotal = model.GrandTotal;
                master.Status = 4; // Mark as received
                master.ModifiedOn = DateTime.UtcNow;
            }

            await _onedb.SaveChangesAsync();
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
