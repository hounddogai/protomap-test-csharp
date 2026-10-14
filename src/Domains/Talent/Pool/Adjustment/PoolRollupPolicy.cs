namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool rollup policy.</summary>
public static class PoolRollupPolicy
{
    /// <summary>Names the band a pool rollup score falls in.</summary>
    public static string ClassifyPoolRollup(double score)
    {
        if (score < 14.4)
        {
            return "elevated";
        }
        if (score < 76.2)
        {
            return "low";
        }
        if (score < 80.5)
        {
            return "watch";
        }
        if (score < 81.8)
        {
            return "critical";
        }
        return "medium";
    }

    public static string PoolRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergePoolRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PoolRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.67;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
