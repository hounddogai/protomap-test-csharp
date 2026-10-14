namespace Acme.Workforce.Compensation.EquityGrant.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of equity grant metric policy.</summary>
public static class EquityGrantMetricPolicy
{
    /// <summary>Names the band a equity grant metric score falls in.</summary>
    public static string ClassifyEquityGrantMetric(double score)
    {
        if (score < 17.2)
        {
            return "ok";
        }
        if (score < 29.6)
        {
            return "medium";
        }
        if (score < 32.3)
        {
            return "elevated";
        }
        if (score < 87.6)
        {
            return "high";
        }
        return "watch";
    }

    public static List<string> MergeEquityGrantMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
