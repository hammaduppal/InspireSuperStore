using System.Text;
using DinkToPdf;
using static MarketBal.Repository.POSManager.POSRepository;

namespace MarketBal.Helper.PDF
{
    public class PdfGenerator
    {

        private static readonly SynchronizedConverter _converter =
          new SynchronizedConverter(new PdfTools());
        public static byte[] GeneratePosReceipt(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,

                    // ✅ Custom paper size in mm (width x height)
                    PaperSize = new PechkinPaperSize ("80mm" , "0mm"),    // 0mm height = auto expand

                    Margins = new MarginSettings { Top = 2, Bottom = 2, Left = 2, Right = 2 }
                },
                Objects =
        {
            new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            return _converter.Convert(doc);
        }


        public static byte[] GeneratePdf(string htmlContent)
        {
            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = new PechkinPaperSize("80mm", "0mm"),
                    //PaperSize = PaperKind.A4, 
                    Margins = new MarginSettings { Top = 10, Bottom = 10 }
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = htmlContent,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            return _converter.Convert(doc);
        }

        //public static async Task<byte[]> GeneratePdfAsync(
        //     string htmlContent,
        //     PdfOptions? options = null)
        //{
        //    var browserFetcher = new BrowserFetcher();
        //    await browserFetcher.DownloadAsync();

        //    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        //    {
        //        Headless = true,
        //        Args = new[] { "--no-sandbox" }
        //    });

        //    using var page = await browser.NewPageAsync();
        //    await page.SetContentAsync(htmlContent);

        //    // Measure content height in pixels
        //    var contentHeightPx = await page.EvaluateFunctionAsync<int>("() => document.body.scrollHeight");

        //    // Convert px → mm (1mm ≈ 3.78px)
        //    var contentHeightMm = contentHeightPx / 2.78;

        //    // Default POS-style options if not provided
        //    options ??= new PdfOptions
        //    {
        //        Width = "80mm",   // POS roll width
        //        PrintBackground = true,
        //        Landscape = false,
        //        MarginOptions = new MarginOptions
        //        {
        //            Top = "2mm",
        //            Bottom = "2mm",
        //            Left = "2mm",
        //            Right = "2mm"
        //        }
        //    };

        //    // Apply dynamic height
        //    options.Height = $"{contentHeightMm}mm";

        //    // Generate and return PDF
        //    return await page.PdfDataAsync(options);
        //}
        public class PdfRequest
        {
            public string HtmlContent { get; set; } = string.Empty;
            public string? HeaderHtml { get; set; }
            public string? FooterHtml { get; set; }
            public bool Landscape { get; set; } = false;
            public int MarginTop { get; set; } = 10;
            public int MarginBottom { get; set; } = 10;
            public int MarginLeft { get; set; } = 10;
            public int MarginRight { get; set; } = 10;
        }
    }
}
