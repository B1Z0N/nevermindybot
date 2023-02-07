using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

using Npgsql;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace nevermindy;

public static class Database 
{
    const string connectionStringConfKey = "CONNECTION_STRING";
    
    private static NpgsqlDataSource dataSource; 

    public static void Init(IConfiguration conf)
    {
        dataSource = NpgsqlDataSource.Create(conf[connectionStringConfKey]); 
    }


    // disable the "MA0004: Use Task.ConfigureAwait(false) as the current SynchronizationContext is not needed" // we don't need it, it's not library code anyway
    #pragma warning disable MA0004

    private const string insertJobSql = "INSERT INTO MessageToItsDeletionJobId VALUES ($1, $2, $3)";
    public static async Task InsertMessageAndItsDeletionJobId(Message msg, string jobId) 
    {
        await using (var cmd = dataSource.CreateCommand(insertJobSql))
        {
            cmd.Parameters.AddWithValue(msg.Chat.Id);
            cmd.Parameters.AddWithValue(msg.MessageId);
            cmd.Parameters.AddWithValue(jobId);
            await cmd.ExecuteNonQueryAsync();
        }
    }

    private const string selectJobSql = "SELECT DeletionJobId FROM MessageToItsDeletionJobId WHERE ChatId = $1 AND MessageId = $2";  
    public static async Task<string> GetDeletionJobIdByMessage(Message msg) 
    {
        await using (var cmd = dataSource.CreateCommand(selectJobSql))
        {
            cmd.Parameters.AddWithValue(msg.Chat.Id);
            cmd.Parameters.AddWithValue(msg.MessageId);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                await reader.ReadAsync();
                return reader.GetString(0);
            }
        }
    }

    #pragma warning restore MA0004


    public static void Dispose()
    {
        dataSource.Dispose();
    }


}
