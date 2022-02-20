using System;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using System.Text;

namespace nevermindy;

public static class SpacedRepetition
{
    public static TimeSpan FibFirst = TimeSpan.FromSeconds(1); 
    public static TimeSpan FibSecond = TimeSpan.FromSeconds(3); 
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