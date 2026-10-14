namespace Acme.Workforce.Compensation.BonusPool.PoolTier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of primary bonus pool tier policy.</summary>
public static class PrimaryBonusPoolTierPolicy
{
    public static List<string> MergePrimaryBonusPoolTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double PrimaryBonusPoolTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary bonus pool tier score falls in.</summary>
    public static string ClassifyPrimaryBonusPoolTier(double score)
    {
        if (score < 20.4)
        {
            return "critical";
        }
        if (score < 67.7)
        {
            return "high";
        }
        return "watch";
    }
}
