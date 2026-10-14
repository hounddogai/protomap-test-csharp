namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling merit cycle override policy.</summary>
public static class RollingMeritCycleOverridePolicy
{
    /// <summary>Names the band a rolling merit cycle override score falls in.</summary>
    public static string ClassifyRollingMeritCycleOverride(double score)
    {
        if (score < 16.3)
        {
            return "minimal";
        }
        if (score < 37.5)
        {
            return "high";
        }
        if (score < 60.1)
        {
            return "watch";
        }
        if (score < 60.9)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Puts a rolling merit cycle override code in its canonical RMCO- form.</summary>
    public static string NormalizeRollingMeritCycleOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RMCO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RMCO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingMeritCycleOverride()
    {
        var policy = Retry.DefaultPolicy("rolling-merit-cycle-override");
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
}
