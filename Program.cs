using Telegram.Bot;
using TgPetProject.Bot;

internal class Program
{
    private static readonly Bot bot = new();

    public static void Main(string[] args)
    {
        var client = new TelegramBotClient("");//ваш токен
        client.StartReceiving(bot.Update, bot.Error);
        Console.ReadLine();
    }
}