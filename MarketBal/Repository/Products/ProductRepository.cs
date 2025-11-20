using Azure.Core;
using Dapper;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Azure.Core.HttpHeader;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MarketBal.Repository.Products
{
    public class ProductRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb context;
        string baseAPIURL;
        private readonly MemoryStream memoryStream;
        public ProductRepository(IConfiguration config, OneDb context)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            baseAPIURL = _config.GetValue<string>("SystemSettings:ContentAPIUrl");
            memoryStream = new MemoryStream();
            this.context = context;
        }

        //public async Task<object> GetProducts(DataTableRequest request)
        //{

        //    string tableName = "INV.Products";
        //    var columnMap = new List<string>
        //        {
        //            "ProductId", "ProductName", "ProductDescription", "QOH","SubCategoryId", "BusinessStoreId", "CreatedOn", "CreatedBy","ModifiedOn", "IsActive", "IsDeleted"
        //        };

        //    var queries = ParamQueries.BuildDataTableQuery(tableName, "ProductName", request, columnMap);
        //    int totalRecords = await _db.ExecuteQuery<int>(queries.TotalRecordsQuery);
        //    int filteredRecords = await _db.ExecuteQuery<int>(queries.FilteredRecordsQuery);
        //    var data = await _db.ExecuteQueryList<ProductVM>(queries.DataQuery);
        //    return new
        //    {
        //        draw = request.Draw,
        //        recordsTotal = totalRecords,
        //        recordsFiltered = filteredRecords,
        //        data = data.ToList()
        //    };
        //}
        public async Task<object> GetProducts(DataTableRequest request)
        {

            var columnMap = new List<string>
                {
                    "ProductId", "ProductName", "ProductDescription", "QOH","SubCategoryId", "BusinessStoreId", "CreatedOn", "CreatedBy","ModifiedOn", "IsActive", "IsDeleted"
                };
            string sortColumn = columnMap[request.Order[0].Column];
            string sortDirection = request.Order[0].Dir.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

            int skip = request.Start;
            int pageSize = request.Length;
            string searchValue = request.Search?.Value;

            // Base WHERE clause
            var whereClauses = new List<string>
    {
        "(IsDeleted = 0 OR IsDeleted IS NULL)",
        "OrganizationId = @OrganizationId"
    };

            var parameters = new DynamicParameters();
            parameters.Add("OrganizationId", AppDataUtility.SessionUser.Person.Branch.Organization.OrganizationId);

            // Optional Search filter
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                whereClauses.Add(@"(ProductName LIKE @Search OR ProductDescription LIKE @Search)");
                parameters.Add("Search", $"%{searchValue}%");
            }

            string whereSql = "WHERE " + string.Join(" AND ", whereClauses);

            string countQuery = $@"
        SELECT COUNT(*) 
        FROM INV.Products
        {whereSql};
    ";

            string dataQuery = $@"
        SELECT * 
        FROM INV.Products 

        {whereSql}
        ORDER BY {sortColumn} {sortDirection}
        OFFSET @Skip ROWS FETCH NEXT @PageSize ROWS ONLY;
    
            ";

            parameters.Add("Skip", skip);
            parameters.Add("PageSize", pageSize);

            int totalRecords = await _db.ExecuteQuery<int>(countQuery, parameters);
            var data = await _db.GetDataListWithQueryAndParam<ProductVM>(dataQuery, parameters);

            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords, // same as total unless advanced filtering
                data = data.ToList()
            };
        }

        public async Task<bool> ActiveUnActiveProduct(Guid Id, int IsActive)
        {
            string query = $"UPDATE INV.Products SET IsActive={IsActive} WHERE ProductId='{Id}'";
            var result = await _db.ExecuteQuery<int>(query);

            return true;
        }


        public async Task<Guid> AddProduct(ProductVM product)
        {
            string ProductSlug = HelperClass.CreateSlug(product.ProductName);
            string query = @"
        DECLARE @NewProductId UNIQUEIDENTIFIER = NEWID();

        INSERT INTO INV.Products (
            ProductId, ProductName, ProductDescription, SubCategoryId, BranchId,BrandModelId,
            CreatedOn, Createdby, ModifiedOn, IsActive, IsDeleted, ProductSlug, UOMId,BrandId,OrganizationId
        )
        VALUES (
            @NewProductId, @ProductName, @ProductDescription, @SubCategoryId, @BranchId,@BrandModelId,
            @CreatedOn, @CreatedBy, @ModifiedOn, 1, 0, @ProductSlug, @UOMId,@BrandId,@OrganizationId
        );

        SELECT @NewProductId;
    ";
            var commonParams = CommonParamHelper.GetCommonParams();
            ProductSlug = $"{ProductSlug}&{commonParams.BranchId.ToString()}";

            var param = new
            {
                product.ProductName,
                product.ProductDescription,
                product.SubCategoryId,
                commonParams.BranchId,
                product.BrandModelId,
                commonParams.CreatedOn,
                commonParams.CreatedBy,
                commonParams.ModifiedOn,
                ProductSlug,
                product.UOMId,
                product.BrandId,
                commonParams.OrganizationId
            };
            var data = await _db.ExecuteQuery<Guid>(query, param);

            return data;
        }

        public async Task<ProductVM> GetProduct(Guid ProductId)
        {
            string query = $@"select p.ProductId, bm.BrandModelId, bm.ModelName, p.ProductName,uom.UOMName,p.UOMId,p.ProductDescription, sc.SubCategoryId,sc.SubCategoryName,c.CategoryName, d.DepartmentName,p.ProductSlug, ppimg.ImageUrl 
    ,b.BrandId,b.BrandName
from Inv.Products p 
LEFT JOIN INV.SubCategory sc on p.SubCategoryId = sc.SubCategoryId 
LEFT JOIN INV.Categories c on sc.CategoryId =c.CategoryId 
LEFT JOIN Inv.Departments d on c.DepartmentId=d.DepartmentId 
LEFT JOIN INv.BrandModels bm on p.BrandModelId = bm.BrandModelId
Left JOIN INV.Brands b on p.BrandId = b.BrandId
 LEFT JOIN INV.UOM uom on p.UOMId = uom.UOMId
LEFT JOIN Inv.ProductImages ppimg on p.Productid = ppimg.ProductId and ppimg.IsDefault = 1
                where P.ProductId = @ProductId";
            var param = new
            {
                ProductId
            };
            var result = await _db.GetSingleItemDatatWithQueryAndParam<ProductVM>(query, param);
            return result;

        }
        public async Task<int> UpdateDescriptionSection(ProductVM vm)
        {
            if (vm.BrandId == null || vm.SubCategoryId == null || vm.BrandId == null)
            {
                return -1;
            }
            string query = @"
                Update Inv.Products Set
                ProductName =@ProductName, 
                ProductDescription = @ProductDescription ,
                SubCategoryId = @SubCategoryId ,
                BrandId=@BrandId,
                UOMId=@UOMId,
                BrandModelId=@BrandModelId
                where ProductId=@ProductId
            select 1
                "
            ;

            var param = new
            {

                vm.ProductName,
                vm.ProductDescription,
                vm.SubCategoryId,
                vm.BrandId,
                vm.UOMId,
                vm.ProductId,
                vm.BrandModelId,
            };


            var result = await _db.ExecuteQuery<int>(query, param);
            return result;
        }
        public async Task<List<ProductImageVM>> GetProductImages(Guid ProductId)
        {
            string query = $@"select * from INV.ProductImages
                where ProductId = @ProductId and IsActive = 1";
            var param = new
            {
                ProductId
            };
            var result = await _db.GetDataListWithQueryAndParam<ProductImageVM>(query, param);
            return result.ToList();

        }
        public async Task<Guid> SaveProductImage(Guid ProductId, string ImageUrl)
        {
            string query = $@"

DECLARE @NewProductId UNIQUEIDENTIFIER = NEWID();
INSERT INTO INV.ProductImages(ProductImageId,ImageUrl,ProductId,IsDeleted, BranchId, IsActive, CreatedBy, CreatedOn, ModifiedOn)
                            Values(NEWID(),@ImageUrl,@ProductId, @IsDeleted, @BranchId, @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn) 

                            select @NewProductId
                            ";
            var commonParams = CommonParamHelper.GetCommonParams();

            var param = new
            {
                ImageUrl,
                ProductId,
                commonParams.IsDeleted,
                commonParams.BranchId,
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,


            };
            var result = await _db.ExecuteQuery<Guid>(query, param);
            return result;

        }
        public async Task<Guid> SetProductDefaultImage(Guid ProductImageId, Guid ProductId)
        {
            string query = $@"
Update Inv.ProductImages set IsDefault = 0 where ProductId=@ProductId
update Inv.ProductImages set IsDefault = 1 where ProductImageId = @ProductImageId

                          select @ProductImageId
                            ";
            //var commonParams = CommonParamHelper.GetCommonParams();

            var param = new
            {
                ProductImageId,
                ProductId

            };
            var result = await _db.ExecuteQuery<Guid>(query, param);
            return result;

        }

        public async Task<List<ProductVariantVM>> GetProductVariants(Guid ProductId)
        {
            string query = @"
SELECT 
    p.ProductId,
    p.ProductName,
    p.ProductDescription,
    p.ProductSlug,
    c.ColorName,
    m.MaterialName,
    s.SizeName,
    uom.UOMName,
    uoms.SubUOMName,
    bs.SalePrice as SalesPrice,
    bs.RetailPrice,
    bs.PromotionPrice,
    bs.Cost,
    bs.Qty,
    bs.BranchId,
    pv.VariantId,
    pv.QuantityPerUnit,
pv.MinQty, pv.MaxQty,
pv.IsSerial, pv.PriceFormat,pv.BarCode,
    pv.VariantImageId,
    b.BrandName
FROM inv.ProductVariants pv
JOIN Inv.Products p ON pv.ProductId = p.ProductId
JOIN Inv.Colors c ON pv.ColorId = c.ColorId
JOIN INV.Material m ON pv.MaterialId = m.MaterialId
JOIN INV.Sizes s ON pv.SizeId = s.SizeId
JOIN INV.UOM uom ON p.UOMId = uom.UOMId
JOIN INV.UOMSub uoms ON pv.SubUOMId = uoms.SubUOMId
JOIN INV.Brands b ON p.BrandId = b.BrandId
JOIN INV.BranchStock bs ON pv.VariantId = bs.ProductVariantId
WHERE pv.ProductId = @ProductId  AND bs.BranchId = @BranchId
  AND pv.IsActive = 1 
  AND pv.IsDeleted = 0
  AND bs.IsActive = 1
  AND bs.IsDeleted = 0";

            var param = new
            {
                ProductId,
                AppDataUtility.SessionUser.Person.Branch.BranchId
            };
            var result = await _db.GetDataListWithQueryAndParam<ProductVariantVM>(query, param);
            foreach (var variant in result)
            {
                var branchParam = new { VariantId = variant.VariantId };
                var branches = await _db.GetDataListWithQueryAndParam<BranchStockVM>(
                    @"SELECT bs.BranchStockId, b.BranchId, b.BranchName 
          FROM INV.BranchStock bs
          JOIN Business.Branches b ON bs.BranchId = b.BranchId
          WHERE bs.ProductVariantId = @VariantId
            AND bs.IsActive = 1
            AND bs.IsDeleted = 0",
                    branchParam
                );

                variant.BranchStock = branches.ToList(); // Add a property in your ProductVariantVM: List<BranchVM> AllowedBranches
            }
            foreach (var variant in result)
            {
                var branchParam = new { VariantId = variant.VariantId };
                var branches = await _db.GetDataListWithQueryAndParam<BranchVM>(
                    @"SELECT b.BranchId, b.BranchName 
          FROM INV.BranchStock bs
          JOIN Business.Branches b ON bs.BranchId = b.BranchId
          WHERE bs.ProductVariantId = @VariantId
            AND bs.IsActive = 1
            AND bs.IsDeleted = 0",
                    branchParam
                );

                variant.AllowedBranches = branches.ToList(); // Add a property in your ProductVariantVM: List<BranchVM> AllowedBranches
            }

            return result.ToList();
        }

        public async Task<int> AddProductVariantBranch(ProductVariantVM model)
        {
            if (model.IsActive == false)
            {
                string deactivateQuery = @"
            UPDATE INV.BranchStock 
            SET IsActive = 0 
            WHERE BranchId = @BranchId AND ProductVariantId = @ProductVariantId;
            SELECT 1;
        ";

                var deactivateParams = new
                {
                    BranchId = model.BranchId,
                    ProductVariantId = model.VariantId
                };

                var result = await _db.ExecuteQuery<int>(deactivateQuery, deactivateParams);
                return result;
            }

            if (model.BranchId == Guid.Empty)
                return 0;

            var masterBranchId = AppDataUtility.SessionUser.Person.Branch.BranchId;

            // Skip if the selected branch is master
            if (model.BranchId == masterBranchId)
                return 0;

            // Get master branch stock for the variant
            var masterStock = await _db.ExecuteQuery<BranchStock>(
                @"SELECT * FROM INV.BranchStock 
          WHERE ProductVariantId = @VariantId AND BranchId = @MasterBranchId AND IsDeleted = 0",
                new { VariantId = model.VariantId, MasterBranchId = masterBranchId }
            );

            if (masterStock == null)
                return 0; // nothing to copy

            // Check if branch stock already exists
            var existingStockCount = await _db.GetDataListWithQueryAndParam<BranchStockVM>(
                @"SELECT * FROM INV.BranchStock 
          WHERE ProductVariantId = @VariantId AND BranchId = @BranchId AND IsDeleted = 0",
                new { VariantId = model.VariantId, BranchId = model.BranchId }
            );

            if (existingStockCount.ToList().Count > 0)
            {
                // Update existing record with latest master stock values
                await _db.ExecuteInsertQueryandParam(
                    @"UPDATE INV.BranchStock
              SET SalePrice = @SalePrice,
                  RetailPrice = @RetailPrice,
                  PromotionPrice = @PromotionPrice,
                  Cost = @Cost,
              
                  IsActive = 1
              WHERE ProductVariantId = @VariantId AND BranchId = @BranchId",
                    new
                    {
                        VariantId = model.VariantId,
                        BranchId = model.BranchId,
                        SalePrice = masterStock.SalePrice,
                        RetailPrice = masterStock.RetailPrice,
                        PromotionPrice = masterStock.PromotionPrice,
                        Cost = masterStock.Cost,

                    }
                );
                return 2; // existing record updated
            }
            else
            {
                // Insert new BranchStock by copying master stock
                var branchStockId = Guid.NewGuid();
                await _db.ExecuteInsertQueryandParam(
                    @"INSERT INTO INV.BranchStock
              (BranchStockId, ProductVariantId, BranchId, SalePrice, RetailPrice, PromotionPrice, StaffPrice, Cost, Qty, IsActive, IsDeleted, CreatedOn, CreatedBy)
              VALUES
              (@BranchStockId, @VariantId, @BranchId, @SalePrice, @RetailPrice, @PromotionPrice, @StaffPrice, @Cost, @Qty, 1, 0, GETDATE(), @CreatedBy)",
                    new
                    {
                        BranchStockId = branchStockId,
                        VariantId = model.VariantId,
                        BranchId = model.BranchId,
                        SalePrice = masterStock.SalePrice,
                        RetailPrice = masterStock.RetailPrice,
                        PromotionPrice = masterStock.PromotionPrice,
                        StaffPrice = masterStock.StaffPrice,
                        Cost = masterStock.Cost,
                        Qty = masterStock.Qty,
                        CreatedBy = AppDataUtility.SessionUser.Person.Id
                    }
                );

                return 1; // one branch added
            }
        }


        public async Task<ProductVariantVM> GetProductVariant(string BarCode)
        {
            var branchId = AppDataUtility.SessionUser.Person.Branch.BranchId; // Logged-in branch

            string query = @"
        SELECT 
            p.ProductId,
            p.ProductName,
            p.ProductDescription,
            p.ProductSlug,
            c.ColorName,
            m.MaterialName,
            s.SizeName,
            uom.UOMName,
            uoms.SubUOMName,
            pv.QuantityPerUnit,
            pv.VariantImageId,
            b.BrandName,
ts.Rate as TaxRate,
pv.BarCode,
pv.PriceFormat,
            pv.VariantId,
            bs.BranchStockId,
            bs.SalePrice,
            bs.RetailPrice,
            bs.PromotionPrice,
            bs.StaffPrice,
            bs.Cost,
            bs.Qty,
            bs.IsActive AS BranchIsActive,
            bs.IsDeleted AS BranchIsDeleted
        FROM inv.ProductVariants pv
        JOIN Inv.Products p ON pv.ProductId = p.ProductId
JOIN System.TaxSlabs ts on pv.TaxSlabId = ts.TaxSlabId
        JOIN Inv.Colors c ON pv.ColorId = c.ColorId
        JOIN Inv.Material m ON pv.MaterialId = m.MaterialId
        JOIN Inv.Sizes s ON pv.SizeId = s.SizeId
        JOIN Inv.UOM uom ON p.UOMId = uom.UOMId
        JOIN Inv.UOMSub uoms ON pv.SubUOMId = uoms.SubUOMId
        JOIN Inv.Brands b ON p.BrandId = b.BrandId
        JOIN INV.BranchStock bs 
            ON pv.VariantId = bs.ProductVariantId 
            AND bs.BranchId = @BranchId
            AND bs.IsActive = 1 
            AND bs.IsDeleted = 0
        WHERE pv.BarCode = @BarCode 
          AND pv.IsActive = 1 
          AND pv.IsDeleted = 0";

            var param = new
            {
                BarCode,
                BranchId = branchId
            };

            var result = await _db.GetSingleItemDatatWithQueryAndParam<ProductVariantVM>(query, param);

            if (result != null)
            {
                switch (result.PriceFormat)
                {
                    case (int)AppConstants.EnumPriceFormat.RetailPrice:
                        result.CurrentPrice = result.RetailPrice;
                        break;
                    case (int)AppConstants.EnumPriceFormat.SalesPrice:
                        result.CurrentPrice = result.SalePrice;
                        break;
                    case (int)AppConstants.EnumPriceFormat.PromotionPrice:
                        result.CurrentPrice = result.PromotionPrice;
                        break;
                }
            }

            return result;
        }

        public async Task<List<ProductVariantVM>> SearchProducts(ProductSearchVM model)
        {
            //        string query = $@"
            //SELECT p.ProductId,p.ProductName,p.ProductDescription,p.ProductSlug,c.ColorName,m.MaterialName,s.SizeName,uom.UOMName,uoms.SubUOMName,pv.QuantityPerUnit,pv.VariantImageId,b.BrandName, *
            //FROM Inv.ProductVariants pv 
            //JOIN Inv.Products p ON pv.ProductId = p.ProductId
            //JOIN Inv.Colors c ON pv.ColorId = c.ColorId
            //JOIN Inv.Material m ON pv.MaterialId = m.MaterialId
            //JOIN Inv.Sizes s ON pv.SizeId = s.SizeId
            //JOIN Inv.UOM uom ON p.UOMId = uom.UOMId
            //JOIN Inv.UOMSub uoms ON pv.SubUOMId = uoms.SubUOMId
            //JOIN Inv.Brands b ON p.BrandId = b.BrandId
            //WHERE 
            //    pv.IsActive = 1 AND pv.IsDeleted = 0
            //    /*** DYNAMIC CONDITIONS WILL BE INJECTED HERE ***/ 
            //ORDER BY p.ProductName";
            string query = $@"
SELECT 
    p.ProductId,
    p.ProductName,
    p.ProductDescription,
    p.ProductSlug,
    c.ColorName,
    m.MaterialName,
    s.SizeName,
    uom.UOMName,
    uoms.SubUOMName,
pv.BarCode,

    pv.QuantityPerUnit,
    pv.VariantImageId,
    b.BrandName,
    pv.VariantId,
    bs.BranchStockId,
    bs.SalePrice,
    bs.RetailPrice,
    bs.PromotionPrice,
    bs.StaffPrice,
    bs.Cost,
    bs.Qty,
ts.Rate as TaxRate,
    pv.PriceFormat
FROM Inv.ProductVariants pv 
JOIN Inv.Products p ON pv.ProductId = p.ProductId
JOIN System.TaxSlabs ts on pv.TaxSlabId = ts.TaxSlabId
JOIN Inv.Colors c ON pv.ColorId = c.ColorId
JOIN Inv.Material m ON pv.MaterialId = m.MaterialId
JOIN Inv.Sizes s ON pv.SizeId = s.SizeId
JOIN Inv.UOM uom ON p.UOMId = uom.UOMId
JOIN Inv.UOMSub uoms ON pv.SubUOMId = uoms.SubUOMId
JOIN Inv.Brands b ON p.BrandId = b.BrandId
JOIN Inv.BranchStock bs ON pv.VariantId = bs.ProductVariantId AND bs.BranchId = @BranchId
WHERE 
    pv.IsActive = 1 AND pv.IsDeleted = 0
    AND bs.IsActive = 1 AND bs.IsDeleted = 0
    
ORDER BY p.ProductName";


            var whereConditions = new List<string>();
            var param = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(model.SearchParams))
            {
                var terms = model.SearchParams.Split('%');

                // Case: Structured Search
                if (terms.Length >= 1)
                {
                    whereConditions.Add("LOWER(p.ProductName) LIKE '%' + LOWER(@nameTerm) + '%'");
                    param.Add("nameTerm", terms[0]);
                }
                if (terms.Length >= 2)
                {
                    whereConditions.Add("LOWER(s.SizeName) LIKE '%' + LOWER(@sizeTerm) + '%'");
                    param.Add("sizeTerm", terms[1]);
                }
                if (terms.Length >= 3)
                {
                    whereConditions.Add("CAST(bs.RetailPrice AS VARCHAR) LIKE '%' + @priceTerm + '%'");
                    param.Add("priceTerm", terms[2]);
                }
                param.Add("BranchId", AppDataUtility.SessionUser.Person.Branch.BranchId);
                // Fallback fuzzy search if only one term (no `%`)
                if (terms.Length == 1)
                {
                    whereConditions.Clear();
                    string fallback = terms[0];
                    param.Add("searchTerm", fallback);

                    whereConditions.Add(@"
                (
                    LOWER(p.ProductName) LIKE '%' + LOWER(@searchTerm) + '%' OR
                    LOWER(s.SizeName) LIKE '%' + LOWER(@searchTerm) + '%' OR
                    LOWER(c.ColorName) LIKE '%' + LOWER(@searchTerm) + '%' OR
                    LOWER(m.MaterialName) LIKE '%' + LOWER(@searchTerm) + '%' OR
                    LOWER(pv.BarCode) LIKE '%' + LOWER(@searchTerm) + '%' OR
                    CAST(bs.RetailPrice AS VARCHAR) LIKE '%' + @searchTerm + '%'
                )");
                }
            }

            // Inject dynamic WHERE
            string whereClause = whereConditions.Any()
                ? " AND " + string.Join(" AND ", whereConditions)
                : "";

            query = query.Replace("/*** DYNAMIC CONDITIONS WILL BE INJECTED HERE ***/", whereClause);

            var result = await _db.GetDataListWithQueryAndParam<ProductVariantVM>(query, param);
            return UpdateCurrentPrice(result.ToList());
        }

        public class SearchTopProductResult
        {
            public List<ProductVariantVM> TopProducts { get; set; } = new();
            public List<ProductVariantVM> AllProducts { get; set; } = new();
        }
        public async Task<SearchTopProductResult> ProductsBySubCategories(Guid subCategoryId, bool isPaginated = false, int page = 1, int pageSize = 20)
        {
            var result = new SearchTopProductResult();
            var branchId = AppDataUtility.SessionUser.Person.Branch.BranchId;
            var query =
         from pv in context.ProductVariants
             // make the types match: pv.VariantId (Guid) -> (Guid?) so it equals bs.ProductVariantId (Guid?)
         join bs in context.BranchStocks
             on (Guid?)pv.VariantId equals bs.ProductVariantId into bsJoin
         // pick only the branch we want (left join semantics)
         from branchStock in bsJoin.Where(b => b.BranchId == branchId).DefaultIfEmpty()
         where pv.Product.SubCategoryId == subCategoryId
         select new ProductVariantVM
         {
             VariantId = pv.VariantId,
             MaterialId = pv.MaterialId,
             ColorId = pv.ColorId,
             SizeId = pv.SizeId,
             ProductId = pv.ProductId,

             // If branchStock is null use sensible fallbacks
             QoH = branchStock != null ? branchStock.Qty : 0M,
             Cost = branchStock != null ? branchStock.Cost : 0M,
             SalePrice = branchStock != null ? branchStock.SalePrice : 0M,
             PromotionPrice = branchStock != null ? branchStock.PromotionPrice : 0M,
             RetailPrice = branchStock != null ? branchStock.RetailPrice : 0M,

             BarCode = pv.BarCode,
             MinQty = pv.MinQty,
             MaxQty = pv.MaxQty,
             TaxRate = pv.TaxSlab.Rate,
             LastPurchase = pv.LastPurchase,
             CreatedOn = pv.CreatedOn,
             Createdby = pv.Createdby,
             ModifiedOn = pv.ModifiedOn,
             IsActive = pv.IsActive,
             IsDeleted = pv.IsDeleted,
             BranchId = branchId,
             VariantImageId = pv.VariantImageId,

             ProductName = pv.Product.ProductName,
             ProductDescription = pv.Product.ProductDescription,
             ProductSlug = pv.Product.ProductSlug,
             ColorName = pv.Color.ColorName,
             MaterialName = pv.Material.MaterialName,
             SizeName = pv.Size.SizeName,
             UOMName = pv.Product.Uom.Uomname,
             SubUOMName = pv.SubUom.SubUomname,
             Uomid = pv.Product.Uomid,
             PriceFormat = pv.PriceFormat,
             SubUomid = pv.SubUomid,
             BrandName = pv.Product.Brand.BrandName,
             QuantityPerUnit = pv.QuantityPerUnit,
             IsSerial = pv.IsSerial,
         };



            if (!isPaginated)
            {

                result.TopProducts = await query
                    .OrderByDescending(p => p.CreatedOn)
                    .Take(5)
                    .ToListAsync();


                result.AllProducts = await query.ToListAsync();
            }
            else
            {

                result.AllProducts = await query
                    .OrderByDescending(p => p.CreatedOn)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            result.TopProducts = UpdateCurrentPrice(result.TopProducts);
            result.AllProducts = UpdateCurrentPrice(result.AllProducts);

            return result;
        }





        public async Task<int> AddProductVariant(ProductVariantVM model)
        {
            
            string query = @"
DECLARE @VariantId UNIQUEIDENTIFIER = NEWID();

IF EXISTS (
    SELECT 1 FROM Inv.ProductVariants WHERE BarCode = @BarCode AND IsDeleted = 0
)
BEGIN
    SELECT -1 AS Result;  -- Barcode already exists
END
ELSE
BEGIN
    -- Insert into ProductVariants
    INSERT INTO Inv.ProductVariants (
        VariantId, MaterialId, ColorId, SizeId, ProductId,
        BarCode, SubUOMId, QuantityPerUnit, IsSerial,
        MinQty, MaxQty, CreatedOn, CreatedBy, ModifiedOn,
        IsActive, IsDeleted,
        BranchId, OrganizationId,TaxSlabId
    )
    VALUES (
        @VariantId, @MaterialId, @ColorId, @SizeId, @ProductId,
        @BarCode, @SubUOMId, @QuantityPerUnit, @IsSerial,
        @MinQty, @MaxQty, @CreatedOn, @CreatedBy, @ModifiedOn,
        @IsActive, @IsDeleted, 
        @BranchId, @OrganizationId,@TaxSlabId
    );

    -- Insert branch-specific pricing/quantity into BranchStock
 
DECLARE @BranchStockId UNIQUEIDENTIFIER = NEWID();

INSERT INTO Inv.BranchStock (
    BranchStockId, ProductVariantId, BranchId, SalePrice, RetailPrice, PromotionPrice, Cost,
    IsActive, IsDeleted, CreatedOn, CreatedBy
)
VALUES (
    @BranchStockId, @VariantId, @BranchId, @SalePrice, @RetailPrice, @PromotionPrice, @Cost, 
    @IsActive, @IsDeleted, @CreatedOn, @CreatedBy
);


    SELECT 300 AS Result;
END
";


            var commonParams = CommonParamHelper.GetCommonParams();

            var param = new
            {
                model.MaterialId,
                model.ColorId,
                model.SizeId,
                model.ProductId,
                model.Cost,
                model.BarCode,
                model.SalePrice,
                model.PromotionPrice,
                model.RetailPrice,
                model.SubUomid,
                model.QuantityPerUnit,
                model.IsSerial,
                model.MinQty,
                model.MaxQty,
                commonParams.CreatedOn,
                commonParams.CreatedBy,
                commonParams.ModifiedOn,
                commonParams.IsActive,
                commonParams.IsDeleted,
                commonParams.BranchId,
                commonParams.OrganizationId,
                model.TaxSlabId
            };

            var result = await _db.ExecuteQuery<int>(query, param);
            return result;
        }

        public async Task<Guid> SetVariantImage(Guid VariantImageId, Guid? VariantId)
        {
            string query = $@"
Update INV.ProductVariants set VariantImageId = @VariantImageId where VariantId = @VariantId

                          select @VariantImageId
                            ";
            //var commonParams = CommonParamHelper.GetCommonParams();

            var param = new
            {
                VariantImageId,
                VariantId

            };
            var result = await _db.ExecuteQuery<Guid>(query, param);
            return result;

        }

        public async Task<Guid> SetPriceFormat(int PriceFormat, Guid? VariantId)
        {
            string query = $@"
    Update INV.ProductVariants set PriceFormat = @PriceFormat where VariantId = @VariantId

                          select @VariantId
                            ";
            //var commonParams = CommonParamHelper.GetCommonParams();

            var param = new
            {
                PriceFormat,
                VariantId

            };
            var result = await _db.ExecuteQuery<Guid>(query, param);
            return result;

        }

        public async Task<int> UpdateVariant(UpdateVariantModel model)
        {
            string query = "";
            if (model.DataType == "BarCode")
            {
                query = @"
                        IF EXISTS (
                            SELECT 1 FROM INV.ProductVariants 
                            WHERE BarCode = @Value AND VariantId = @VariantId
                        )
                        BEGIN
                            SELECT -1
                        END
                        ELSE
                        BEGIN
                            UPDATE INV.ProductVariants 
                            SET BarCode = @Value 
                            WHERE VariantId = @VariantId
                            SELECT 1
                        END";

                var param = new
                {
                    model.Value,
                    VariantId = model.VariantId
                };


                var result = await _db.ExecuteQuery<int>(query, param);
                return result;

            }
            else if (model.DataType == "QuantityPerUnit" || model.DataType == "MinQty" || model.DataType == "MaxQty")
            {
                query = $@"
                    UPDATE INV.ProductVariants set {model.DataType} = {model.Value} where VariantId = '{model.VariantId}' AND BranchId = '{AppDataUtility.SessionUser.Person.Branch.BranchId}'";
                var allResult = await _db.ExecuteQueryModify(query);
                return allResult;
            }
            else
            {
                query = $@"
                    UPDATE INV.BranchStock set {model.DataType} = {model.Value} where VariantId = '{model.VariantId}' AND BranchId = '{AppDataUtility.SessionUser.Person.Branch.BranchId}'";
                var allResult = await _db.ExecuteQueryModify(query);
                return allResult;
            }





        }
        public List<ProductVariantVM> UpdateCurrentPrice(List<ProductVariantVM> model)
        {
            foreach (var item in model)
            {
                if (item.PriceFormat == (int)AppConstants.EnumPriceFormat.RetailPrice)
                {
                    item.CurrentPrice = item.RetailPrice;
                }
                else if (item.PriceFormat == (int)AppConstants.EnumPriceFormat.SalesPrice)
                {
                    item.CurrentPrice = item.SalePrice;
                }
                else if (item.PriceFormat == (int)AppConstants.EnumPriceFormat.PromotionPrice)
                {
                    item.CurrentPrice = item.PromotionPrice;
                }
                else // If null or doesn't match, default to RetailPrice
                {
                    item.CurrentPrice = item.RetailPrice;
                }
            }

            return model;
        }


    }
}
