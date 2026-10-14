namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled refresh cycle policy.</summary>
public static class ScheduledRefreshCyclePolicy
{
    /// <summary>Puts a scheduled refresh cycle code in its canonical SRC- form.</summary>
    public static string NormalizeScheduledRefreshCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SRC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SRC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledRefreshCycle()
    {
        var policy = Retry.DefaultPolicy("scheduled-refresh-cycle");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a scheduled refresh cycle score falls in.</summary>
    public static string ClassifyScheduledRefreshCycle(double score)
    {
        if (score < 13.7)
        {
            return "high";
        }
        if (score < 33.0)
        {
            return "medium";
        }
        if (score < 96.3)
        {
            return "critical";
        }
        if (score < 98.5)
        {
            return "elevated";
        }
        return "minimal";
    }
}
