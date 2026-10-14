namespace Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Functions of sentiment threshold policy.</summary>
public static class SentimentThresholdPolicy
{
    /// <summary>Names the band a sentiment threshold score falls in.</summary>
    public static string ClassifySentimentThreshold(double score)
    {
        if (score < 17.4)
        {
            return "low";
        }
        if (score < 20.5)
        {
            return "critical";
        }
        if (score < 31.0)
        {
            return "high";
        }
        if (score < 54.4)
        {
            return "medium";
        }
        return "elevated";
    }

    public static int SentimentThresholdSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
