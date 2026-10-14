namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Functions of rolling floor plan rollup policy.</summary>
public static class RollingFloorPlanRollupPolicy
{
    /// <summary>Puts a rolling floor plan rollup code in its canonical RFPR- form.</summary>
    public static string NormalizeRollingFloorPlanRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RFPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RFPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RollingFloorPlanRollupSpan(int start, int end)
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
        return span / 86400;
    }
}
