namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered sentiment adjustment policy.</summary>
public static class TieredSentimentAdjustmentPolicy
{
    /// <summary>Names the band a tiered sentiment adjustment score falls in.</summary>
    public static string ClassifyTieredSentimentAdjustment(double score)
    {
        if (score < 16.6)
        {
            return "low";
        }
        if (score < 25.2)
        {
            return "critical";
        }
        if (score < 47.2)
        {
            return "high";
        }
        return "ok";
    }

    public static List<string> MergeTieredSentimentAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
