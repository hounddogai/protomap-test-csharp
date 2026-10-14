namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment bundle policy.</summary>
public static class SentimentBundlePolicy
{
    /// <summary>Names the band a sentiment bundle score falls in.</summary>
    public static string ClassifySentimentBundle(double score)
    {
        if (score < 1.9)
        {
            return "high";
        }
        if (score < 70.7)
        {
            return "critical";
        }
        return "watch";
    }

    public static double SentimentBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
