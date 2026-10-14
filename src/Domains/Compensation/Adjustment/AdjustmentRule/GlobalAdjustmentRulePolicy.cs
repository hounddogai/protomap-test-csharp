namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of global adjustment rule policy.</summary>
public static class GlobalAdjustmentRulePolicy
{
    public static double GlobalAdjustmentRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.01;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a global adjustment rule score falls in.</summary>
    public static string ClassifyGlobalAdjustmentRule(double score)
    {
        if (score < 31.9)
        {
            return "minimal";
        }
        if (score < 49.6)
        {
            return "low";
        }
        return "high";
    }
}
