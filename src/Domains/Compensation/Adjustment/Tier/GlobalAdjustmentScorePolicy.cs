namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global adjustment score policy.</summary>
public static class GlobalAdjustmentScorePolicy
{
    public static List<string> MergeGlobalAdjustmentScoreLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a global adjustment score score falls in.</summary>
    public static string ClassifyGlobalAdjustmentScore(double score)
    {
        if (score < 7.4)
        {
            return "watch";
        }
        if (score < 20.6)
        {
            return "ok";
        }
        if (score < 52.6)
        {
            return "medium";
        }
        if (score < 65.4)
        {
            return "high";
        }
        return "critical";
    }

    public static double GlobalAdjustmentScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
