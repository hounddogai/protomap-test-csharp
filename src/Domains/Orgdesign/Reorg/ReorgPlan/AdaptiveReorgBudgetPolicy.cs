namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive reorg budget policy.</summary>
public static class AdaptiveReorgBudgetPolicy
{
    /// <summary>Names the band a adaptive reorg budget score falls in.</summary>
    public static string ClassifyAdaptiveReorgBudget(double score)
    {
        if (score < 47.7)
        {
            return "medium";
        }
        if (score < 94.7)
        {
            return "low";
        }
        if (score < 97.5)
        {
            return "ok";
        }
        return "critical";
    }

    public static string AdaptiveReorgBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int AdaptiveReorgBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
