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

using Newtonsoft.Json;
 
namespace nevermindy;

public static class Program
{
    const string accessTokenConfKey = "BOT_ACCESS_TOKEN";
    
    static IConfiguration conf = new ConfigurationBuilder().AddJsonFile("appsettings.prod.json", false, true).Build();

    public static void Main()
    {
        using var cts = new CancellationTokenSource();
        SpacedRepetition.InitFibonacci(conf);
        Scheduler.InitJobStorage(conf);
        Handler.InitClient(conf[accessTokenConfKey], cts.Token);
        Scheduler.RunServer();

        cts.Cancel();
    }
}

