using MainModels;
using MainModels.DTOModels;
using MainModels.Util;
using static Azure.Core.HttpHeader;

namespace MarketBal.Repository.Products
{
    public class ProductRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        string baseAPIURL;
        private readonly MemoryStream memoryStream;
        public ProductRepository(IConfiguration config)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            baseAPIURL = _config.GetValue<string>("SystemSettings:ContentAPIUrl");
            memoryStream = new MemoryStream();
        }

        public async Task<object> GetProducts(DataTableRequest request)
        {

            string tableName = "INV.Products";
            var columnMap = new List<string>
                {
                    "ProductId", "ProductName", "ProductDescription", "QOH","SubCategoryId", "BusinessStoreId", "CreatedOn", "CreatedBy","ModifiedOn", "IsActive", "IsDeleted"
                };

            var queries = ParamQueries.BuildDataTableQuery(tableName, "ProductName", request, columnMap);
            int totalRecords = await _db.ExecuteQuery<int>(queries.TotalRecordsQuery);
            int filteredRecords = await _db.ExecuteQuery<int>(queries.FilteredRecordsQuery);
            var data = await _db.ExecuteQueryList<ProductVM>(queries.DataQuery);
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.ToList()
            };
        }

        public async Task<bool> ActiveUnActiveProduct(Guid Id, int IsActive)
        {
            string query = $"UPDATE INV.Products SET IsActive={IsActive} WHERE ProductId='{Id}'";
            var result = await _db.ExecuteQuery<int>(query, 1);

            return result > 0;
        }


        public async Task<Guid> AddProduct(ProductVM product)
        {
            string ProductSlug = HelperClass.CreateSlug(product.ProductName);
            string query = @"
        DECLARE @NewProductId UNIQUEIDENTIFIER = NEWID();

        INSERT INTO INV.Products (
            ProductId, ProductName, ProductDescription, SubCategoryId, BranchId,
            CreatedOn, Createdby, ModifiedOn, IsActive, IsDeleted, ProductSlug, UOMId,BrandId
        )
        VALUES (
            @NewProductId, @ProductName, @ProductDescription, @SubCategoryId, @BranchId,
            @CreatedOn, @CreatedBy, @ModifiedOn, 1, 0, @ProductSlug, @UOMId,@BrandId
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
                commonParams.CreatedOn,
                commonParams.CreatedBy,
                commonParams.ModifiedOn,
                ProductSlug,
                product.UOMId,
                product.BrandId
            };
            var data = await _db.ExecuteQuery<Guid>(query, param);
            
            return data;
        }

        public async Task<ProductVM> GetProduct(Guid ProductId)
        {
            string query = $@"select p.ProductId,p.ProductName,uom.UOMName,p.UOMId,p.ProductDescription, sc.SubCategoryId,sc.SubCategoryName,c.CategoryName, d.DepartmentName,p.ProductSlug, ppimg.ImageUrl from Inv.Products p 
JOIN INV.SubCategory sc on p.SubCategoryId = sc.SubCategoryId 
JOIN INV.Categories c on sc.CategoryId =c.CategoryId 
JOIN Inv.Departments d on c.DepartmentId=d.DepartmentId 
 JOIN INV.UOM uom on p.UOMId = uom.UOMId
LEFT JOIN Inv.ProductImages ppimg on p.Productid = ppimg.ProductId and ppimg.IsDefault = 1
                where P.ProductId = @ProductId";
            var param = new
            {
                ProductId
            };
            var result = await _db.GetSingleItemDatatWithQueryAndParam<ProductVM>(query,param);
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
        public async Task<Guid> SaveProductImage(Guid ProductId,string ImageUrl)
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
            string query = $@"select p.ProductId,p.ProductName,p.ProductDescription,p.ProductSlug,c.ColorName,m.MaterialName,s.SizeName,uom.UOMName,uoms.SubUOMName,pv.QuantityPerUnit,pv.VariantImageId,b.BrandName  from inv.ProductVariants pv 
            JOIN Inv.Products p on pv.productId = p.productid
            JOIN INv.Colors c on pv.colorid = c.ColorId
            JOIN INV.Material m on pv.MaterialId = m.MaterialId
            JOIN INV.Sizes s on pv.SizeId = s.SizeId
            JOIN INV.UOM uom on p.UOMId = uom.UOMId
            JOIN INV.UOMSub uoms on pv.SubUOMId = uoms.SubUOMId
JOIN INV Brands b on p.BrandId = b.BrandId
            where pv.ProductId= @ProductId and pv.IsActive =1 and pv.IsDeleted =0";
            var param = new
            {
                ProductId
            };
            var result = await _db.GetDataListWithQueryAndParam<ProductVariantVM>(query,param);
            return result.ToList();
        }



        public async Task<Guid> AddProductVariant(ProductVariantVM model)
        {
            string query = $@"
Declare @VariantId uniqueidentifier = NewId()
                    INSERT INTO Inv.ProductVariants (VariantId,MaterialId,ColorId,SizeId,ProductId,Cost,BarCode,SalesPrice,PromotionPrice,RetailPrice,UOMId,SubUOMId,QuantityPerUnit,IsSerial,MinQty,MaxQty,CreatedOn,Createdby,ModifiedOn,IsActive,IsDeleted,BranchId) 
                                        VALUES (@VariantId,@MaterialId,@ColorId,@SizeId,@ProductId,@Cost,@BarCode,@SalesPrice,@PromotionPrice,@RetailPrice,@UOMId,@SubUOMId,@QuantityPerUnit,@IsSerial,@MinQty,@MaxQty,@CreatedOn,@CreatedBy,@ModifiedOn,@IsActive,@IsDeleted,@BranchId);
select @VariantId
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
                model.SalesPrice,
                model.PromotionPrice,
                model.RetailPrice,
                model.Uomid,
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
                commonParams.BranchId
            };

            var result = await _db.ExecuteQuery<Guid>(query, param);
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
    }
}
