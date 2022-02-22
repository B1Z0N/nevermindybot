using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace nevermindy;

public static class SpacedRepetition
{
    const string fibFirstConfKey = "FIBONACCI_TIMESPAN_FIRST";
    const string fibSecondConfKey = "FIBONACCI_TIMESPAN_SECOND";

    public static void InitFibonacci(IConfiguration conf)
    {
        if (conf[fibFirstConfKey] != null) FibFirst = TimeSpan.Parse(conf[fibFirstConfKey]);
        if (conf[fibSecondConfKey] != null) FibSecond = TimeSpan.Parse(conf[fibSecondConfKey]);
    }

    public static TimeSpan FibFirst { get; private set; }  = TimeSpan.FromDays(1);
    public static TimeSpan FibSecond { get; private set; } = TimeSpan.FromDays(3); 
}

public class FibonacciTimeSpan
{
    [JsonConverter(typeof(TimespanConverter))]
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    public TimeSpan Current { get; private set; }

    [JsonConverter(typeof(TimespanConverter))]
    [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
    private TimeSpan Next { get; set; }

    public FibonacciTimeSpan(TimeSpan cur, TimeSpan next) => (Current, Next) = (cur, next); 

    public FibonacciTimeSpan Move() 
    {
        return new FibonacciTimeSpan(Next, Current + Next);
    }

    public FibonacciTimeSpan MoveBack()
    {
        return new FibonacciTimeSpan(Next - Current, Current);
    }

    public override string ToString()
    {
        return "Fib{" + Current + "," + Next + "}";
    }

    public static FibonacciTimeSpan Parse(string s)
    {
        var arr = new StringBuilder(s).Remove(s.Length - 1, 1).Remove(0, 4).ToString().Split(',', 2);
        var (cur, next) = (TimeSpan.Parse(arr[0]), TimeSpan.Parse(arr[1]));
        return new FibonacciTimeSpan(cur, next);
    }

}