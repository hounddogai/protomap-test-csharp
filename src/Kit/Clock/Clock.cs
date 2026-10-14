namespace Acme.Workforce.Kit.Clock;

/// <summary>Clock tells the current time in Unix seconds.</summary>
public interface IClock
{
    int Now();
}

/// <summary>SystemClock reads the wall clock.</summary>
public sealed class SystemClock : IClock
{
    public int Now()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

/// <summary>Functions of clock.</summary>
public static class Clock
{
    public static IClock System()
    {
        return new SystemClock();
    }
}
