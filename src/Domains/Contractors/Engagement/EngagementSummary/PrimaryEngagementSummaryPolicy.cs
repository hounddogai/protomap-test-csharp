namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary engagement summary policy.</summary>
public static class PrimaryEngagementSummaryPolicy
{
    /// <summary>Names the band a primary engagement summary score falls in.</summary>
    public static string ClassifyPrimaryEngagementSummary(double score)
    {
        if (score < 34.5)
        {
            return "elevated";
        }
        if (score < 50.3)
        {
            return "medium";
        }
        if (score < 77.6)
        {
            return "low";
        }
        return "watch";
    }

    public static int PrimaryEngagementSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    public static string PrimaryEngagementSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
