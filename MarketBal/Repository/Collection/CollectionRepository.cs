using System.Globalization;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MarketBal.Repository.Account;

namespace MarketBal.Repository.Collection
{
    public class CollectionRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly OneDb _onedb;
        private readonly FileRepository _file;
        public CollectionRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _onedb = oneDb;
            _file = new FileRepository();
        }
        public async Task<List<CollectionMasterVM>> GetCollections()
        {
            var query = "SELECT * FROM INV.CollectionMaster where isActive=@IsActive";
            var parameters = new
            {
                IsActive = true
            };
            var result = await _db.GetDataListWithQueryAndParam<CollectionMasterVM>(query, parameters);
            return result.ToList();
        }
        public async Task<int> AddNewCollection(AddCollectionVM model,IFormFile file)
        {
            var items = new List<CollectionDetail>();
            var collectionId = Guid.NewGuid();
            foreach (var item in model.VariantIds)
            {
                items.Add(new CollectionDetail
                {
                     CollectionDetailId = Guid.NewGuid(),
                    VariantId = item,
                   CollectionId =collectionId

                });

            }
            string[] splitedDates = model.DateRange.Split('-');
            if (splitedDates.Length != 2)
                throw new Exception("Invalid date range format. Expected format: 'MM/dd/yyyy - MM/dd/yyyy'");

            string startDateStr = splitedDates[0].Trim();
            string endDateStr = splitedDates[1].Trim();

            DateTime startDate, endDate;
            string format = "MM/dd/yyyy";

            if (!DateTime.TryParseExact(startDateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) ||
                !DateTime.TryParseExact(endDateStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                throw new Exception("Invalid date format. Please ensure it is 'MM/dd/yyyy'");
            }
            var imageUrl = await _file.SaveFile(file,"Collections","Collections");
            var ppc = new CollectionMaster
            {
                CollectionId = collectionId,
                CollectionName = model.CollectionName,
                CreatedAt = DateTime.Now,
                StartDate = startDate,
                EndDate = endDate,
                ImageUrl = imageUrl.ImageUrl,
                Description = model.CollectionDescription,
                IsActive = true,
            };
            await _onedb.CollectionMasters.AddAsync(ppc);
             await _onedb.SaveChangesAsync();

            await _onedb.CollectionDetails.AddRangeAsync(items);
            return await _onedb.SaveChangesAsync();
        }
    }
}
