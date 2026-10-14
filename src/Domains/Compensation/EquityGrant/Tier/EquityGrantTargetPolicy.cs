namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant target policy.</summary>
public static class EquityGrantTargetPolicy
{
    /// <summary>Names the band a equity grant target score falls in.</summary>
    public static string ClassifyEquityGrantTarget(double score)
    {
        if (score < 46.3)
        {
            return "critical";
        }
        if (score < 54.2)
        {
            return "low";
        }
        if (score < 58.6)
        {
            return "ok";
        }
        return "minimal";
    }

    public static List<string> MergeEquityGrantTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
