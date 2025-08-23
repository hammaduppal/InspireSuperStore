using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace MarketBal.Helper.PDF
{
    public class PdfGenerator
    {

        public static async Task<byte[]> GeneratePdfAsync(
      string htmlContent,
      PdfOptions? options = null)
        {
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" }
            });

            using var page = await browser.NewPageAsync();
            await page.SetContentAsync(htmlContent);

            // Measure content height in pixels
            var contentHeightPx = await page.EvaluateFunctionAsync<int>("() => document.body.scrollHeight");

            // Convert px → mm (1mm ≈ 3.78px)
            var contentHeightMm = contentHeightPx / 2.78;

            // Default POS-style options if not provided
            options ??= new PdfOptions
            {
                Width = "80mm",   // POS roll width
                PrintBackground = true,
                Landscape = false,
                MarginOptions = new MarginOptions
                {
                    Top = "2mm",
                    Bottom = "2mm",
                    Left = "2mm",
                    Right = "2mm"
                }
            };

            // Apply dynamic height
            options.Height = $"{contentHeightMm}mm";

            // Generate and return PDF
            return await page.PdfDataAsync(options);
        }

    }
}
