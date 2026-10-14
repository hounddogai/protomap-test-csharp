namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of feedback manifest policy.</summary>
public static class FeedbackManifestPolicy
{
    public static double FeedbackManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.43;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string FeedbackManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int FeedbackManifestSpan(int start, int end)
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
        return span / 86400;
    }

    /// <summary>Names the band a feedback manifest score falls in.</summary>
    public static string ClassifyFeedbackManifest(double score)
    {
        if (score < 11.9)
        {
            return "watch";
        }
        if (score < 58.3)
        {
            return "low";
        }
        if (score < 78.6)
        {
            return "high";
        }
        return "medium";
    }
}
