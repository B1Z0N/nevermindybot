using System;
using System.Linq.Expressions;

using Hangfire;
using Hangfire.PostgreSql;

namespace nevermindy;

public static class Scheduler
{
    public static void InitJobStorage(string connectionString)
    {
        GlobalConfiguration.Configuration
            .UseColouredConsoleLogProvider()
            .UsePostgreSqlStorage(connectionString);
    }

    public static void Schedule(Expression<Action> f, TimeSpan ts)
    {
        BackgroundJob.Schedule(f, ts);
    }

    public static void RunServer()
    {
        var options = new BackgroundJobServerOptions
        {
            SchedulePollingInterval = TimeSpan.FromSeconds(1)
        };
        
        using (var server = new BackgroundJobServer(options))
        {
            Console.WriteLine("Press [ENTER] to stop...");
            Console.ReadLine();
        }
    }
}
