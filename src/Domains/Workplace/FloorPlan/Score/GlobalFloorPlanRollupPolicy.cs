namespace Acme.Workforce.Workplace.FloorPlan.Score;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global floor plan rollup policy.</summary>
public static class GlobalFloorPlanRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalFloorPlanRollup()
    {
        var policy = Retry.DefaultPolicy("global-floor-plan-rollup");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Puts a global floor plan rollup code in its canonical GFPR- form.</summary>
    public static string NormalizeGlobalFloorPlanRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GFPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GFPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double GlobalFloorPlanRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
