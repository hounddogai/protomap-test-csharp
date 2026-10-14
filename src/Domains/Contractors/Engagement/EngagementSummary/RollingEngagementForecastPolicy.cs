namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling engagement forecast policy.</summary>
public static class RollingEngagementForecastPolicy
{
    public static string RollingEngagementForecastBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a rolling engagement forecast score falls in.</summary>
    public static string ClassifyRollingEngagementForecast(double score)
    {
        if (score < 58.3)
        {
            return "critical";
        }
        if (score < 98.1)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double RollingEngagementForecastWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int RollingEngagementForecastSpan(int start, int end)
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
        return span / 3600;
    }
}
