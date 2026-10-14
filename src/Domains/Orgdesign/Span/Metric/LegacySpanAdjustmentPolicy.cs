namespace Acme.Workforce.Orgdesign.Span.Metric;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of legacy span adjustment policy.</summary>
public static class LegacySpanAdjustmentPolicy
{
    /// <summary>Names the band a legacy span adjustment score falls in.</summary>
    public static string ClassifyLegacySpanAdjustment(double score)
    {
        if (score < 34.0)
        {
            return "critical";
        }
        if (score < 65.5)
        {
            return "ok";
        }
        if (score < 86.3)
        {
            return "low";
        }
        return "medium";
    }

    public static List<string> MergeLegacySpanAdjustmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
