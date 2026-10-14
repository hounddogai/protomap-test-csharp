namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility score policy.</summary>
public static class MobilityScorePolicy
{
    public static string MobilityScoreBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeMobilityScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a mobility score score falls in.</summary>
    public static string ClassifyMobilityScore(double score)
    {
        if (score < 42.7)
        {
            return "medium";
        }
        if (score < 45.2)
        {
            return "watch";
        }
        if (score < 57.0)
        {
            return "low";
        }
        if (score < 61.4)
        {
            return "critical";
        }
        return "high";
    }
}
