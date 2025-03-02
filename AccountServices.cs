using PuppeteerSharp;
using TgPetProject.GetFuncPage;
using TgPetProject.UtilsForService;

public class AccountService
{
    private readonly string loginUrl =
        "https://sso.urfu.ru/adfs/OAuth2/authorize?resource=https%3A%2F%2Fistudent.urfu.ru&type=web_server&client_id" +
        "=https%3A%2F%2Fistudent.urfu.ru&redirect_uri=https%3A%2F%2Fistudent.urfu.ru%2Fstudent%2Flogin%3Foauth&" +
        "response_type=code&scope=";

    private readonly Utils utils = new();
    public readonly GetFuncPage GetFuncPage = new();

    public async Task<Page> Login(string email, string password)
    {
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultRevision);

        var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        await page.GoToAsync(loginUrl);
        await page.WaitForSelectorAsync("input[name='UserName']");
        await page.TypeAsync("input[name='UserName']", email);
        await page.TypeAsync("input[name='Password']", password);
        await page.ClickAsync("span#submitButton");

        try
        {
            await page.WaitForSelectorAsync("#errorText", new WaitForSelectorOptions { Timeout = 3000 });
            await browser.CloseAsync();
            return null;
        }
        catch (PuppeteerException)
        {
        }

        await page.WaitForSelectorAsync("h1.module-title");
        var currentUrl = page.Url;

        if (currentUrl.Contains("dashboard") || currentUrl.Contains("student"))
        {
            _ = CloseSessionAfterDelay(browser, TimeSpan.FromMinutes(10));
            return page;
        }

        await browser.CloseAsync();
        return null;
    }

    private async Task CloseSessionAfterDelay(Browser browser, TimeSpan delay)
    {
        await Task.Delay(delay);
        await browser.CloseAsync();
    }
}