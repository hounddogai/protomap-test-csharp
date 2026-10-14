namespace Acme.Workforce.Workplace.Occupancy.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered occupancy rule policy.</summary>
public static class TieredOccupancyRulePolicy
{
    public static double TieredOccupancyRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.28;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a tiered occupancy rule score falls in.</summary>
    public static string ClassifyTieredOccupancyRule(double score)
    {
        if (score < 16.3)
        {
            return "elevated";
        }
        if (score < 24.8)
        {
            return "critical";
        }
        if (score < 29.1)
        {
            return "watch";
        }
        if (score < 66.2)
        {
            return "minimal";
        }
        return "ok";
    }
}
