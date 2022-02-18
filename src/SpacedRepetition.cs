using System;

namespace nevermindy;

public static class SpacedRepetition
{
    public static TimeSpan FibFirst = TimeSpan.FromDays(1); 
    public static TimeSpan FibSecond = TimeSpan.FromDays(3); 
}

public class FibonacciTimeSpan
{
    public TimeSpan Current { get; private set; }
    private TimeSpan Next { get; set; }

    public FibonacciTimeSpan(TimeSpan cur, TimeSpan next) => (Current, Next) = (cur, next); 

    public FibonacciTimeSpan Move() 
    {
        (Current, Next) = (Next, Current + Next);
        return this;
    }
}