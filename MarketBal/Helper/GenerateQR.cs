using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using System.IO;
namespace MarketBal.Helper
{
    public class GenerateQR
    {
        public static string GenerateBarCode(string invoiceNo)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(invoiceNo, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData); 

            var qrBytes = qrCode.GetGraphic(20); 
            var base64 = Convert.ToBase64String(qrBytes);
            return $"data:image/png;base64,{base64}";
        }

    }
}
