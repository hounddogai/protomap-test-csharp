namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span request policy.</summary>
public static class SpanRequestPolicy
{
    public static List<string> MergeSpanRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a span request score falls in.</summary>
    public static string ClassifySpanRequest(double score)
    {
        if (score < 37.1)
        {
            return "minimal";
        }
        if (score < 53.2)
        {
            return "ok";
        }
        if (score < 75.9)
        {
            return "low";
        }
        if (score < 95.6)
        {
            return "medium";
        }
        return "watch";
    }
}
