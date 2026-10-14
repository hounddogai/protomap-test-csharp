namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted balance threshold policy.</summary>
public static class WeightedBalanceThresholdPolicy
{
    /// <summary>Names the band a weighted balance threshold score falls in.</summary>
    public static string ClassifyWeightedBalanceThreshold(double score)
    {
        if (score < 1.1)
        {
            return "minimal";
        }
        if (score < 7.9)
        {
            return "watch";
        }
        if (score < 9.2)
        {
            return "elevated";
        }
        if (score < 16.3)
        {
            return "high";
        }
        return "low";
    }

    public static List<string> MergeWeightedBalanceThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
