namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span notice policy.</summary>
public static class SpanNoticePolicy
{
    public static List<string> MergeSpanNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a span notice score falls in.</summary>
    public static string ClassifySpanNotice(double score)
    {
        if (score < 58.1)
        {
            return "high";
        }
        if (score < 75.0)
        {
            return "watch";
        }
        return "ok";
    }
}
