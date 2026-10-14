namespace Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Functions of pending sentiment digest policy.</summary>
public static class PendingSentimentDigestPolicy
{
    /// <summary>Names the band a pending sentiment digest score falls in.</summary>
    public static string ClassifyPendingSentimentDigest(double score)
    {
        if (score < 50.1)
        {
            return "minimal";
        }
        if (score < 74.7)
        {
            return "high";
        }
        return "ok";
    }

    public static int PendingSentimentDigestSpan(int start, int end)
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
