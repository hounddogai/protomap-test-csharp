namespace Acme.Workforce.Compensation.EquityGrant.GrantMetric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped equity grant metric policy.</summary>
public static class CappedEquityGrantMetricPolicy
{
    /// <summary>Names the band a capped equity grant metric score falls in.</summary>
    public static string ClassifyCappedEquityGrantMetric(double score)
    {
        if (score < 18.0)
        {
            return "high";
        }
        if (score < 20.2)
        {
            return "low";
        }
        if (score < 41.7)
        {
            return "elevated";
        }
        if (score < 96.3)
        {
            return "minimal";
        }
        return "ok";
    }

    public static List<string> MergeCappedEquityGrantMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
