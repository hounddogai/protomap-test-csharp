namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of floor plan baseline policy.</summary>
public static class FloorPlanBaselinePolicy
{
    public static double FloorPlanBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a floor plan baseline score falls in.</summary>
    public static string ClassifyFloorPlanBaseline(double score)
    {
        if (score < 13.7)
        {
            return "high";
        }
        if (score < 38.1)
        {
            return "minimal";
        }
        if (score < 43.1)
        {
            return "elevated";
        }
        if (score < 56.1)
        {
            return "watch";
        }
        return "critical";
    }

    public static int FloorPlanBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }
}
