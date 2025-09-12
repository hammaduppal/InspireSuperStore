using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace MarketBal.Helper.PDF
{
    public class PdfService : IAsyncDisposable
    {
        private readonly string _browserDownloadPath;
        private readonly string _revision = BrowserFetcher.DefaultChromiumRevision; // if your PuppeteerSharp only exposes DefaultRevision, replace it
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private IBrowser? _browser;

        public PdfService(string? browserDownloadPath = null)
        {
            // persistent folder (inside app root by default)
            _browserDownloadPath = browserDownloadPath ?? Path.Combine(AppContext.BaseDirectory, "puppeteer_chromium");
        }

        /// <summary>
        /// Ensure chromium is present and a browser is launched. Call once on app start (or lazily on first request).
        /// </summary>
      
        public async Task InitializeAsync()
        {
            if (_browser != null) return;

            await _initLock.WaitAsync();
            try
            {
                if (_browser != null) return; // another thread might have finished

                var fetcherOptions = new BrowserFetcherOptions
                {
                    Path = _browserDownloadPath
                };
                var fetcher = new BrowserFetcher(fetcherOptions);

                // only ONE download at a time will happen
                await fetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

                var executablePath = fetcher.GetExecutablePath(BrowserFetcher.DefaultChromiumRevision);

                var launchOptions = new LaunchOptions
                {
                    Headless = true,
                    ExecutablePath = executablePath,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                };

                _browser = await Puppeteer.LaunchAsync(launchOptions);
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <summary>
        /// Generate PDF from HTML. Opens a new page per request (cheap compared to launching browsers).
        /// </summary>
        public async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent)) throw new ArgumentNullException(nameof(htmlContent));

            if (_browser == null) await InitializeAsync();

            // create a fresh page, render, then close the page
            await using var page = await _browser!.NewPageAsync();

            await page.SetContentAsync(htmlContent, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            }).ConfigureAwait(false);

            // optional: wait for fonts to load (helps PDF text rendering)
            try { await page.EvaluateExpressionHandleAsync("document.fonts.ready").ConfigureAwait(false); } catch { /*ignore*/ }

            var pdf = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "0mm", Bottom = "0mm", Left = "8mm", Right = "8mm" }
            }).ConfigureAwait(false);

            await page.CloseAsync().ConfigureAwait(false);
            return pdf;
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                try { await _browser.CloseAsync().ConfigureAwait(false); }
                catch { /*ignore*/ }
                _browser = null;
            }

            _initLock.Dispose();
        }
    }
}
