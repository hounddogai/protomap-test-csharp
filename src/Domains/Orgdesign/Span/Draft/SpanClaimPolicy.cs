namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span claim policy.</summary>
public static class SpanClaimPolicy
{
    /// <summary>Names the band a span claim score falls in.</summary>
    public static string ClassifySpanClaim(double score)
    {
        if (score < 2.4)
        {
            return "watch";
        }
        if (score < 99.9)
        {
            return "low";
        }
        return "minimal";
    }

    public static List<string> MergeSpanClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
