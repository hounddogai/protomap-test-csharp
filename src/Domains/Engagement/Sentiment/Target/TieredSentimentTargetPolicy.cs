namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered sentiment target policy.</summary>
public static class TieredSentimentTargetPolicy
{
    public static string TieredSentimentTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a tiered sentiment target score falls in.</summary>
    public static string ClassifyTieredSentimentTarget(double score)
    {
        if (score < 48.9)
        {
            return "high";
        }
        if (score < 51.1)
        {
            return "minimal";
        }
        if (score < 51.9)
        {
            return "critical";
        }
        if (score < 93.0)
        {
            return "elevated";
        }
        return "low";
    }
}
