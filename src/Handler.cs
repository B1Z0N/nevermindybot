using System.Security.AccessControl;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft.Json;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
            UpdateType.Message       => HandleMessageRecieved(botClient, update.Message!),
            UpdateType.CallbackQuery => HandleCallbackQueryReceived(botClient, update.CallbackQuery!),
            _                        => Task.CompletedTask,
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
        
        var task = message.Text!.Split(' ')[0] switch
        {
            "/start" => HandleStart(botClient, message),
            _        => HandleTodo(botClient, message),
        };
        await task;
    }

    static async Task HandleStart(ITelegramBotClient botClient, Message message)
    {
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id, 
            text: "Hello, just send me a short info you want to remember, and I'll guide you through our learning session");
    }

    #region Reminder handling

    static async Task HandleTodo(ITelegramBotClient botClient, Message message)
    {
        var fib = new FibonacciTimeSpan(SpacedRepetition.FibFirst, SpacedRepetition.FibSecond);
        await botClient.SendTextMessageAsync(
            message.Chat.Id, 
            $"Got it! I'll text you soon  *{EmojiGenerator.Get()}*",
            parseMode: ParseMode.Markdown);
        Scheduler.Schedule(() => SendMessage(message.Chat.Id, message.Text, fib), fib.Current);
    }

    // public so that compiler could generate ExpressionTree for Hangfire 
    public static void SendMessage(long chatId, string msg, FibonacciTimeSpan fib)
    {
        var (prevFib, nextFib) = (fib.MoveBack(), fib.Move());

        var inlineKeyboard = new InlineKeyboardMarkup(new [] // reminders keyboard, see HandleCallbackQueryReceived for handling
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: prevFib.Current.ToHumanReadableString(),
                        callbackData: prevFib.ToContinuedCallbackString()),
                    InlineKeyboardButton.WithCallbackData(
                        text: fib.Current.ToHumanReadableString(),
                        callbackData: fib.ToContinuedCallbackString()),
                    InlineKeyboardButton.WithCallbackData(
                        text: nextFib.Current.ToHumanReadableString(),
                        callbackData: nextFib.ToContinuedCallbackString())
                },
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        text: "Learnt", 
                        callbackData: fib.ToLearnedCallbackString()),
                },
            });

        var _ = botClient.SendTextMessageAsync(
            chatId, 
            $"Your reminder:\n\n`{msg}`\n\nWhen should I remind you next time?", 
            replyMarkup: inlineKeyboard,
            parseMode: ParseMode.Markdown).Result;
    }

    // Handle reminders keys on keyboard, for the keys, see SendMessage
    private static async Task HandleCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        var todo = callbackQuery.Message.Text.Split("\n\n")[1].Trim('`');
        var chatId = callbackQuery.Message.Chat.Id;
        var messageId = callbackQuery.Message.MessageId;

        if (FibExtensions.IsLearned(callbackQuery.Data))
        {
            await botClient.EditMessageTextAsync(
                chatId, messageId, text: $"`{todo}`\n\nDone✅", parseMode: ParseMode.Markdown);
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Congrats, you've made it!");
        }
        else if (FibExtensions.TryGetContinued(callbackQuery.Data, out var fib))
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Postponed on {fib.Current.ToHumanReadableString()}");
            await botClient.DeleteMessageAsync(chatId, messageId);
            Scheduler.Schedule(() => SendMessage(chatId, todo, fib), fib.Current);
        }
    }

    #endregion Reminder handling

    #endregion Handlers
}

