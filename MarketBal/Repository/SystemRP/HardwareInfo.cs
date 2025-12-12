using System.Net.NetworkInformation;
using System.Text;

using System.Security.Cryptography;
using System.Management;
namespace MarketBal.Repository.SystemRP
{
    public static class HardwareInfo
    {
        // Get CPU ID
        public static string GetCpuId()
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("win32_processor"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        string id = mo.Properties["processorID"]?.Value?.ToString();
                        if (!string.IsNullOrEmpty(id))
                            return id;
                    }
                }
            }
            catch { }
            return "UnknownCPU";
        }

        // Get Motherboard Serial
        public static string GetMotherBoardId()
        {
            try
            {
                using (ManagementClass mc = new ManagementClass("Win32_BaseBoard"))
                {
                    foreach (ManagementObject mo in mc.GetInstances())
                    {
                        string serial = mo["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serial))
                            return serial;
                    }
                }
            }
            catch { }
            return "UnknownMB";
        }

        // Get System Drive Serial
        public static string GetDiskId()
        {
            try
            {
                using (ManagementObject disk = new ManagementObject(@"Win32_LogicalDisk.DeviceID=""C:"""))
                {
                    disk.Get();
                    string serial = disk["VolumeSerialNumber"]?.ToString();
                    if (!string.IsNullOrEmpty(serial))
                        return serial;
                }
            }
            catch { }
            return "UnknownDisk";
        }

        // Get first MAC Address
        public static string GetMacAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                            .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up)
                            .FirstOrDefault();

                if (nic != null)
                {
                    return nic.GetPhysicalAddress().ToString();
                }
            }
            catch { }
            return "UnknownMAC";
        }

        // Generate SHA256 Fingerprint
        public static string GetServerFingerprint()
        {
            string raw = $"{GetCpuId()}|{GetMotherBoardId()}|{GetDiskId()}|{GetMacAddress()}";
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(raw);
                byte[] hash = sha.ComputeHash(bytes);
                return string.Concat(hash.Select(b => b.ToString("X2")));
            }
        }
    }

}


