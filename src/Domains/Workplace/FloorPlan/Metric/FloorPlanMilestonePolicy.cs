namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan milestone policy.</summary>
public static class FloorPlanMilestonePolicy
{
    public static double FloorPlanMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.42;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a floor plan milestone code in its canonical FPM- form.</summary>
    public static string NormalizeFloorPlanMilestoneCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
