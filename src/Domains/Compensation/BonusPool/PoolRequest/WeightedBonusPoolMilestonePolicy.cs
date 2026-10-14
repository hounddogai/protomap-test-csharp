namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted bonus pool milestone policy.</summary>
public static class WeightedBonusPoolMilestonePolicy
{
    public static List<string> MergeWeightedBonusPoolMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int WeightedBonusPoolMilestoneSpan(int start, int end)
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
        return span / 60;
    }

    public static string WeightedBonusPoolMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Names the band a weighted bonus pool milestone score falls in.</summary>
    public static string ClassifyWeightedBonusPoolMilestone(double score)
    {
        if (score < 1.7)
        {
            return "critical";
        }
        if (score < 78.7)
        {
            return "minimal";
        }
        if (score < 79.0)
        {
            return "watch";
        }
        return "ok";
    }
}
