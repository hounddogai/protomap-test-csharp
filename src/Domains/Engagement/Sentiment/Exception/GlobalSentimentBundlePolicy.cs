namespace Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Functions of global sentiment bundle policy.</summary>
public static class GlobalSentimentBundlePolicy
{
    /// <summary>Names the band a global sentiment bundle score falls in.</summary>
    public static string ClassifyGlobalSentimentBundle(double score)
    {
        if (score < 1.3)
        {
            return "medium";
        }
        if (score < 11.3)
        {
            return "minimal";
        }
        if (score < 82.0)
        {
            return "critical";
        }
        return "high";
    }

    public static int GlobalSentimentBundleSpan(int start, int end)
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
        return span / 86400;
    }
}
