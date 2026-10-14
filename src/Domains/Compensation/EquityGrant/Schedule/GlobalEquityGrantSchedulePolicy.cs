namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global equity grant schedule policy.</summary>
public static class GlobalEquityGrantSchedulePolicy
{
    public static string GlobalEquityGrantScheduleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalEquityGrantSchedule()
    {
        var policy = Retry.DefaultPolicy("global-equity-grant-schedule");
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

    /// <summary>Names the band a global equity grant schedule score falls in.</summary>
    public static string ClassifyGlobalEquityGrantSchedule(double score)
    {
        if (score < 5.8)
        {
            return "ok";
        }
        if (score < 36.8)
        {
            return "minimal";
        }
        if (score < 59.9)
        {
            return "high";
        }
        return "elevated";
    }

    /// <summary>Puts a global equity grant schedule code in its canonical GEGS- form.</summary>
    public static string NormalizeGlobalEquityGrantScheduleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GEGS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GEGS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
