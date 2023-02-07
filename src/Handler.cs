using System;
using System.Linq;
using System.Security.AccessControl;
using Microsoft.Extensions.Configuration; 
using Microsoft.Extensions.Configuration.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

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
    public static ITelegramBotClient botClient { get; private set; }

    private static TimeSpan telegramEditLimit = TimeSpan.FromHours(47);

    public static void InitClient(string botAccessToken, CancellationToken ct)
    {
        botClient = new TelegramBotClient(botAccessToken);
        botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, new() { AllowedUpdates = { } }, ct);

        Console.WriteLine($"Started nevermindy");
    }

    #region Handlers
    
    // disable the "MA0004: Use Task.ConfigureAwait(false) as the current SynchronizationContext is not needed"
    // we don't need it, it's not library code anyway
    #pragma warning disable MA0004
    
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
        var text = "Hello, I'm a [spaced repetition](https://en.wikipedia.org/wiki/Spaced_repetition) bot. Just send me a short info you want to remember, and I'll guide you through our learning session";
        await botClient.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Markdown);
    }

    #region Reminder handling

    static async Task HandleTodo(ITelegramBotClient botClient, Message message)
    {
        var fib = new FibonacciTimeSpan(SpacedRepetition.FibFirst, SpacedRepetition.FibSecond);
        var emoji = System.Security.SecurityElement.Escape(EmojiGenerator.Get());
        var text = $"Got it! I'll text it to you in {SpacedRepetition.FibFirst.ToHumanReadableString()}  <b>{emoji}</b>";

        await botClient.SendTextMessageAsync(message.Chat.Id, text, ParseMode.Html, replyToMessageId: message.MessageId);
        message.AddPrefix("Your reminder:\n\n").AddPostfix("\n\nWhen should I remind you next time?");
        Scheduler.Schedule(() => SendMessage(message, fib), fib.Current); 
    }

    // public so that compiler could generate ExpressionTree for Hangfire 
    public static void SendMessage(Message msg, FibonacciTimeSpan fib)
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

        var reminderMsg = botClient.SendTextMessageAsync(
            msg.Chat.Id, msg.Text, entities: msg.Entities, 
            disableWebPagePreview: true, // opinionated, it will be better to not show that large picutre
            replyMarkup: inlineKeyboard).Result;

        // Now resend a message if we are about to loose the ability to edit it
        // Remind the user to do the task
        var jobId = Scheduler.Schedule(() => ResendMessage(reminderMsg, msg, fib), telegramEditLimit); 
        Database.InsertMessageAndItsDeletionJobId(reminderMsg, jobId).GetAwaiter().GetResult();
    }

    public static void ResendMessage(Message reminderMsg, Message originalMsg, FibonacciTimeSpan fib)
    {
        botClient.DeleteMessageAsync(reminderMsg.Chat.Id, reminderMsg.MessageId).GetAwaiter().GetResult(); 
        SendMessage(originalMsg, fib);
    }

    // Handle reminders keys on keyboard, for the keys, see SendMessage
    private static async Task HandleCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
        const string delimiter = "\n\n";
        var arr = callbackQuery.Message.Text.Split("\n\n");
        var (prefix, todo, postfix) = (arr[0] + delimiter, arr[1], delimiter + arr[2]);
        var msg = callbackQuery.Message;

        if (FibExtensions.IsLearned(callbackQuery.Data))
        {
            var jobId = await Database.GetDeletionJobIdByMessage(msg);
            Scheduler.DeSchedule(jobId);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Congrats, you've made it!");

            msg.RemovePrefix(prefix.Length).RemovePostfix(postfix.Length).AddPostfix("\n\nDone✅");
            await botClient.EditMessageTextAsync(
                msg.Chat.Id, msg.MessageId, msg.Text, 
                entities: msg.Entities, disableWebPagePreview: true);
        }
        else if (FibExtensions.TryGetContinued(callbackQuery.Data, out var fib))
        {
            var jobId = await Database.GetDeletionJobIdByMessage(msg);
            Scheduler.DeSchedule(jobId);

            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Postponed on {fib.Current.ToHumanReadableString()}");

            await botClient.DeleteMessageAsync(msg.Chat.Id, msg.MessageId);
            Scheduler.Schedule(() => SendMessage(msg, fib), fib.Current);
        }
    }

    #endregion Reminder handling

    #pragma warning restore MA0004

    #endregion Handlers
}

