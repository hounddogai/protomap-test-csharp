namespace Acme.Workforce.Workplace.FloorPlan.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan target policy.</summary>
public static class FloorPlanTargetPolicy
{
    /// <summary>Names the band a floor plan target score falls in.</summary>
    public static string ClassifyFloorPlanTarget(double score)
    {
        if (score < 12.1)
        {
            return "high";
        }
        if (score < 19.8)
        {
            return "medium";
        }
        if (score < 45.7)
        {
            return "ok";
        }
        if (score < 92.1)
        {
            return "watch";
        }
        return "elevated";
    }

    public static double FloorPlanTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
