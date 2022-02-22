using System;
using System.Linq;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace nevermindy;

public static class TimespanExtensions
{
    public static string ToHumanReadableString(this TimeSpan t)
    {
        if (t.TotalSeconds < 1) {
            return $@"{t:s\.ff}s";
        }
        if (t.TotalMinutes < 1) {
            return $@"{t:%s}s";
        }
        if (t.TotalHours < 1) {
            return $@"{t:%m}m";
        }
        if (t.TotalDays < 1) {
            return $@"{t:%h}h";
        }

        return $@"{t:%d}d";
    }
}

/// <summary>
/// TimeSpans are not serialized consistently depending on what properties are present. So this 
/// serializer will ensure the format is maintained no matter what.
/// </summary>
public class TimespanConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Format: Days.Hours:Minutes:Seconds:Milliseconds
    /// </summary>
    public const string TimeSpanFormatString = @"d\.hh\:mm\:ss\:FFF";

    public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
        var timespanFormatted = $"{value.ToString(TimeSpanFormatString)}";
        writer.WriteValue(timespanFormatted);
    }

    public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        TimeSpan parsedTimeSpan;
        TimeSpan.TryParseExact((string)reader.Value, TimeSpanFormatString, null, out parsedTimeSpan);
        return parsedTimeSpan;
    }
}

public static class FibExtensions
{
    private static string LearnedDelimiter = "Learnt_";
    private static string ContinuedDelimiter = "Continued_";

    public static string ToLearnedCallbackString(this FibonacciTimeSpan fib)
    {
        return LearnedDelimiter + fib.ToString();
    }

    public static string ToContinuedCallbackString(this FibonacciTimeSpan fib)
    {
        return ContinuedDelimiter + fib.ToString();
    }

    public static bool IsLearned(string s)
    {
        return s.StartsWith(LearnedDelimiter); 
    }

    public static bool TryGetContinued(string s, out FibonacciTimeSpan fib)
    {
        fib = null;
        if (s.StartsWith(ContinuedDelimiter))
        {
            try 
            {
                fib = FibonacciTimeSpan.Parse(s.Substring(ContinuedDelimiter.Length));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} [ERROR] Can't parse FibonacciTimeSpan '{fib.ToString()}': {ex.ToString()}");
                fib = null;
            }
        }
        return false;
    }
}


public static class EmojiGenerator
{
    private static string[] Pool = new[] {"$( ͡° ͜ʖ ͡°)",  "$ಠ_ಠ",  "$ʕ•ᴥ•ʔ",  "$(｡◕‿◕｡)",  "$\\_(ツ)_/¯",  "$¯\\(°_o)/¯",  "$(╬ ಠ益ಠ)",  "$(⌒▽⌒)☞",  "$(´▽`)/",  "$(´ー｀)ノ",  "$V●ᴥ●Vฅ^•ﻌ•^ฅ",  "$^_^）o自自o（^_^ ）",  "$ಠ‿ಠ",  "$( ͡° ͜ʖ ͡°)",  "$ᕦ(ò_óˇ)ᕤ",  "$(◉‿◉)つ",  "$(❂‿❂)p",  "$⊙﹏⊙",  "$\\_(⊙︿⊙)_/¯",  "$°‿‿°",  "$¿ⓧ_ⓧﮌ(⊙.☉)7",  "$(´･_･`)",  "$٩(͡๏̯͡๏)۶",  "$ఠ_ఠ",  "$( ᐛ )ᕗ",  "$(⊙_◎)༼∵༽ ༼⍨༽ ༼⍢༽ ༼⍤༽",  "$ ಠ益ಠ ༽ﾉ",  "$(-_-t)",  "$(ಥ⌣ಥ)",  "$(づ￣ ³￣)づ",  "$(づ｡◕‿‿◕｡)づ",  "$(ノಠ ∩ಠ)ノ彡( \\o°o)( ﾟஇ‸இﾟ)ﾟ｡",  "$(´▽｀)ノ”",  "$( ఠൠఠ )ﾉ",  "$( ◔ ౪◔)「      ┑(￣Д ￣)┍",  "$(๑•́ ₃ •̀๑)⁽⁽ଘ( ˊᵕˋ )ଓ⁾⁾",  "$◔_◔ԅ(≖‿≖ԅ)",  "$( ˘ ³˘)♥ ( ˇ෴ˇ )",  "$(-_- )ゞ",  "$•́؈•̀ ₎",  "$(•́•́ლ)",  "${•̃_•̃}(ᵔᴥᵔ)(Ծ‸ Ծ)",  "$(•̀ᴗ•́)و ̑̑",  "$¬º-°]¬",  "$(☞ﾟヮﾟ)☞"};
    private static Random Generator = new();

    public static string Get() => Pool[Generator.Next(0, Pool.Length)];
}

public static class MessageUtils
{
    public static Message AddPrefix(this Message m, string prefix)
    {
        m.Text = $"{prefix}{m.Text}";
        m.Entities = m.Entities?.Select(e => { e.Offset += prefix.Length; return e; }).ToArray();
        return m;
    }

    public static Message RemovePrefix(this Message m, int len)
    {
        m.Text = m.Text.Substring(len);
        m.Entities = m.Entities?.Select(e => { e.Offset -= len; return e; }).ToArray();
        return m;
    }

    public static Message AddPostfix(this Message m, string postfix)
    {
        m.Text = $"{m.Text}{postfix}";
        return m;
    }

    public static Message RemovePostfix(this Message m, int len)
    {
        m.Text = m.Text.Substring(0, m.Text.Length - len);
        return m;
    }
}