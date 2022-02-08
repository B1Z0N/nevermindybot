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

namespace Nevermindy;

public static class Handlers
{

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
        
        var action = message.Text!.Split(' ')[0] switch
        {
            "/start"   => HandleStart(botClient, message),
            _          => HandleTodo(botClient, message)
        };
        Message sentMessage = await action;
    }

    static async Task<Message> HandleStart(ITelegramBotClient botClient, Message message)
    {
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, 
            text: "Hello, just send me a short info you want to remember, and I'll guide you through our learning session");
    }

    static async Task<Message> HandleTodo(ITelegramBotClient botClient, Message message)
    {
        // TODO: schedule
        return await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, 
            text: "You've got it.");
    }
}

