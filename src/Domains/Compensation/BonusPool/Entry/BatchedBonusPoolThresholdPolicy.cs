namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched bonus pool threshold policy.</summary>
public static class BatchedBonusPoolThresholdPolicy
{
    public static List<string> MergeBatchedBonusPoolThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string BatchedBonusPoolThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a batched bonus pool threshold score falls in.</summary>
    public static string ClassifyBatchedBonusPoolThreshold(double score)
    {
        if (score < 76.7)
        {
            return "low";
        }
        if (score < 87.0)
        {
            return "medium";
        }
        if (score < 87.4)
        {
            return "ok";
        }
        if (score < 93.4)
        {
            return "minimal";
        }
        return "critical";
    }
}
