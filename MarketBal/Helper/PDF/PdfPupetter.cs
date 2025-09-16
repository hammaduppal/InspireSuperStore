using System.Runtime.InteropServices;
using System.Text.Json;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace MarketBal.Helper.PDF
{


    namespace OMS.Data.Repositories.PDFGenerate
    {
        public class PdfPupetter
        {

            public async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
            {
                // Adjust path for Plesk/IIS hosting
                string executablePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "chrome-win",
                    "chrome.exe"
                );

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = executablePath,
                    Args = new[]
                               {
                            "--no-sandbox",
                            "--disable-setuid-sandbox",
                            "--disable-web-security",
                            "--disable-features=IsolateOrigins,site-per-process",
                            "--ignore-certificate-errors",
                            "--allow-insecure-localhost",
                            "--headless=new",
                            "--disable-gpu",
                            "--disable-software-rasterizer",
                            "--window-size=1920,1080",
                            "--disable-dev-shm-usage",
                            "--disable-extensions",
                            "--disable-default-apps",
                            "--disable-background-networking",
                            "--no-first-run",
                            "--kiosk-printing"
                        }
                };

                using var browser = await Puppeteer.LaunchAsync(launchOptions);
                using var page = await browser.NewPageAsync();

                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                var pdfBytes = await page.PdfDataAsync(new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true,
                    MarginOptions = new MarginOptions
                    {
                        Top = "0mm",
                        Bottom = "0mm",
                        Left = "8mm",
                        Right = "8mm"
                    }
                });

                return pdfBytes;
            }


            #region CommentedCode

            //public async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
            //{
            //    await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            //    using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            //    using var page = await browser.NewPageAsync();


            //    await page.SetContentAsync(htmlContent, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle0 } });


            //    var pdfBytes = await page.PdfDataAsync(new PdfOptions
            //    {
            //        Format = PaperFormat.A4,
            //        PrintBackground = true,
            //        MarginOptions = new MarginOptions
            //        {
            //            Top = "0mm",
            //            Bottom = "0mm",
            //            Left = "8mm",
            //            Right = "8mm"
            //        }
            //    });
            //    return pdfBytes;
            //}



            //private async Task<string> DownloadChromium()
            //{

            //    var browserFetcher = new BrowserFetcher();
            //    await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            //    string path = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);
            //    return path;
            //}
            ////public class PdfPupetter
            //{
            //public async Task<byte[]> GeneratePdfFromHtmlUsingExternalChromium(string htmlContent, bool IsLandScape = false, MarginOptions options = null, PaperFormat pageFormat = null)
            //{
            //    //c:\chrome-win\chrome --headless --no-sandbox --disable-gpu --remote-debugging-port=9222
            //    using var httpClient = new HttpClient();
            //    string json = await httpClient.GetStringAsync("http://localhost:9222/json/version");
            //    string webSocketDebuggerUrl = JsonDocument.Parse(json)
            //        .RootElement
            //        .GetProperty("webSocketDebuggerUrl")
            //        .GetString();

            //    var browser = await Puppeteer.ConnectAsync(new ConnectOptions
            //    {
            //        BrowserWSEndpoint = webSocketDebuggerUrl
            //    });

            //    var page = await browser.NewPageAsync();
            //    await page.SetContentAsync(htmlContent, new NavigationOptions
            //    {
            //        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            //    });
            //    if (options == null)
            //    {
            //        options = new MarginOptions
            //        {
            //            Top = "0mm",
            //            Bottom = "0mm",
            //            Left = "10mm",
            //            Right = "10mm"
            //        };
            //    }
            //    if (pageFormat == null)
            //    {
            //        pageFormat = PaperFormat.A4;
            //    }
            //    var pdfBytes = await page.PdfDataAsync(new PdfOptions
            //    {
            //        Format = pageFormat,
            //        PrintBackground = true,
            //        Landscape = IsLandScape,

            //        MarginOptions = options
            //    });
            //    await page.CloseAsync();
            //    //await browser.CloseAsync(); 

            //    return pdfBytes;
            //}

            //public async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
            //{
            //    LaunchOptions launchOptions;
            //    bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            //    if (isLinux)
            //    {
            //        // Use system-installed Chromium in Linux
            //        string executablePath = "/usr/bin/chromium-browser"; // Or "/usr/bin/chromium"

            //        launchOptions = new LaunchOptions
            //        {
            //            Headless = true,
            //            ExecutablePath = executablePath,
            //            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            //        };
            //    }
            //    else
            //    {
            //        // Use downloaded Chromium in Windows (only download if not already exists)
            //        var browserFetcher = new BrowserFetcher();
            //        string path = browserFetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);

            //        if (!File.Exists(path))
            //        {
            //            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            //        }

            //        launchOptions = new LaunchOptions
            //        {
            //            Headless = true,
            //            ExecutablePath = path
            //        };
            //    }

            //    using var browser = await Puppeteer.LaunchAsync(launchOptions);
            //    using var page = await browser.NewPageAsync();

            //    await page.SetContentAsync(htmlContent, new NavigationOptions
            //    {
            //        WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            //    });

            //    var pdfBytes = await page.PdfDataAsync(new PdfOptions
            //    {
            //        Format = PaperFormat.A4,
            //        PrintBackground = true,
            //        MarginOptions = new MarginOptions
            //        {
            //            Top = "0mm",
            //            Bottom = "0mm",
            //            Left = "8mm",
            //            Right = "8mm"
            //        }
            //    });

            //    return pdfBytes;
            //}

            #endregion


        }

    }
}

