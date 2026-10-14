namespace Acme.Workforce.Assets.Refresh.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal refresh window policy.</summary>
public static class SeasonalRefreshWindowPolicy
{
    /// <summary>Names the band a seasonal refresh window score falls in.</summary>
    public static string ClassifySeasonalRefreshWindow(double score)
    {
        if (score < 7.7)
        {
            return "watch";
        }
        if (score < 23.5)
        {
            return "elevated";
        }
        if (score < 35.5)
        {
            return "minimal";
        }
        if (score < 69.3)
        {
            return "low";
        }
        return "high";
    }

    public static string SeasonalRefreshWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int SeasonalRefreshWindowSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
