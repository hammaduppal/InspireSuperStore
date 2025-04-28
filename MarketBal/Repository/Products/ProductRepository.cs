using MainModels;
using MainModels.DTOModels;
using MainModels.Util;

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
            var data = await _db.ExecuteQueryList<OrganizationVM>(queries.DataQuery);
            return new
            {
                draw = request.Draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data = data.ToList()
            };
        }
    }
}
