namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient sentiment manifest policy.</summary>
public static class LenientSentimentManifestPolicy
{
    public static double LenientSentimentManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient sentiment manifest score falls in.</summary>
    public static string ClassifyLenientSentimentManifest(double score)
    {
        if (score < 13.2)
        {
            return "minimal";
        }
        if (score < 26.9)
        {
            return "elevated";
        }
        return "medium";
    }

    public static string LenientSentimentManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
