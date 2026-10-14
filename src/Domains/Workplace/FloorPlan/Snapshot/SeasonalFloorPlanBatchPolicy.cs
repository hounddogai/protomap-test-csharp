namespace Acme.Workforce.Workplace.FloorPlan.Snapshot;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal floor plan batch policy.</summary>
public static class SeasonalFloorPlanBatchPolicy
{
    /// <summary>Puts a seasonal floor plan batch code in its canonical SFPB- form.</summary>
    public static string NormalizeSeasonalFloorPlanBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalFloorPlanBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.46;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
