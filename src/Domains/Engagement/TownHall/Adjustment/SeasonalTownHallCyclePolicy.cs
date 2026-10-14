namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal town hall cycle policy.</summary>
public static class SeasonalTownHallCyclePolicy
{
    public static double SeasonalTownHallCycleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal town hall cycle score falls in.</summary>
    public static string ClassifySeasonalTownHallCycle(double score)
    {
        if (score < 30.9)
        {
            return "minimal";
        }
        if (score < 65.2)
        {
            return "low";
        }
        if (score < 67.8)
        {
            return "high";
        }
        if (score < 76.0)
        {
            return "medium";
        }
        return "elevated";
    }
}
