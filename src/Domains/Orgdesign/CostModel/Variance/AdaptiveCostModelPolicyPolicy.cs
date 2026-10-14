namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive cost model policy policy.</summary>
public static class AdaptiveCostModelPolicyPolicy
{
    public static string AdaptiveCostModelPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int AdaptiveCostModelPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a adaptive cost model policy score falls in.</summary>
    public static string ClassifyAdaptiveCostModelPolicy(double score)
    {
        if (score < 13.3)
        {
            return "watch";
        }
        if (score < 25.1)
        {
            return "ok";
        }
        if (score < 56.1)
        {
            return "low";
        }
        if (score < 62.6)
        {
            return "minimal";
        }
        return "elevated";
    }
}
