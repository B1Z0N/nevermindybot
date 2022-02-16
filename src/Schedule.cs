using System;
using Hangfire;
using Hangfire.PostgreSql;

namespace nevermindy;

public static class Scheduler
{
    public static void Init(string connectionString)
    {
        GlobalConfiguration.Configuration.UsePostgreSqlStorage(connectionString);
    }

    
}