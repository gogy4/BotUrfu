using PuppeteerSharp;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgPetProject.Data;

namespace TgPetProject.Bot;

public class Bot
{
    private static readonly AccountService accountService = new();

    private readonly Dictionary<long, string>
        pendingLogins = new();

    private readonly Dictionary<long, Page> pages = new();
    private readonly DataBase db = new(accountService);
    private readonly HashSet<int> messagesToIgnore = new();

    public async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var message = update.Message;
        if (message?.Text is null)
            return;

        var chatId = message.Chat.Id;
        var messageId = message.MessageId;
        var text = message.Text;

        if (messagesToIgnore.Contains(messageId))
            return;
        var authorized = await AnswerNotAuthorizedUses(await db.IsAuthorized(chatId), chatId, text, botClient);
        switch (authorized)
        {
            case false:
                return;
            case true:
                AnswerAuthorizedUsers(text, botClient, chatId);
                break;
        }
    }

    private async Task<bool> AnswerNotAuthorizedUses(bool authorized, long chatId, string text,
        ITelegramBotClient botClient)
    {
        if (!authorized)
        {
            if (!pendingLogins.ContainsKey(chatId))
            {
                if (text.Contains("@"))
                {
                    pendingLogins[chatId] = text;
                    await botClient.SendTextMessageAsync(chatId, "Введите пароль: ");
                }
                else
                {
                    await botClient.SendTextMessageAsync(chatId, "Вы не авторизованы. Введите логин (email): ");
                }

                return false;
            }

            var password = text;
            var email = pendingLogins[chatId];

            var loginPage = await accountService.Login(email, password);
            if (loginPage is not null)
            {
                await db.SaveUserToDatabase(chatId, email, password);
                pendingLogins.Remove(chatId);
                await botClient.SendTextMessageAsync(chatId, "Успешная авторизация. Выберите действие. " +
                                                             "Выбирая действие: расписание, вы должны понимать, что " +
                                                             "оно не точно для некоторых направлений " +
                                                             "(у которых есть выбор в модеусе).",
                    replyMarkup: GetButtons());
                pages[chatId] = loginPage;
            }
            else
            {
                await botClient.SendTextMessageAsync(chatId, "Ошибка авторизации. Неверный логин или пароль.");
                pendingLogins.Remove(chatId);
                return false;
            }
        }

        return true;
    }

    private async Task AnswerAuthorizedUsers(string text, ITelegramBotClient botClient, long chatId)
    {
        text = text.ToLower();
        switch (text)
        {
            case "функции":
                await botClient.SendTextMessageAsync(chatId, "Вы можете посмотреть свое расписание " +
                                                             "или баллы брс с помощью кнопки 'расписание' или 'брс' " +
                                                             "соответсвенно, а так же можете написать 'расписание' " +
                                                             "или 'брс' в чат соответсвенно. Вы должны понимать, " +
                                                             "что расписание всегда точным не является, " +
                                                             "в частности для тех групп, " +
                                                             "у которых есть выбор в модеусе");
                break;
            case "о нас":
                await botClient.SendTextMessageAsync(chatId, "Бота написал @sapkjfl. " +
                                                             "Если есть ошибки или бот не отвечает, пишите мне в личку");
                break;
            case "расписание":
                if (!pages.TryGetValue(chatId, out var currentPage) || currentPage.Browser.IsClosed)
                {
                    var newPage = await db.ReLogin(chatId);
                    pages[chatId] = newPage;
                    var schedule = await accountService.GetFuncPage.GetSchedule(newPage);
                    await SendLongMessage(botClient, chatId, $"Ваше расписание: \n {schedule}");
                    break;
                }
                else
                {
                    var schedule = await accountService.GetFuncPage.GetSchedule(currentPage);
                    await SendLongMessage(botClient, chatId, $"Ваше расписание: \n {schedule}");
                    break;
                }


            case "брс":

                if (!pages.TryGetValue(chatId, out var page) || page.Browser.IsClosed)
                {
                    var newPage = await db.ReLogin(chatId);
                    pages[chatId] = newPage;
                    var brs = await accountService.GetFuncPage.GetBrsPage(newPage);
                    await botClient.SendTextMessageAsync(chatId, $"Ваши баллы брс: \n {brs}");
                    break;
                }
                else
                {
                    var brs = await accountService.GetFuncPage.GetBrsPage(page);
                    await botClient.SendTextMessageAsync(chatId, $"Ваши баллы брс: \n {brs}");
                    break;
                }
            case "/start":
                await botClient.SendTextMessageAsync(chatId, "Вы уже авторизованы.", replyMarkup: GetButtons());
                break;
        }
    }

    public Task Error(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken token)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        if (exception.Data.Contains("messageId"))
        {
            var messageId = (int)exception.Data["messageId"];
            messagesToIgnore.Add(messageId);
        }

        return Task.CompletedTask;
    }

    private IReplyMarkup GetButtons()
    {
        return new ReplyKeyboardMarkup
        {
            Keyboard = new List<IEnumerable<KeyboardButton>>
            {
                new List<KeyboardButton> { new() { Text = "БРС" }, new() { Text = "Расписание" } },
                new List<KeyboardButton> { new() { Text = "Функции" }, new() { Text = "О нас" } }
            }
        };
    }

    private async Task SendLongMessage(ITelegramBotClient botClient, long chatId, string text)
    {
        var parts = text.Split(new[] { "==========================" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            await botClient.SendTextMessageAsync(chatId, part.Trim());
        }
    }

}
