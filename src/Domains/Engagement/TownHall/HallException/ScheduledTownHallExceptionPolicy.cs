namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled town hall exception policy.</summary>
public static class ScheduledTownHallExceptionPolicy
{
    public static string ScheduledTownHallExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a scheduled town hall exception code in its canonical STHE- form.</summary>
    public static string NormalizeScheduledTownHallExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STHE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STHE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledTownHallException()
    {
        var policy = Retry.DefaultPolicy("scheduled-town-hall-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int ScheduledTownHallExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
