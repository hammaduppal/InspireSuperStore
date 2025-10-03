using SixLabors.ImageSharp;
using SkiaSharp;
using ZXing;
using ZXing.Common;

namespace MarketBal.Helper
{
    public class GenerateBarCode
    {


public static string GenerateBarcode(string text)
    {
        var writer = new BarcodeWriterSvg
        {
            Format = BarcodeFormat.CODE_128, // or EAN_13, CODE_39 etc.
            Options = new ZXing.Common.EncodingOptions
            {
                Height = 80,
                Width = 300,
                Margin = 2
            }
        };

        // Generate as SVG (better for scaling) or PNG if needed
        var svgImage = writer.Write(text);

        // If you need PNG bytes instead of SVG
        var barcodeWriter = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Height = 80,
                Width = 300,
                Margin = 2
            }
        };

        var pixelData = barcodeWriter.Write(text);

        using var surface = SKSurface.Create(new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Bgra8888));
        surface.Canvas.Clear(SKColors.White);

        // Copy pixels to Skia surface
        surface.Canvas.DrawBitmap(
            SKBitmap.FromImage(SKImage.FromPixelCopy(new SKImageInfo(pixelData.Width, pixelData.Height, SKColorType.Gray8), pixelData.Pixels)),
            0, 0);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        var base64 = Convert.ToBase64String(data.ToArray());
        return $"data:image/png;base64,{base64}";
    }

}
}
