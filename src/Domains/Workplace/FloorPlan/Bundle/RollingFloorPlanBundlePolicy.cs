namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling floor plan bundle policy.</summary>
public static class RollingFloorPlanBundlePolicy
{
    public static string RollingFloorPlanBundleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static double RollingFloorPlanBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.98;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rolling floor plan bundle score falls in.</summary>
    public static string ClassifyRollingFloorPlanBundle(double score)
    {
        if (score < 31.6)
        {
            return "minimal";
        }
        if (score < 93.5)
        {
            return "watch";
        }
        return "medium";
    }
}
