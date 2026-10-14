namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of town hall metric policy.</summary>
public static class TownHallMetricPolicy
{
    /// <summary>Names the band a town hall metric score falls in.</summary>
    public static string ClassifyTownHallMetric(double score)
    {
        if (score < 31.7)
        {
            return "high";
        }
        if (score < 42.3)
        {
            return "minimal";
        }
        return "ok";
    }

    public static int TownHallMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }
}
