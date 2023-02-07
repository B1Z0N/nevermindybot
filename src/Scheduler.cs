using System;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

using Hangfire;
using Hangfire.PostgreSql;

namespace nevermindy;

public static class Scheduler
{
    const string connectionStringConfKey = "CONNECTION_STRING";
    const string pollingIntervalConfKey = "POLLING_INTERVAL_TIMESPAN";
    
    public static TimeSpan PollingInterval { get; private set; }  = TimeSpan.FromMinutes(20);

    public static void InitJobStorage(IConfiguration conf)
    {
        GlobalConfiguration.Configuration
            .UseColouredConsoleLogProvider()
            .UsePostgreSqlStorage(conf[connectionStringConfKey]);

        if (conf[pollingIntervalConfKey] != null) 
            PollingInterval = TimeSpan.Parse(conf[pollingIntervalConfKey], FormatDefaults.FormatProvider); 
    }

    public static string Schedule(Expression<Action> f, TimeSpan ts)
    {
        return BackgroundJob.Schedule(f, ts);
    }

    public static bool DeSchedule(string id)
    {
        return BackgroundJob.Delete(id);
    }

    public static void RunServer()
    {
        var options = new BackgroundJobServerOptions
        {
            SchedulePollingInterval = PollingInterval
        };
        
        using (var server = new BackgroundJobServer(options))
        {
            Console.WriteLine("Press [ENTER] to stop...");
            Console.ReadLine();
        }
    }
}
