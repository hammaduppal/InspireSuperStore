using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MarketBal.Repository.Products
{
    public class AttributeRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly OneDb _onedb;
        public AttributeRepository(IConfiguration config,OneDb onedb)
        {
            _config = config;
            _db = new DBManager(_config);
            _onedb = onedb;
        }
        public async Task<int> AddDepartment(DepartmentVM model)
        {
            string DepartmentSlug = HelperClass.CreateSlug(model.DepartmentName);
            //string query = $"INSERT INTO INV.Departments (DepartmentId, DepartmentName, IsDeleted, IsActive, CreatedBy, CreatedOn, ModifiedOn) " +
            //      $"VALUES (NEWID(), @DepartmentName, @IsDeleted, @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn)";
            string query = @"
                        DECLARE @Exists INT;
                        SET @Exists = (SELECT COUNT(1) FROM INV.Departments WHERE DepartmentName = @DepartmentName AND IsDeleted = 0);

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Departments (DepartmentId, DepartmentName, IsDeleted, IsActive, CreatedBy, CreatedOn, ModifiedOn,DepartmentSlug)
                            VALUES (NEWID(), @DepartmentName, @IsDeleted, @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@DepartmentSlug);
    
                            SELECT 1 AS Result; -- Insert successful
                        END";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.DepartmentName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                DepartmentSlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }
        public async Task<int> AddCagtegory(CategoryVM model)
        {
            string CategorySlug = HelperClass.CreateSlug(model.CategoryName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.Categories 
                            WHERE CategoryName = @CategoryName AND DepartmentId = @DepartmentId AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; -- Category already exists with the same DepartmentId
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Categories (
                                CategoryId, CategoryName, DepartmentId, IsDeleted, 
                                IsActive, CreatedBy, CreatedOn, ModifiedOn,CategorySlug
                            )
                            VALUES (
                                NEWID(), @CategoryName, @DepartmentId, @IsDeleted, 
                                @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@CategorySlug
                            );

                            SELECT 1 AS Result; -- Insert successful
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.CategoryName,
                model.DepartmentId,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                CategorySlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }

        public async Task<int> AddSubCategory(SubCategoryVM model)
        {
            string SubCategorySlug = HelperClass.CreateSlug(model.SubCategoryName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.SubCategory 
                            WHERE SubCategoryName = @SubCategoryName AND CategoryId = @CategoryId AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.SubCategory (
                                SubCategoryId, SubCategoryName, CategoryId, IsDeleted, 
                                IsActive, CreatedBy, CreatedOn, ModifiedOn,SubCategorySlug,HarmonizedSystemCode
                            )
                            VALUES (
                                NEWID(), @SubCategoryName, @CategoryId, @IsDeleted, 
                                @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@SubCategorySlug,@HarmonizedSystemCode
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.SubCategoryName,
                model.CategoryId,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                SubCategorySlug,
                model.HarmonizedSystemCode
            };
            return await _db.ExecuteQuery<int>(query, param);
        }
        public async Task<int> AddColor(ColorVM model)
        {
            string ColorSlug = HelperClass.CreateSlug(model.ColorName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.Colors 
                            WHERE ColorName = @ColorName AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Colors (
                                ColorId, ColorName, IsDeleted, 
                                IsActive, CreatedBy, CreatedOn, ModifiedOn,ColorSlug
                            )
                            VALUES (
                                NEWID(), @ColorName,  @IsDeleted, 
                                @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@ColorSlug
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.ColorName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                ColorSlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }

        public async Task<int> AddSize(SizeVM model)
        {
            string SizeSlug = HelperClass.CreateSlug(model.SizeName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.Sizes 
                            WHERE SizeName = @SizeName AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Sizes (
                                SizeId, SizeName, IsDeleted, 
                                 IsActive, CreatedBy, CreatedOn, ModifiedOn,SizeSlug
                            )
                            VALUES (
                                NEWID(), @SizeName,  @IsDeleted, 
                                 @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@SizeSlug
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.SizeName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                SizeSlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }

        public async Task<int> AddUOM(UomVM model)
        {

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.UOM 
                            WHERE UOMName = @UOMName AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.UOM (
                                UOMId, UOMName, IsDeleted, 
                                 IsActive, CreatedBy, CreatedOn, ModifiedOn
                            )
                            VALUES (
                                NEWID(), @UOMName,  @IsDeleted, 
                                 @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.UOMName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
            };
            return await _db.ExecuteQuery<int>(query, param);
        }

        public async Task<int> AddSubUOM(UomsubVM model)
        {

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.UOMSub 
                            WHERE SubUOMName = @SubUOMName AND UOMId = @UOMId AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.UOMSub (
                                SubUOMId, SubUOMName,UOMId,ConversionFactor, IsDeleted, 
                                 IsActive, CreatedBy, CreatedOn, ModifiedOn
                            )
                            VALUES (
                                NEWID(), @SubUOMName, @UOMId,@ConversionFactor,  @IsDeleted, 
                                 @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.SubUOMName,
                model.UOMId,
                model.ConversionFactor,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
            };
            return await _db.ExecuteQuery<int>(query, param);
        }
        public async Task<int> AddMaterial(MaterialVM model)
        {
            string MaterialSlug = HelperClass.CreateSlug(model.MaterialName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.Material 
                            WHERE MaterialName = @MaterialName AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Material (
                                MaterialId, MaterialName, IsDeleted, 
                                 IsActive, CreatedBy, CreatedOn, ModifiedOn,MaterialSlug
                            )
                            VALUES (
                                NEWID(), @MaterialName,  @IsDeleted, 
                                 @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@MaterialSlug
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.MaterialName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                MaterialSlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }

        public async Task<int> AddBrand(BrandVM model)
        {
            string BrandSlug = HelperClass.CreateSlug(model.BrandName);

            string query = @"DECLARE @Exists INT;
                        SET @Exists = (
                            SELECT COUNT(1) 
                            FROM INV.Brands 
                            WHERE BrandName = @BrandName AND IsDeleted = 0
                        );

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Brands (
                                BrandId, BrandName, IsDeleted,
                                 IsActive, CreatedBy, CreatedOn, ModifiedOn,BrandSlug
                            )
                            VALUES (
                                NEWID(), @BrandName,  @IsDeleted,
                                 @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn,@BrandSlug
                            );

                            SELECT 1 AS Result;
                        END
                        ";
            var commonParams = CommonParamHelper.GetCommonParams();
            var param = new
            {
                model.BrandName,
                commonParams.IsDeleted,
                
                commonParams.IsActive,
                commonParams.CreatedBy,
                commonParams.CreatedOn,
                commonParams.ModifiedOn,
                BrandSlug
            };
            return await _db.ExecuteQuery<int>(query, param);
        }
        public async Task<List<BrandVM>> GetBrands()
        {
            string query = $@"select BrandId,BrandName from Inv.Brands where IsActive =1";
            var result = await _db.ExecuteQueryList<BrandVM>(query);
            return result.ToList();
        }
        public async Task<List<BrandModelVM>> GetBrandModels()
        {
            return await _onedb.BrandModels.Select(x => new BrandModelVM
            {
                 BrandModelId= x.BrandModelId,
                 ModelName = x.ModelName,
                 Brand =new BrandVM
                 {
                      BrandId= x.Brand.BrandId,
                      BrandName=x.Brand.BrandName
                 }
            }).ToListAsync();
        }
        public async Task<List<BrandModelVM>> GetBrandModel(Guid brandId)
        {
            return await _onedb.BrandModels.Where(x=>x.BrandId==brandId).Select(x => new BrandModelVM
            {
                BrandModelId = x.BrandModelId,
                ModelName = x.ModelName
               
            }).ToListAsync();
        }
        public async Task<int> AddBrandModel(BrandModelVM model)
        {
            bool exists = await _onedb.BrandModels
                .AnyAsync(x => x.BrandId == model.BrandId &&
                               x.ModelName.ToLower() == model.ModelName.ToLower());

            if (exists)
            {
                return -1; 
            }

            var entity = new BrandModel
            {
                BrandId = model.BrandId,
                ModelName = model.ModelName.Trim(),
                IsActive = true,
                CreatedOn = DateTime.UtcNow
            };

            await _onedb.BrandModels.AddAsync(entity);
            return await _onedb.SaveChangesAsync();
        }

        #region DCS
        public async Task<List<DepartmentVM>> GetDepartment()
        {
            string query = $@"select DepartmentId,DepartmentName from Inv.Departments where IsActive =1";
            var result = await _db.ExecuteQueryList<DepartmentVM>(query);
            return result.ToList();
        }

        public async Task<List<CategoryVM>> GetCategory()
        {
            string query = $@"select CategoryId,CategoryName,DepartmentId from Inv.Categories where IsActive =1";
            var result = await _db.ExecuteQueryList<CategoryVM>(query);
            return result.ToList();
        }
        public async Task<List<CategoryVM>> GetCategory(Guid? DepartmentId)
        {
            string query = $@"select CategoryId,CategoryName,DepartmentId from Inv.Categories where IsActive =1 AND DepartmentId = @DepartmentId";
            var param = new
            {
                DepartmentId
            };

            var result = await _db.GetDataListWithQueryAndParam<CategoryVM>(query, param);
            return result.ToList();
        }
        public async Task<List<SubCategoryVM>> GetSubCategory(Guid? CategoryId)
        {
            string query = $@"select SubCategoryId,SubCategoryName from Inv.SubCategory where  IsActive = 1 AND CategoryId = @CategoryId";
            var param = new
            {
                CategoryId
            };

            var result = await _db.GetDataListWithQueryAndParam<SubCategoryVM>(query, param);
            return result.ToList();
        }

        public async Task<List<UomVM>> GetUOM()
        {
            string query = $@"select UOMId,UOMName from Inv.UOM where IsActive = 1";
            var result = await _db.ExecuteQueryList<UomVM>(query);
            return result.ToList();
        }

        public async Task<List<UomsubVM>> GetSubUOMs(Guid? UomId)
        {
            string query = $@"select SubUOMId,UOMId,SubUOMName,ConversionFactor from Inv.UOMSub where UOMId=@UomId";
            var param = new
            {
                UomId
            };
            var result = await _db.GetDataListWithQueryAndParam<UomsubVM>(query,param);
            return result.ToList();
        }
        public async Task<List<SizeVM>> GetSizes()
        {
            string query = $@"select * from Inv.Sizes where SizeName is not null and IsActive =1";
          
            var result = await _db.GetDataListWithQueryAndParam<SizeVM>(query);
            return result.ToList();
        }
        public async Task<List<ColorVM>> GetColors()
        {
            string query = $@"select * from Inv.Colors where IsActive = 1";

            var result = await _db.GetDataListWithQueryAndParam<ColorVM>(query);
            return result.ToList();
        }
        public async Task<List<MaterialVM>> GetMaterials()
        {
            string query = $@"select * from Inv.Material where IsActive = 1";

            var result = await _db.GetDataListWithQueryAndParam<MaterialVM>(query);
            return result.ToList();
        }
        #endregion
        public async Task<List<DCSExcel>> GetDCS()
        {
            string query = $@"select d.DepartmentId,d.DepartmentName,c.CategoryId,c.CategoryName,sc.SubCategoryId,sc.SubCategoryName from Inv.Departments d join INV.Categories c on d.DepartmentId= c.DepartmentId join Inv.SubCategory sc on c.CategoryId = sc.CategoryId";
            var result = await _db.ExecuteQueryList<DCSExcel>(query);
            return result.ToList();
        }
        public async Task<List<UOMSUBUomExcel>> GETUOMSUBUOM()
        {
            string query = $@"select uom.UOMId, uom.UOMName,usub.SubUOMId,usub.SubUOMName from INV.UOM uom  join INV.UOMSUB usub on uom.UOMId = usub.UOMId";
            var result = await _db.ExecuteQueryList<UOMSUBUomExcel>(query);
            return result.ToList();
        }

        public async Task<bool> UpdateSlugs()
        {
            var deps = await _onedb.Departments.Where(x => x.DepartmentSlug == null).ToListAsync();
            foreach (var item in deps)
            {
                item.DepartmentSlug = HelperClass.CreateSlug(item.DepartmentName);
            }
            var cats = await _onedb.Categories.Where(x => x.CategorySlug == null).ToListAsync();
            foreach (var item in cats)
            {
                item.CategorySlug = HelperClass.CreateSlug(item.CategoryName);
            }

            var subCats = await _onedb.SubCategories.Where(x => x.SubCategorySlug == null).ToListAsync();
            foreach (var item in subCats)
            {
                item.SubCategorySlug = HelperClass.CreateSlug(item.SubCategoryName);
            }
            var sizes = await _onedb.Sizes.Where(x => x.SizeSlug == null).ToListAsync();
            foreach (var item in sizes)
            {
                item.SizeSlug = HelperClass.CreateSlug(item.SizeName);
            }
            var brands= await _onedb.Brands.Where(x => x.BrandSlug == null).ToListAsync();
            foreach (var item in brands)
            {
                item.BrandSlug = HelperClass.CreateSlug(item.BrandName);
            }
            var materials = await _onedb.Materials.Where(x => x.MaterialSlug == null).ToListAsync();
            foreach (var item in materials)
            {
                item.MaterialSlug = HelperClass.CreateSlug(item.MaterialName);
            }
            var colors = await _onedb.Colors.Where(x => x.ColorSlug == null).ToListAsync();
            foreach (var item in colors)
            {
                item.ColorSlug = HelperClass.CreateSlug(item.ColorName);
            }
            var products = await _onedb.Products.ToListAsync();
            foreach (var item in products)
            {
                item.ProductSlug = HelperClass.CreateProductLink(item.ProductName,item.SubCategoryId.Value);
            }

        


            await _onedb.SaveChangesAsync();
            return true;
        }
    }
}

