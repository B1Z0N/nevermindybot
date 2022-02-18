using System;
using Newtonsoft.Json;

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
        (Current, Next) = (Next, Current + Next);
        return this;
    }
}