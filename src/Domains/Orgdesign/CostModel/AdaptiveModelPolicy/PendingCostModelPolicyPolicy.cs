namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pending cost model policy policy.</summary>
public static class PendingCostModelPolicyPolicy
{
    public static int PendingCostModelPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a pending cost model policy score falls in.</summary>
    public static string ClassifyPendingCostModelPolicy(double score)
    {
        if (score < 13.5)
        {
            return "elevated";
        }
        if (score < 93.2)
        {
            return "critical";
        }
        return "low";
    }

    public static double PendingCostModelPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
