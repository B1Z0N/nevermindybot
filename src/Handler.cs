using System.Security.AccessControl;
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

namespace nevermindy;

public static class Handler
{
    // static because 
    // 1. It's simple, no need to create custom JSON serializer for TelegramBotClient
    // 2. It's efficient, no need to keep botclient in storage, so we just keep the messages and recipients(see SendMessage)
    public static ITelegramBotClient botClient;

    public static void InitClient(string botAccessToken, CancellationToken ct)
    {
        botClient = new TelegramBotClient(botAccessToken);
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new() { AllowedUpdates = { } }, ct);

        Console.WriteLine($"Started nevermindy");
    }

    #region Handlers
    
    public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine(errorMessage);
        return Task.CompletedTask;
    }

    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var handler  = update.Type switch
        {
            UpdateType.Message => HandleMessageRecieved(botClient, update.Message!),
            _                  => Task.CompletedTask,
        };
        
        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(botClient, exception, cancellationToken);
        }
    }

    static async Task HandleMessageRecieved(ITelegramBotClient botClient, Message message)
    {
        if (message.Type != MessageType.Text)
            return;
        
        if (message.Text!.Split(' ')[0] == "/start") await HandleStart(botClient, message);
        else HandleTodo(botClient as TelegramBotClient, message);
    }

    static async Task HandleStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, 
            text: "Hello, just send me a short info you want to remember, and I'll guide you through our learning session");
    }

    static void HandleTodo(TelegramBotClient botClient, Message message)
    {
        var fib = new FibonacciTimeSpan(SpacedRepetition.FibFirst, SpacedRepetition.FibSecond);
        Scheduler.Schedule(() => SendMessage(message.Chat.Id, message.Text, fib), fib.Current);
    }

    // public so that compiler could generate ExpressionTree for Hangfire 
    public static void SendMessage(long chatId, string message, FibonacciTimeSpan fib)
    {
        botClient.SendTextMessageAsync(chatId, message).Result.Discard();

        fib.Move();
        Scheduler.Schedule(() => SendMessage(chatId, message, fib), fib.Current);
    }

    #endregion Handlers
}

