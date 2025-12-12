using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using MainModels;
using MainModels.DTOModels;
using MainModels.Models;
using MainModels.Util;
using MarketBal.Repository.Products;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MarketBal.Repository.SystemRP
{
    public class DeviceRegistrationRepository
    {

        private readonly IConfiguration _config;
        private readonly DBManager _db;
        private readonly ApiMethods _api;
        private readonly OneDb _onedb;
        private readonly AttributeRepository _attrib;
        public DeviceRegistrationRepository(IConfiguration config, OneDb oneDb)
        {
            _config = config;
            _db = new DBManager(_config);
            _api = new ApiMethods();
            _onedb = oneDb;
            _attrib = new AttributeRepository(_config, _onedb);
        }
        public async Task<bool> IsAlreadyRegistered(DeviceRequestModelVM device)
        {
            // 1️⃣ Find device by unique ID
            var existingDevice = await _onedb.Devices
                .FirstOrDefaultAsync(x => x.DeviceUniqueId == device.DeviceUniqueId);

            if (existingDevice != null)
            {
                try
                {


                    // 3️⃣ Deserialize JSON into model

                    var bindedData = JsonConvert.DeserializeObject<DeviceRequestModelVM>(existingDevice.DevicePayload);

                    // 4️⃣ Compare all fields
                    bool isSameDevice =
                        string.Equals(bindedData.DeviceUniqueId, device.DeviceUniqueId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bindedData.DeviceName, device.DeviceName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bindedData.IpAddress, device.IpAddress, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bindedData.BrowserName, device.BrowserName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bindedData.OperatingSystem, device.OperatingSystem, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(bindedData.AppVersion, device.AppVersion, StringComparison.OrdinalIgnoreCase);

                    return isSameDevice;
                }
                catch (Exception ex)
                {

                    throw;
                }
                // 2️⃣ Decrypt stored payload

            }


            return false;
        }

        public async Task<int> RegisteredDevicesCount()
        {
            return await _onedb.Devices.CountAsync();
        }
        public async Task<int> RegisterNewDevice(DeviceRequestModelVM device, Guid branchId)
        {

            await _onedb.Devices.AddAsync(new Device
            {
                DeviceUniqueId = device.DeviceUniqueId,
                BranchId = branchId,
                DevicePayload = JsonConvert.SerializeObject(device),
                CreatedAt = DateTime.Now
            });
            return await _onedb.SaveChangesAsync();
        }

     
       

    }

}