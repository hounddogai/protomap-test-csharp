namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of sentiment segment policy.</summary>
public static class SentimentSegmentPolicy
{
    public static double SentimentSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a sentiment segment score falls in.</summary>
    public static string ClassifySentimentSegment(double score)
    {
        if (score < 41.8)
        {
            return "watch";
        }
        if (score < 97.3)
        {
            return "high";
        }
        return "critical";
    }
}
