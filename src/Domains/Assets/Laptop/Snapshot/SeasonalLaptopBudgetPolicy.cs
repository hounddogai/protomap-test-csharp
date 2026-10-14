namespace Acme.Workforce.Assets.Laptop.Snapshot;

/// <summary>Functions of seasonal laptop budget policy.</summary>
public static class SeasonalLaptopBudgetPolicy
{
    /// <summary>Names the band a seasonal laptop budget score falls in.</summary>
    public static string ClassifySeasonalLaptopBudget(double score)
    {
        if (score < 18.2)
        {
            return "high";
        }
        if (score < 21.4)
        {
            return "minimal";
        }
        if (score < 99.7)
        {
            return "medium";
        }
        return "critical";
    }

    public static int SeasonalLaptopBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
