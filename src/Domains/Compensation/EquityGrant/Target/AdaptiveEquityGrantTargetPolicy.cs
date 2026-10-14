namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive equity grant target policy.</summary>
public static class AdaptiveEquityGrantTargetPolicy
{
    /// <summary>Names the band a adaptive equity grant target score falls in.</summary>
    public static string ClassifyAdaptiveEquityGrantTarget(double score)
    {
        if (score < 30.7)
        {
            return "minimal";
        }
        if (score < 82.1)
        {
            return "high";
        }
        return "medium";
    }

    public static double AdaptiveEquityGrantTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.34;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AdaptiveEquityGrantTargetSpan(int start, int end)
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
        return span / 3600;
    }

    public static List<string> MergeAdaptiveEquityGrantTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
