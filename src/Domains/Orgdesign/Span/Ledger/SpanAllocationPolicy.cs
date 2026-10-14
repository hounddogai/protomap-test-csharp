namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span allocation policy.</summary>
public static class SpanAllocationPolicy
{
    public static List<string> MergeSpanAllocationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a span allocation score falls in.</summary>
    public static string ClassifySpanAllocation(double score)
    {
        if (score < 40.3)
        {
            return "ok";
        }
        if (score < 75.0)
        {
            return "minimal";
        }
        return "critical";
    }
}
