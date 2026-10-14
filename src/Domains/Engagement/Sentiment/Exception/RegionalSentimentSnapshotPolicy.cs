namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional sentiment snapshot policy.</summary>
public static class RegionalSentimentSnapshotPolicy
{
    public static double RegionalSentimentSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeRegionalSentimentSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a regional sentiment snapshot score falls in.</summary>
    public static string ClassifyRegionalSentimentSnapshot(double score)
    {
        if (score < 43.7)
        {
            return "elevated";
        }
        if (score < 74.3)
        {
            return "high";
        }
        if (score < 99.3)
        {
            return "ok";
        }
        return "minimal";
    }
}
