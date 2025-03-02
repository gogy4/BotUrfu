using HtmlAgilityPack;
using PuppeteerSharp;
using TgPetProject.FormatterHtml;

namespace TgPetProject.UtilsForService;

public class Utils
{
    public async Task<string> GetPage(Page page, string newPage, string waitElement, string removeDiv, string site)
    {
        await page.GoToAsync(newPage);
        await page.WaitForSelectorAsync($"#{waitElement}", new WaitForSelectorOptions { Visible = true });
        var pageHtml = await page.GetContentAsync();
        var htmlWithoutTags = await Parse(pageHtml, removeDiv, waitElement);
        await page.GoToAsync("https://istudent.urfu.ru/");
        var formatter = site == "brs"
            ? new FormatBrs().Format(htmlWithoutTags)
            : new FormatSchedule().Format(htmlWithoutTags);
        return await formatter;
    }

    private async Task<string> Parse(string schedule, string removeDiv, string findContainer)
    {
        if (string.IsNullOrEmpty(schedule)) return string.Empty;

        var doc = new HtmlDocument();
        doc.LoadHtml(schedule);

        var scheduleDiv = doc.DocumentNode.SelectSingleNode($"//div[@id='{findContainer}']");

        if (scheduleDiv == null) return string.Empty;
        var alertDiv = scheduleDiv.SelectSingleNode(removeDiv);
        alertDiv?.Remove();
        return scheduleDiv.InnerText.Trim();
    }
}