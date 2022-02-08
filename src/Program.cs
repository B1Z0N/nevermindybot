using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Threading.Tasks;

using Telegram.Bot;


public static class Program
{
    const string accessToken = "ACCESS_TOKEN";
    static IConfiguration conf = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

    public static async Task Main()
    {
        var botClient = new TelegramBotClient(conf[accessToken]);
        
        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Hello, World! I am user {me.Id} and my name is {me.FirstName}.");
    }
}

