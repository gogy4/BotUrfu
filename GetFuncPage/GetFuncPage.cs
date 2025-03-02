using PuppeteerSharp;
using TgPetProject.UtilsForService;

namespace TgPetProject.GetFuncPage;

public class GetFuncPage
{
    private readonly Utils utils = new ();
    
    public async Task<string> GetBrsPage(Page page)
    {
        return await utils.GetPage(page,
            "https://istudent.urfu.ru/s/servis-informirovaniya-studenta-o-ballah-brs",
            "disciplines", "//div[@id='empty-discipline-list-alert']", "brs");
    }
    
    public async Task<string> GetSchedule(Page page)
    {
        return await utils.GetPage(page, "https://istudent.urfu.ru/s/schedule",
            "schedule_its-schedule",
            ".//a[contains(@href, 'calendar.istudent.urfu.ru/get')]", "schedule");
    }
}