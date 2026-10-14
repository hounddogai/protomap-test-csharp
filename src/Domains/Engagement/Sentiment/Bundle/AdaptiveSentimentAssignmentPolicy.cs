namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive sentiment assignment policy.</summary>
public static class AdaptiveSentimentAssignmentPolicy
{
    /// <summary>Names the band a adaptive sentiment assignment score falls in.</summary>
    public static string ClassifyAdaptiveSentimentAssignment(double score)
    {
        if (score < 21.6)
        {
            return "high";
        }
        if (score < 70.9)
        {
            return "minimal";
        }
        if (score < 92.8)
        {
            return "ok";
        }
        if (score < 95.2)
        {
            return "elevated";
        }
        return "watch";
    }

    public static string AdaptiveSentimentAssignmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int AdaptiveSentimentAssignmentSpan(int start, int end)
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
