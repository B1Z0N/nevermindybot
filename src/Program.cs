using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Threading.Tasks;
using System.Threading;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Nevermindy;
using static Handlers;

public static class Program
{
    const string accessToken = "ACCESS_TOKEN";
    static IConfiguration conf = new ConfigurationBuilder().AddJsonFile("appsettings.prod.json", false, true).Build();

    public static async Task Main()
    {
        var botClient = new TelegramBotClient(conf[accessToken]);
        using var cts = new CancellationTokenSource();
        ReceiverOptions receiverOptions = new() { AllowedUpdates = { } }; // recieve all updates

        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cts.Token);

        var me = await botClient.GetMeAsync();
        Console.Title = me.Username ?? "My awesome Bot";
        Console.WriteLine($"Start listening for @{me.Username}");
        Console.ReadLine();

        cts.Cancel();
    }
}

