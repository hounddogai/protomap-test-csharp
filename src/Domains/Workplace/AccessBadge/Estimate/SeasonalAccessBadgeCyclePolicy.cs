namespace Acme.Workforce.Workplace.AccessBadge.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal access badge cycle policy.</summary>
public static class SeasonalAccessBadgeCyclePolicy
{
    /// <summary>Names the band a seasonal access badge cycle score falls in.</summary>
    public static string ClassifySeasonalAccessBadgeCycle(double score)
    {
        if (score < 40.6)
        {
            return "minimal";
        }
        if (score < 64.1)
        {
            return "high";
        }
        return "medium";
    }

    public static double SeasonalAccessBadgeCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.83;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
