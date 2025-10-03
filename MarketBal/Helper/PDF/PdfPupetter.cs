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

            public async Task<byte[]> GeneratePdfFromHtml(string htmlContent, PdfOptions pdfOptions)
            {
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
                var contentHeightPx = await page.EvaluateFunctionAsync<int>("() => document.body.scrollHeight");
                var contentHeightMm = contentHeightPx / 3.78;
                pdfOptions.Height = $"{contentHeightMm}mm";

                await page.SetContentAsync(htmlContent, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

          
                return await page.PdfDataAsync(pdfOptions);
            }


            


        }

    }
}

