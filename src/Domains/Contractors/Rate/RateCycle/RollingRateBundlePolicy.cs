namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling rate bundle policy.</summary>
public static class RollingRateBundlePolicy
{
    public static List<string> MergeRollingRateBundleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a rolling rate bundle score falls in.</summary>
    public static string ClassifyRollingRateBundle(double score)
    {
        if (score < 20.3)
        {
            return "minimal";
        }
        if (score < 70.0)
        {
            return "ok";
        }
        if (score < 73.9)
        {
            return "critical";
        }
        if (score < 76.4)
        {
            return "elevated";
        }
        return "watch";
    }
}
