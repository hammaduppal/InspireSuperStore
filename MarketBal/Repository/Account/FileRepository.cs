using MainModels;
using MainModels.DTOModels;
using MainModels.Util;
using Newtonsoft.Json;
using static MainModels.ApiMethods;

namespace MarketBal.Repository.Account
{
    public class FileRepository
    {
        private readonly ApiMethods _api;
        private readonly ISessionService _sessionService;
        public FileRepository(ISessionService sessionService)
        {
            _api = new ApiMethods();
            _sessionService = sessionService;
        }
        public async Task<APIImageContentResponse> SaveFile(APIImageContentRequest request)
        {
            request.Website = _sessionService.SessionUser.Person.Branch.BranchId.ToString();
            string content = JsonConvert.SerializeObject(request);
            var apiresponse = await _api.PostMethodNew(PagesViewModel.SystemSettings.ContentAPIUrl, "/api/Content/UploadImage", content,"iStore");
            if (string.IsNullOrEmpty(apiresponse.ResponseString))
            {
                var rd = new APIImageContentResponse()
                {
                    Status = apiresponse.Status,
                     StatusCode = apiresponse.StatusCode,
                    Message = apiresponse.Message
                };
                return rd;
            }
            var returndata = JsonConvert.DeserializeObject<APIImageContentResponse>(apiresponse.ResponseString);
            return returndata;
        }
        public async Task<APIImageContentResponse> SaveFile(IFormFile file, string folderName, string dataType)
        {
            var result = await FileRepository.ConvertToBase64Async(file);
            var extension = "."+file.FileName.Split('.')[1];
            var FileRequest = new APIImageContentRequest
            {
                Folder = folderName,
                DataType = dataType,
                Base64String = result,
                FileExtension = extension
            };

            FileRequest.Website = _sessionService.SessionUser.Person.Branch.BranchId.ToString();
            string content = JsonConvert.SerializeObject(FileRequest);
            var apiresponse = await _api.PostMethodNew(PagesViewModel.SystemSettings.ContentAPIUrl, "/api/Content/UploadImage", content, "iStore");
            if (string.IsNullOrEmpty(apiresponse.ResponseString))
            {
                var rd = new APIImageContentResponse()
                {
                    Status = apiresponse.Status,
                    StatusCode = apiresponse.StatusCode,
                    Message = apiresponse.Message
                };
                return rd;
            }
            var returndata = JsonConvert.DeserializeObject<APIImageContentResponse>(apiresponse.ResponseString);
            return returndata;
        }
        public static async Task<string> ConvertToBase64Async(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return null;

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    return Convert.ToBase64String(fileBytes);
                }
            }
    }
}
