using Market.Models;
using Market.Models.DTOModels;
using Market.Models.Util;

namespace MarketBAL.Repository.Products
{
    public class AttributeRepository
    {
        private readonly IConfiguration _config;
        private readonly DBManager _db;
        public AttributeRepository(IConfiguration config)
        {
            _config=config;
            _db = new DBManager(_config);
        }
        public async Task<int> AddDepartment(DepartmentVM model)
        {
            //string query = $"INSERT INTO INV.Departments (DepartmentId, DepartmentName, IsDeleted, BusinessStoreId, IsActive, CreatedBy, CreatedOn, ModifiedOn) " +
            //      $"VALUES (NEWID(), @DepartmentName, @IsDeleted, @BusinessStoreId, @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn)";
            string query = @"
                        DECLARE @Exists INT;
                        SET @Exists = (SELECT COUNT(1) FROM INV.Departments WHERE DepartmentName = @DepartmentName AND IsDeleted = 0);

                        IF (@Exists > 0)
                        BEGIN
                            SELECT 300 AS Result; 
                        END
                        ELSE
                        BEGIN
                            INSERT INTO INV.Departments (DepartmentId, DepartmentName, IsDeleted, BusinessStoreId, IsActive, CreatedBy, CreatedOn, ModifiedOn)
                            VALUES (NEWID(), @DepartmentName, @IsDeleted, @BusinessStoreId, @IsActive, @CreatedBy, @CreatedOn, @ModifiedOn);
    
                            SELECT 1 AS Result; -- Insert successful
                        END";
                                    var commonParams = CommonParamHelper.GetCommonParams();
            var param = new 
            {
                    DepartmentName=model.DepartmentName,
                     commonParams.IsDeleted, commonParams.BusinessStoreId,commonParams.IsActive,commonParams.CreatedBy,commonParams.CreatedOn,commonParams.ModifiedOn
            };
            return await _db.ExecuteQuery<int>(query,param);
        }
    }
}
