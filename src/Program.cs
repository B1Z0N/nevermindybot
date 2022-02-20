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
    const string accessToken = "ACCESS_TOKEN";
    const string connectionString = "CONNECTION_STRING";
    
    static IConfiguration conf = new ConfigurationBuilder().AddJsonFile("appsettings.prod.json", false, true).Build();

    public static void Main()
    {
        using var cts = new CancellationTokenSource();

        Scheduler.InitJobStorage(conf[connectionString]);
        Handler.InitClient(conf[accessToken], cts.Token);
        Scheduler.RunServer();

        cts.Cancel();
    }
}

